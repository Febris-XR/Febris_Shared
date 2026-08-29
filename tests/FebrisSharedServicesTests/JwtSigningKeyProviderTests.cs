// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Febris.SharedServices.Tests
{
    // TEST-B2 (2026-08-26). Three classes -- JwtSigningKeyProviderTests,
    // JwtSigningKeyProviderAsymmetricTests and JwtSigningKeyProviderPublisherOnlyTests -- each
    // mutate the PROCESS-GLOBAL environment variables FEBRIS_JWT_SIGNING_SECRET,
    // FEBRIS_JWT_PRIVATE_KEY_PEM and FEBRIS_JWT_KEY_ID in their constructors and restore them on
    // Dispose. None of them carried a [Collection] attribute, and this project has no
    // xunit.runner.json, so xUnit ran all three IN PARALLEL as separate collections.
    //
    // JwtSigningKeyProvider reads those variables when it resolves a key
    // (JwtSigningKeyProvider.cs:170 onward), so one class's constructor or Dispose could change
    // the value out from under another class's assertion. That is a genuine intra-process race in
    // the suite that flaked twice, and it is the same shape the SerilogStartupValidator collection
    // below was created to fix (cf. TEST-B1).
    //
    // Putting all three in one collection makes xUnit run them sequentially. See docs/BUGS.md.
    [CollectionDefinition("JwtSigningKeyProviderEnv", DisableParallelization = true)]
    public class JwtSigningKeyProviderEnvCollection { }

    /// <summary>
    /// Tests for <see cref="JwtSigningKeyProvider"/>.
    ///
    /// <para>
    /// The provider centralizes JWT signing-key resolution across the Febris API
    /// hosts. It is the single audit-traceable point where a misconfigured secret
    /// (unsubstituted Helm/Octopus template placeholder, under-strength key,
    /// missing value) fails the host at startup instead of failing on the first
    /// authentication attempt.
    /// </para>
    ///
    /// <para>
    /// Resolution precedence (verified below): env var > config. Validation rules
    /// are environment-aware: Development is permissive so a local-dev appsettings
    /// with a 32-byte literal secret keeps working; non-Development is strict and
    /// rejects unsubstituted templates plus under-strength keys.
    /// </para>
    /// </summary>
    [Collection("JwtSigningKeyProviderEnv")]
    public class JwtSigningKeyProviderTests : IDisposable
    {
        // 32-char ASCII = 32 bytes = 256 bits, the HMAC-SHA256 floor. Matches
        // the existing appsettings.Development.json placeholder so prod-mode
        // tests with this value scrape by the length check.
        private const string MinViableSecret = "abcdefghijklmnopqrstuvwxyz012345";

        // 64-char ASCII = 512 bits, comfortably above the floor; mirrors what a
        // real production secret should look like.
        private const string StrongSecret = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@";

        // Tracks env-var state across the test so we restore exactly what the
        // process started with -- avoids leaking state into adjacent tests if
        // the runner reuses an AppDomain.
        private readonly string _originalEnvValue;

        public JwtSigningKeyProviderTests()
        {
            _originalEnvValue = Environment.GetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName);
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName, null);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName, _originalEnvValue);
        }

        private static IConfiguration ConfigWith(string secretValue)
        {
            var mock = new Mock<IConfiguration>();
            mock.Setup(c => c["JwtSettings:Secret"]).Returns(secretValue);
            return mock.Object;
        }

        // --- Resolution precedence ----------------------------------------------------------

        [Fact]
        public void EnvVar_TakesPrecedenceOverConfig()
        {
            // Both set, env-var should win. This is the deploy-time-rotation pattern.
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName, StrongSecret);
            var provider = new JwtSigningKeyProvider(ConfigWith("CONFIG_VALUE_THAT_SHOULD_BE_IGNORED_LONG_ENOUGH"), isDevelopment: false);

            provider.GetSecret().Should().Be(StrongSecret);
        }

        [Fact]
        public void Config_UsedWhenEnvVarMissing()
        {
            // No env var, config wins. Legacy/dev path.
            var provider = new JwtSigningKeyProvider(ConfigWith(StrongSecret), isDevelopment: false);

            provider.GetSecret().Should().Be(StrongSecret);
        }

        [Fact]
        public void Config_UsedWhenEnvVarIsEmpty()
        {
            // Empty env var is treated as "not set" -- per the IsNullOrWhiteSpace check.
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName, "");
            var provider = new JwtSigningKeyProvider(ConfigWith(StrongSecret), isDevelopment: false);

            provider.GetSecret().Should().Be(StrongSecret);
        }

        [Fact]
        public void Config_UsedWhenEnvVarIsWhitespace()
        {
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName, "   ");
            var provider = new JwtSigningKeyProvider(ConfigWith(StrongSecret), isDevelopment: false);

            provider.GetSecret().Should().Be(StrongSecret);
        }

        // --- Validation: missing / empty --------------------------------------------------

        [Fact]
        public void NullSecret_ThrowsInDevelopment()
        {
            // Even Development requires *some* secret; we can't HMAC-sign with nothing.
            Action act = () => new JwtSigningKeyProvider(ConfigWith(null), isDevelopment: true);

            act.Should().Throw<InvalidOperationException>()
               .Where(e => e.Message.Contains("not configured"));
        }

        [Fact]
        public void NullSecret_ThrowsInProduction()
        {
            Action act = () => new JwtSigningKeyProvider(ConfigWith(null), isDevelopment: false);

            act.Should().Throw<InvalidOperationException>()
               .Where(e => e.Message.Contains("not configured"));
        }

        [Fact]
        public void EmptySecret_ThrowsInProduction()
        {
            Action act = () => new JwtSigningKeyProvider(ConfigWith(""), isDevelopment: false);

            act.Should().Throw<InvalidOperationException>()
               .Where(e => e.Message.Contains("not configured"));
        }

        // --- Validation: unsubstituted templates (non-Development only) -------------------

        [Theory]
        [InlineData("{JwtTokenSecret}")]
        [InlineData("{DataDBConnectionString}")]
        [InlineData("{Anything}")]
        public void UnsubstitutedTemplate_ThrowsInProduction(string templateValue)
        {
            // Helm / Octopus / K8s deploy-time substitution that didn't run --
            // catch it at host startup, not on the first authentication attempt.
            Action act = () => new JwtSigningKeyProvider(ConfigWith(templateValue), isDevelopment: false);

            act.Should().Throw<InvalidOperationException>()
               .Where(e => e.Message.Contains("template placeholder")
                       && e.Message.Contains(JwtSigningKeyProvider.EnvVarName));
        }

        [Fact]
        public void UnsubstitutedTemplate_AllowedInDevelopment()
        {
            // Local dev with a never-substituted appsettings.json should not fail
            // at boot just because someone forgot to run the substitution tool.
            // The token-signing layer will still fail later when it tries to make
            // a key out of "{JwtTokenSecret}", but that's acceptable in dev.
            var provider = new JwtSigningKeyProvider(ConfigWith("{JwtTokenSecret}"), isDevelopment: true);

            provider.GetSecret().Should().Be("{JwtTokenSecret}");
        }

        [Fact]
        public void IsUnsubstitutedTemplate_DoesNotMatchLegitimateBraceValues()
        {
            // Conservative: values that happen to contain braces in odd places
            // should NOT be flagged. Specifically: outer braces with whitespace,
            // nested braces, or non-brace prefix/suffix are all "real values".
            JwtSigningKeyProvider.IsUnsubstitutedTemplate("{value with spaces}").Should().BeFalse();
            JwtSigningKeyProvider.IsUnsubstitutedTemplate("{nested{inner}}").Should().BeFalse();
            JwtSigningKeyProvider.IsUnsubstitutedTemplate("prefix{token}").Should().BeFalse();
            JwtSigningKeyProvider.IsUnsubstitutedTemplate("{token}suffix").Should().BeFalse();
            JwtSigningKeyProvider.IsUnsubstitutedTemplate("real-secret-no-braces").Should().BeFalse();
            JwtSigningKeyProvider.IsUnsubstitutedTemplate("").Should().BeFalse();
            JwtSigningKeyProvider.IsUnsubstitutedTemplate(null).Should().BeFalse();
        }

        [Fact]
        public void IsUnsubstitutedTemplate_MatchesCanonicalPlaceholders()
        {
            JwtSigningKeyProvider.IsUnsubstitutedTemplate("{JwtTokenSecret}").Should().BeTrue();
            JwtSigningKeyProvider.IsUnsubstitutedTemplate("{X}").Should().BeTrue();
            // Even one-char inner content qualifies, since substitution tooling
            // typically uses any identifier name.
        }

        // --- Validation: minimum key length (non-Development only) ------------------------

        [Fact]
        public void TooShortSecret_ThrowsInProduction()
        {
            // 16-byte secret would fail HMAC-SHA256's 256-bit floor at first signing.
            // Fail at startup instead.
            var shortSecret = new string('a', 16);
            Action act = () => new JwtSigningKeyProvider(ConfigWith(shortSecret), isDevelopment: false);

            act.Should().Throw<InvalidOperationException>()
               .Where(e => e.Message.Contains("too short")
                       && e.Message.Contains("16 bytes")
                       && e.Message.Contains("minimum 32"));
        }

        [Fact]
        public void TooShortSecret_AllowedInDevelopment()
        {
            // Local-dev fixtures often use short secrets; tolerate them here.
            // (Microsoft.IdentityModel still throws IDX10720 on the first signing
            // attempt, but that's deferred to the path that actually needs it.)
            var shortSecret = new string('a', 16);
            var provider = new JwtSigningKeyProvider(ConfigWith(shortSecret), isDevelopment: true);

            provider.GetSecret().Should().Be(shortSecret);
        }

        [Fact]
        public void MinViableSecret_AcceptedInProduction()
        {
            // Exactly 32 bytes -- the HMAC-SHA256 floor -- should pass validation.
            // Verifies the boundary condition: the existing dev-env literal
            // "EXAMPLE_WEAK_KEY_REPLACE_ME_0000" is 32 chars and should not be
            // rejected (it's an unfortunate baseline but we don't want to break
            // it without a paired migration).
            var provider = new JwtSigningKeyProvider(ConfigWith(MinViableSecret), isDevelopment: false);

            provider.GetSecret().Should().Be(MinViableSecret);
        }

        [Fact]
        public void StrongSecret_AcceptedInProduction()
        {
            var provider = new JwtSigningKeyProvider(ConfigWith(StrongSecret), isDevelopment: false);

            provider.GetSecret().Should().Be(StrongSecret);
        }

        // --- Cached signing key -------------------------------------------------------------

        [Fact]
        public void GetSigningKey_ReturnsSameInstanceAcrossCalls()
        {
            // The provider is a singleton; the SymmetricSecurityKey is allocated
            // once at construction and reused. Verifies no per-call reallocation
            // (which would silently undo the perf improvement of centralizing).
            var provider = new JwtSigningKeyProvider(ConfigWith(StrongSecret), isDevelopment: false);

            var first = provider.GetSigningKey();
            var second = provider.GetSigningKey();

            second.Should().BeSameAs(first);
        }

        [Fact]
        public void GetSigningKey_KeyBytesMatchSecret()
        {
            // Confirms the signing key wraps the same byte sequence the caller
            // provided -- catches a regression where the provider accidentally
            // used UTF-8 vs ASCII or otherwise transformed the input.
            var provider = new JwtSigningKeyProvider(ConfigWith(StrongSecret), isDevelopment: false);

            var key = provider.GetSigningKey();
            key.Key.Should().BeEquivalentTo(Encoding.ASCII.GetBytes(StrongSecret));
        }

        // ------------------------------------------------------------------
        // ROADMAP 18: the Development carve-out is explicit and observable.
        //
        // The carve-out itself is unchanged and deliberate: a fresh clone boots on the
        // unsubstituted "{JwtTokenSecret}" placeholder. What changed is that it used to be a bare
        // `if (isDevelopment) return;` before both checks, so a node ran with the literal
        // placeholder string as its HMAC key and nothing anywhere recorded it. The provider now
        // evaluates production validation in EVERY environment and, in Development, reports what
        // it waived through DevelopmentSecretWaiver, which both node hosts log at boot.
        // ------------------------------------------------------------------

        [Fact]
        public void DevelopmentWaiver_NamesThePlaceholderWhenOneIsAccepted()
        {
            var provider = new JwtSigningKeyProvider(ConfigWith("{JwtTokenSecret}"), isDevelopment: true);

            provider.DevelopmentSecretWaiver.Should().NotBeNull(
                "Development accepted a placeholder, and that decision must be visible");
            provider.DevelopmentSecretWaiver.Should().Contain("template placeholder");
            provider.DevelopmentSecretWaiver.Should().Contain("{JwtTokenSecret}",
                "the operator needs to see WHICH value was accepted");
        }

        [Fact]
        public void DevelopmentWaiver_NamesTheLengthWhenAShortSecretIsAccepted()
        {
            var provider = new JwtSigningKeyProvider(ConfigWith(new string('a', 16)), isDevelopment: true);

            provider.DevelopmentSecretWaiver.Should().NotBeNull();
            provider.DevelopmentSecretWaiver.Should().Contain("too short");
            provider.DevelopmentSecretWaiver.Should().Contain("16 bytes");
        }

        [Fact]
        public void DevelopmentWaiver_IsNullWhenTheSecretIsProductionGrade()
        {
            // A strong secret in Development is not a waiver. If this ever went non-null the
            // hosts would warn on every boot of a correctly configured dev box, the warning would
            // be tuned out, and the one time it mattered nobody would read it.
            var provider = new JwtSigningKeyProvider(ConfigWith(StrongSecret), isDevelopment: true);

            provider.DevelopmentSecretWaiver.Should().BeNull();
        }

        [Fact]
        public void DevelopmentWaiver_IsNullOutsideDevelopment()
        {
            // Outside Development nothing is ever waived: a weak secret throws (pinned by the
            // production tests above) and a strong one passes with nothing to report.
            var provider = new JwtSigningKeyProvider(ConfigWith(StrongSecret), isDevelopment: false);

            provider.DevelopmentSecretWaiver.Should().BeNull();
        }

        [Fact]
        public void ProductionRejection_StillCarriesTheOperatorGuidance()
        {
            // The refactor split the message into reason + guidance. The guidance names the
            // environment variable to set, which is the only actionable part for an operator
            // staring at a boot failure, so it must survive on BOTH rejection paths.
            Action placeholder = () => new JwtSigningKeyProvider(ConfigWith("{JwtTokenSecret}"), isDevelopment: false);
            Action tooShort = () => new JwtSigningKeyProvider(ConfigWith(new string('a', 16)), isDevelopment: false);

            placeholder.Should().Throw<InvalidOperationException>()
                .Which.Message.Should().Contain(JwtSigningKeyProvider.EnvVarName);
            tooShort.Should().Throw<InvalidOperationException>()
                .Which.Message.Should().Contain(JwtSigningKeyProvider.EnvVarName);
        }

        [Fact]
        public void PublisherOnlyProvider_HasNoWaiverBecauseItHasNoSecret()
        {
            // The SSO's publisher-only mode skips the symmetric path entirely (audit X-02), so
            // there is nothing to waive and the property must not invent one.
            var provider = JwtSigningKeyProvider.CreatePublisherOnly(ConfigWith(null), isDevelopment: true);

            provider.DevelopmentSecretWaiver.Should().BeNull();
        }
    }
}
