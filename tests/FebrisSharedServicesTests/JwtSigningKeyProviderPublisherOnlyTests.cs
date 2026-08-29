// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using System.Text;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for the AUTH-6 / audit X-02 publisher-only construction mode of JwtSigningKeyProvider.
    /// Pins the trust-domain contract: a publisher-only host (the SSO) boots with NO symmetric secret
    /// because it signs/validates nothing and only publishes the JWKS; the full provider (the API
    /// tiers) STILL fails closed without a symmetric secret; publisher-only exposes the asymmetric
    /// JWKS material while its symmetric accessors return null.
    /// </summary>
    [Collection("JwtSigningKeyProviderEnv")]
    public class JwtSigningKeyProviderPublisherOnlyTests : IDisposable
    {
        private const string StrongSecret =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@";

        private readonly string _origSecretEnv;
        private readonly string _origPrivateKeyEnv;
        private readonly string _origKidEnv;

        public JwtSigningKeyProviderPublisherOnlyTests()
        {
            _origSecretEnv = Environment.GetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName);
            _origPrivateKeyEnv = Environment.GetEnvironmentVariable(JwtSigningKeyProvider.PrivateKeyEnvVarName);
            _origKidEnv = Environment.GetEnvironmentVariable(JwtSigningKeyProvider.KeyIdEnvVarName);
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName, null);
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.PrivateKeyEnvVarName, null);
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.KeyIdEnvVarName, null);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName, _origSecretEnv);
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.PrivateKeyEnvVarName, _origPrivateKeyEnv);
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.KeyIdEnvVarName, _origKidEnv);
        }

        private static IConfiguration Config(string secret, string pem = null)
        {
            var mock = new Mock<IConfiguration>();
            mock.Setup(c => c["JwtSettings:Secret"]).Returns(secret);
            mock.Setup(c => c["JwtSettings:PrivateKey"]).Returns(pem);
            mock.Setup(c => c["JwtSettings:KeyId"]).Returns((string)null);
            return mock.Object;
        }

        private static string GeneratePkcs8Pem()
        {
            using (var rsa = RSA.Create(2048))
            {
                byte[] der = rsa.ExportPkcs8PrivateKey();
                string b64 = Convert.ToBase64String(der);
                var sb = new StringBuilder();
                sb.Append("-----BEGIN PRIVATE KEY-----\n");
                for (int i = 0; i < b64.Length; i += 64)
                {
                    sb.Append(b64.Substring(i, Math.Min(64, b64.Length - i))).Append('\n');
                }
                sb.Append("-----END PRIVATE KEY-----\n");
                return sb.ToString();
            }
        }

        // --- The SSO boot fix: publisher-only requires no symmetric secret ---

        [Fact]
        public void PublisherOnly_BootsInProduction_WithNoSymmetricSecretAnywhere()
        {
            Action act = () => JwtSigningKeyProvider.CreatePublisherOnly(
                Config(secret: null), isDevelopment: false);

            act.Should().NotThrow(
                "the SSO is publisher-only and carries no symmetric HMAC secret (audit X-02)");
        }

        [Fact]
        public void PublisherOnly_SymmetricAccessors_AreNull()
        {
            var provider = JwtSigningKeyProvider.CreatePublisherOnly(
                Config(secret: null), isDevelopment: false);

            provider.GetSecret().Should().BeNull("publisher-only holds no symmetric secret");
            provider.GetSigningKey().Should().BeNull("publisher-only holds no symmetric key");
        }

        [Fact]
        public void PublisherOnly_IgnoresAStraySymmetricSecret_AndNeverExposesIt()
        {
            // Even if a secret happens to be present in the environment, a publisher-only host must not
            // read it -- it has no signing role. This pins that the mode is about ROLE, not absence.
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.EnvVarName, StrongSecret);

            var provider = JwtSigningKeyProvider.CreatePublisherOnly(
                Config(secret: null), isDevelopment: false);

            provider.GetSecret().Should().BeNull();
            provider.GetSigningKey().Should().BeNull();
        }

        // --- The other half: the API tiers still fail closed ---

        [Fact]
        public void FullProvider_StillFailsClosed_WithoutASecret_InProduction()
        {
            Action act = () => new JwtSigningKeyProvider(Config(secret: null), isDevelopment: false);

            act.Should().Throw<InvalidOperationException>()
               .Where(e => e.Message.Contains("not configured"));
        }

        [Fact]
        public void FullProvider_StillFailsClosed_WithoutASecret_InDevelopment()
        {
            Action act = () => new JwtSigningKeyProvider(Config(secret: null), isDevelopment: true);

            act.Should().Throw<InvalidOperationException>()
               .Where(e => e.Message.Contains("not configured"));
        }

        // --- Publisher-only still publishes the JWKS ---

        [Fact]
        public void PublisherOnly_WithConfiguredKey_PublishesTheJwks()
        {
            string pem = GeneratePkcs8Pem();
            var provider = JwtSigningKeyProvider.CreatePublisherOnly(
                Config(secret: null, pem: pem), isDevelopment: false);

            provider.HasAsymmetricKey.Should().BeTrue();
            provider.GetActiveKeyId().Should().NotBeNullOrEmpty();

            JsonWebKey jwk = provider.GetPublicJwk();
            jwk.Should().NotBeNull();
            jwk.Kty.Should().Be("RSA");
            jwk.Use.Should().Be("sig");
            jwk.Alg.Should().Be(SecurityAlgorithms.RsaSha256);
            jwk.Kid.Should().Be(provider.GetActiveKeyId());
            jwk.N.Should().NotBeNullOrEmpty();
            jwk.E.Should().NotBeNullOrEmpty();
            jwk.D.Should().BeNullOrEmpty("the published JWK must be public-only");
        }

        [Fact]
        public void PublisherOnly_InDevelopment_GeneratesEphemeralKey_AndNeedsNoSecret()
        {
            var provider = JwtSigningKeyProvider.CreatePublisherOnly(
                Config(secret: null), isDevelopment: true);

            provider.HasAsymmetricKey.Should().BeTrue();
            provider.GetPublicJwk().Should().NotBeNull();
            provider.GetSecret().Should().BeNull();
        }

        [Fact]
        public void PublisherOnly_NoConfiguredKey_InProduction_HasEmptyJwks()
        {
            var provider = JwtSigningKeyProvider.CreatePublisherOnly(
                Config(secret: null), isDevelopment: false);

            provider.HasAsymmetricKey.Should().BeFalse();
            provider.GetPublicJwk().Should().BeNull();
            provider.GetSecret().Should().BeNull();
        }

        // --- Validation-key set never contains a symmetric key in publisher-only ---

        [Fact]
        public void PublisherOnly_ValidationKeys_ContainNoSymmetricKey()
        {
            string pem = GeneratePkcs8Pem();
            var provider = JwtSigningKeyProvider.CreatePublisherOnly(
                Config(secret: null, pem: pem), isDevelopment: false);

            IList<SecurityKey> keys = provider.GetAllValidationKeys();
            keys.Should().NotContain(k => k is SymmetricSecurityKey,
                "a publisher-only host holds no symmetric key");
            keys.Should().ContainSingle().Which.Should().BeAssignableTo<RsaSecurityKey>();
        }

        [Fact]
        public void PublisherOnly_ValidationKeys_AreEmpty_WhenNoAsymmetricKey()
        {
            var provider = JwtSigningKeyProvider.CreatePublisherOnly(
                Config(secret: null), isDevelopment: false);

            provider.GetAllValidationKeys().Should().BeEmpty(
                "no symmetric key and no asymmetric key -> nothing to validate against");
        }
    }
}
