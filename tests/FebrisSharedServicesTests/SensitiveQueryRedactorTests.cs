// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Audit H-26, a cut line A publication blocker: a password-reset token reachable by any Org
    /// Admin, forever.
    ///
    /// <para>
    /// ASP.NET Identity puts its reset and confirmation tokens in the query string of the emailed
    /// link. The analytics middleware stored <c>Request.QueryString</c> verbatim on every request,
    /// <c>LocalAnalyticsController</c> is <c>[Authorize(Roles = OrgAdmins)]</c>, and the views
    /// render <c>Query</c> directly. Clicking your own reset link handed a live account-takeover
    /// credential to every admin on the node.
    /// </para>
    ///
    /// <para>
    /// The two halves of the contract are equally important, and the second is the one that is easy
    /// to break by accident: secrets must go, and everything else must SURVIVE VERBATIM, because
    /// the stored query has a real reader -- the bot/attack heuristic scans it for fingerprints
    /// like <c>SELECT</c>, <c>.git</c> and <c>.env</c>.
    /// </para>
    /// </summary>
    public class SensitiveQueryRedactorTests
    {
        [Theory]
        [InlineData("?code=CfDJ8ABCDEF", "?code=[REDACTED]")]
        [InlineData("?Code=CfDJ8ABCDEF", "?Code=[REDACTED]")]                     // key match is case-insensitive
        [InlineData("?CODE=CfDJ8ABCDEF", "?CODE=[REDACTED]")]
        [InlineData("?token=abc", "?token=[REDACTED]")]
        [InlineData("?access_token=abc", "?access_token=[REDACTED]")]
        [InlineData("?password=hunter2", "?password=[REDACTED]")]
        public void RedactsTheValueOfASecretKey(string input, string expected)
        {
            SensitiveQueryRedactor.Redact(input).Should().Be(expected);
        }

        [Fact]
        public void RedactsTheRealPasswordResetLinkShape()
        {
            // Exactly what ForgotPassword.cshtml.cs builds, with the userId that ConfirmEmail adds.
            string actual = SensitiveQueryRedactor.Redact(
                "?userId=8f14e45f-ceea-467a-9575-28d3e0f7d4c1&code=CfDJ8Nv7bQ2xUx1PmQ.veryLongTokenBody");

            actual.Should().Be("?userId=8f14e45f-ceea-467a-9575-28d3e0f7d4c1&code=[REDACTED]");
            actual.Should().NotContain("CfDJ8", "no fragment of the token may survive");
        }

        [Theory]
        [InlineData("?page=2&sort=name")]
        [InlineData("?includeArchived=true")]
        [InlineData("?id=5")]
        public void LeavesOrdinaryParametersCompletelyAlone(string input)
        {
            SensitiveQueryRedactor.Redact(input).Should().Be(input);
        }

        [Fact]
        public void PreservesTheAttackFingerprintsTheBotHeuristicLooksFor()
        {
            // The read side. AnalyticsLogic scans the stored Query for these substrings, so a
            // redactor that blanked the field would silently disable attack detection.
            string[] probes =
            {
                "?q=SELECT * FROM users",
                "?file=../../.env",
                "?path=/wp-includes/wlwmanifest.xml",
                "?repo=.git/config"
            };

            foreach (string probe in probes)
            {
                SensitiveQueryRedactor.Redact(probe).Should().Be(probe, "attack fingerprints must survive redaction");
            }
        }

        [Fact]
        public void MatchesKeysExactly_NotBySubstring()
        {
            // "barcode" contains "code" and "tokenCount" contains "token". Neither is a credential,
            // and over-redacting would quietly degrade analytics.
            SensitiveQueryRedactor.Redact("?barcode=12345").Should().Be("?barcode=12345");
            SensitiveQueryRedactor.Redact("?tokenCount=3").Should().Be("?tokenCount=3");
        }

        [Fact]
        public void RedactsEverySensitiveKeyWhenThereAreSeveral()
        {
            SensitiveQueryRedactor.Redact("?code=a&page=1&token=b")
                .Should().Be("?code=[REDACTED]&page=1&token=[REDACTED]");
        }

        [Fact]
        public void RedactsRepeatedKeys()
        {
            SensitiveQueryRedactor.Redact("?code=a&code=b")
                .Should().Be("?code=[REDACTED]&code=[REDACTED]");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("?")]
        public void HandlesEmptyInput(string input)
        {
            SensitiveQueryRedactor.Redact(input).Should().Be(input);
        }

        [Fact]
        public void HandlesAQueryWithNoLeadingQuestionMark()
        {
            SensitiveQueryRedactor.Redact("code=abc&page=1").Should().Be("code=[REDACTED]&page=1");
        }

        [Theory]
        [InlineData("?flag", "?flag")]                       // bare key, no value to leak
        [InlineData("?code", "?code")]                       // sensitive key with no value
        [InlineData("?code=", "?code=[REDACTED]")]           // present but empty
        [InlineData("?a=1&&b=2", "?a=1&&b=2")]               // empty segment
        [InlineData("?=novalue", "?=novalue")]               // empty key
        public void SurvivesMalformedInput(string input, string expected)
        {
            // The input is attacker-controlled and this runs inside a fire-and-forget task, so it
            // must never throw.
            SensitiveQueryRedactor.Redact(input).Should().Be(expected);
        }

        [Fact]
        public void SensitiveKeys_IncludeTheIdentityTokenParameter()
        {
            // "code" is the whole point: it is what ASP.NET Identity names both the password-reset
            // and the email-confirmation token.
            SensitiveQueryRedactor.SensitiveKeys.Should().Contain("code");
        }
    }
}
