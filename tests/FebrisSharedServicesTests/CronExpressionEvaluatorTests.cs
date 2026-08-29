// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Threading.Tasks;
using Febris.SharedServices;
using FluentAssertions;
using NCrontab;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="CronExpressionEvaluator"/> and the <see cref="CronExpression"/>
    /// constants it is paired with.
    ///
    /// <para>
    /// Both evaluator methods swallow parse errors silently:
    /// <list type="bullet">
    ///   <item><see cref="CronExpressionEvaluator.GetNextOccurrence"/> returns <c>DateTime.UtcNow</c> on any exception.</item>
    ///   <item><see cref="CronExpressionEvaluator.GetExpectedRunTimeBasedOnLastRun"/> returns <c>default(DateTime)</c> (i.e., <c>DateTime.MinValue</c>) on any exception.</item>
    /// </list>
    /// These tests pin that current behavior so any future refactor that changes it
    /// shows up as a failing test. See <c>..\BUGS.md</c> for the known issue with
    /// <see cref="CronExpression.EverySecond"/> (6-field expression that the underlying
    /// 5-field parser cannot handle).
    /// </para>
    /// </summary>
    public class CronExpressionEvaluatorTests
    {
        // WHY THESE READ THE CLOCK TWICE (2026-08-26).
        //
        // GetNextOccurrence calls DateTime.UtcNow ITSELF, at some instant T1 at or after the
        // `before` this test captures at T0. (Not strictly after: two back-to-back UtcNow reads
        // return the identical value about two thirds of the time on this box.) The returned value
        // is the first cron boundary strictly after T1, not after T0. Bounding it against T0 plus
        // one period is therefore wrong whenever T0 and T1 straddle a boundary.
        //
        // The algebra for EveryMinute. Let s be the seconds-into-minute at T0 and delta = T1 - T0.
        // If T0 and T1 straddle a boundary (delta >= 60 - s -- NCrontab is strictly-after, so T1
        // landing exactly ON the boundary already straddles), the answer is
        // boundary(T0) + 60 = T0 + 120 - s, while the old assertion demanded it be strictly under
        // T0 + 61. That fails when 120 - s >= 61, i.e. s <= 59.
        //
        // Both conditions together put s in the CLOSED interval [60 - delta, 59], which is empty
        // unless delta is at least one second, and p(fail) = (delta - 1) / 60. At delta = 2s that
        // is roughly one run in 60, at 5s about one in 15, and at any delta under 1s it cannot
        // happen at all. (Beyond delta = 60 every s fails, but that is far outside the regime
        // here.) Brute-forced against the real NCrontab in 1 ms steps to confirm.
        //
        // So this is not microsecond jitter: it needs the call to take a full second or more.
        // Note it CANNOT be thread-pool starvation. GetNextOccurrence is declared async but has no
        // await in its body, so it completes synchronously on the calling thread and the await here
        // resumes inline with no continuation ever queued. A stall of that size on this path is the
        // running thread being preempted under CPU oversubscription, or a long GC pause -- which a
        // back-to-back sweep of all seven suites can produce and an isolated rerun does not. That
        // matches the reported signature: only ever during full-suite runs, never on rerun.
        // See docs/BUGS.md.
        //
        // The fix is to bound against a clock read taken AFTER the call. Since T1 lies in
        // [before, after], the first boundary after T1 is at most after + one period, and is
        // always strictly later than before. BeOnOrBefore rather than BeBefore is load-bearing:
        // when T1 lands exactly on a boundary, next equals after + one period exactly.
        //
        // Residual, deliberately not fixed here: DateTime.UtcNow is wall time and is NOT monotonic.
        // A backward clock step (NTP correction, manual set) landing between the two reads puts T1
        // outside [before, after] and breaks both bounds. Removing that would need Stopwatch or an
        // injected clock, which is a change to the production signature rather than to these tests.

        [Fact]
        public async Task GetNextOccurrence_WithEveryMinute_ReturnsTimeWithinTheNextMinute()
        {
            // EveryMinute = "0/1 * * * *" -> at the start of every minute.
            var before = DateTime.UtcNow;

            var next = await CronExpressionEvaluator.GetNextOccurrence(CronExpression.EveryMinute);

            var after = DateTime.UtcNow;

            next.Should().BeAfter(before);
            next.Should().BeOnOrBefore(after.AddMinutes(1));
        }

        [Fact]
        public async Task GetNextOccurrence_WithEveryHour_ReturnsTimeWithinTheNextHour()
        {
            // EveryHour = "0 * * * *" -> at minute 0 of every hour. Same straddle, but the danger
            // window is the last second of an HOUR, so p = (delta - 1) / 3600. That is 1/60 as
            // likely as the minute case, NOT 1/3600 as an earlier version of this comment said.
            // (The two get confused because the ABSOLUTE figure at delta = 2s is about 1 in 3600,
            // which is 1/60 of 1/60.)
            var before = DateTime.UtcNow;

            var next = await CronExpressionEvaluator.GetNextOccurrence(CronExpression.EveryHour);

            var after = DateTime.UtcNow;

            next.Should().BeAfter(before);
            next.Should().BeOnOrBefore(after.AddHours(1));
        }

        [Fact]
        public void TheBoundaryStraddle_IsWhatUsedToFail_PinnedWithoutTheClock()
        {
            // Pure arithmetic, so it cannot flake in either direction.
            //
            // T0 at 12:00:58.000 (s = 58). The awaited call takes 3 seconds under a loaded sweep,
            // so the evaluator reads ITS clock at T1 = 12:01:01.000, and the first minute boundary
            // after that is 12:02:00.000.
            var t0 = new DateTime(2026, 1, 1, 12, 0, 58, 0, DateTimeKind.Utc);
            var t1 = t0.AddSeconds(3);

            // Ask the REAL scheduler, rather than hard-coding the answer. This is what makes the
            // test exercise the library the evaluator uses instead of merely restating arithmetic.
            var next = CrontabSchedule.Parse(CronExpression.EveryMinute).GetNextOccurrence(t1);

            next.Should().Be(new DateTime(2026, 1, 1, 12, 2, 0, 0),
                "NCrontab is strictly-after, so 12:01:01 yields the 12:02 boundary");

            // The OLD bound was T0 + 1min + 1s = 12:01:59.000, and the assertion was
            // next < bound. A correct answer of 12:02:00 sits one second PAST it, so the
            // assertion rejected a correct answer. That is the whole defect.
            var oldBound = t0.AddMinutes(1).AddSeconds(1);
            oldBound.Should().Be(new DateTime(2026, 1, 1, 12, 1, 59, 0, DateTimeKind.Utc));
            next.Should().BeOnOrAfter(oldBound,
                "a correct answer lands at or past the old bound, which is why the old assertion flaked");

            // The NEW bound is taken from a clock read at or after T1, so it accepts it.
            var after = t1;
            next.Should().BeOnOrBefore(after.AddMinutes(1));
            next.Should().BeAfter(t0);
        }

        [Fact]
        public async Task GetNextOccurrence_WithInvalidExpression_ReturnsCurrentTime()
        {
            // The implementation swallows the parse exception and returns DateTime.UtcNow.
            // The returned value should be approximately "now", not far in the future.
            var before = DateTime.UtcNow;

            var result = await CronExpressionEvaluator.GetNextOccurrence("totally not a cron expression");

            result.Should().BeOnOrAfter(before);
            result.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public async Task GetNextOccurrence_WithNull_ReturnsCurrentTime()
        {
            // Null input -> CrontabSchedule.Parse throws -> caught -> DateTime.UtcNow returned.
            var before = DateTime.UtcNow;

            var result = await CronExpressionEvaluator.GetNextOccurrence(null);

            result.Should().BeOnOrAfter(before);
            result.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public async Task GetNextOccurrence_WithEverySecond_ReturnsCurrentTime_DocumentsKnownBug()
        {
            // KNOWN BUG (see ..\BUGS.md): CronExpression.EverySecond = "* * * * * *" is a
            // 6-field expression (with seconds). CrontabSchedule.Parse defaults to 5-field
            // mode and throws on this input, which the evaluator silently turns into
            // DateTime.UtcNow. The constant is therefore unusable with the current evaluator.
            //
            // This test pins the broken behavior so any future fix (either parsing as
            // 6-field, or surfacing the exception) shows up as a failing test that should
            // be updated alongside the fix.
            var before = DateTime.UtcNow;

            var result = await CronExpressionEvaluator.GetNextOccurrence(CronExpression.EverySecond);

            result.Should().BeOnOrAfter(before);
            result.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public async Task GetExpectedRunTimeBasedOnLastRun_WithValidExpression_ReturnsNextOccurrenceAfterGivenDate()
        {
            // Anchor: an arbitrary midnight UTC.
            var anchor = new DateTime(2026, 5, 19, 0, 0, 0, DateTimeKind.Utc);

            var result = await CronExpressionEvaluator.GetExpectedRunTimeBasedOnLastRun(anchor, CronExpression.EveryHour);

            // EveryHour after 00:00 should be 01:00 UTC.
            result.Should().Be(new DateTime(2026, 5, 19, 1, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public async Task GetExpectedRunTimeBasedOnLastRun_WithInvalidExpression_ReturnsDefault()
        {
            // The implementation returns default(DateTime) == DateTime.MinValue on parse failure,
            // which is a notable departure from GetNextOccurrence's "return UtcNow" behavior.
            var anchor = DateTime.UtcNow;

            var result = await CronExpressionEvaluator.GetExpectedRunTimeBasedOnLastRun(anchor, "not a cron");

            result.Should().Be(default(DateTime));
            result.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public async Task GetExpectedRunTimeBasedOnLastRun_WithNull_ReturnsDefault()
        {
            var result = await CronExpressionEvaluator.GetExpectedRunTimeBasedOnLastRun(DateTime.UtcNow, null);

            result.Should().Be(default(DateTime));
        }

        [Theory]
        // Each constant must be a non-empty string. The actual "is this valid cron?" check
        // is in CronExpression_FiveFieldConstantsParseSuccessfully below.
        [InlineData("EveryMinute")]
        [InlineData("EveryFiveMinutes")]
        [InlineData("EveryTenMinutes")]
        [InlineData("EveryFifteenMinutes")]
        [InlineData("EveryTwentyMinutes")]
        [InlineData("EveryHour")]
        [InlineData("EveryDay")]
        [InlineData("EveryWeek")]
        [InlineData("EveryMonth")]
        public void CronExpression_Constants_AreNonEmpty(string fieldName)
        {
            var field = typeof(CronExpression).GetField(fieldName);
            field.Should().NotBeNull("the constant " + fieldName + " should exist on CronExpression");

            var value = (string)field.GetValue(null);
            value.Should().NotBeNullOrEmpty();
        }

        [Theory]
        // All five-field constants must produce a valid future time when evaluated.
        // (EverySecond is excluded -- see the "known bug" test above.)
        [InlineData("0/1 * * * *")]   // EveryMinute
        [InlineData("*/5 * * * *")]   // EveryFiveMinutes
        [InlineData("*/10 * * * *")]  // EveryTenMinutes
        [InlineData("*/15 * * * *")]  // EveryFifteenMinutes
        [InlineData("*/20 * * * *")]  // EveryTwentyMinutes
        [InlineData("0 * * * *")]     // EveryHour
        [InlineData("0 0 * * *")]     // EveryDay
        [InlineData("0 0 * * 0")]     // EveryWeek
        [InlineData("0 0 1 * *")]     // EveryMonth
        public async Task CronExpression_FiveFieldConstants_ParseAndProduceFutureTime(string expression)
        {
            var before = DateTime.UtcNow;

            var next = await CronExpressionEvaluator.GetNextOccurrence(expression);

            // CORRECTED 2026-08-26. This used to read `BeAfter(before.AddMilliseconds(-1))`, a
            // tolerance added because the evaluator can return within the clock's resolution.
            // That fudge defeated the test: a swallowed parse failure returns DateTime.UtcNow,
            // which is ALSO after before-1ms, so the assertion could no longer detect the one
            // thing it exists to detect.
            //
            // Discriminate on shape instead of on timing. Every 5-field cron boundary lands
            // exactly on a minute, so seconds and milliseconds are zero. DateTime.UtcNow
            // effectively never is, which is what makes this a real check rather than a
            // tolerance. It is also fully deterministic under load.
            next.Second.Should().Be(0, "a 5-field cron boundary always lands on a whole minute");
            next.Millisecond.Should().Be(0, "a swallowed parse failure returns DateTime.UtcNow, which does not");
            next.Should().BeAfter(before);
        }
    }
}
