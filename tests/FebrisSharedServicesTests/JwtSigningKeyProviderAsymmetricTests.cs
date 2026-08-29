// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for the SSO Tier 1 asymmetric (RS256) additions to
    /// <see cref="JwtSigningKeyProvider"/>. Pins: a configured PEM private key is
    /// loaded and exposes RS256 signing credentials with a stable kid; a token signed
    /// with the private key validates against the published public key; the public JWK
    /// carries only public material; the kid is deterministic and overridable; and the
    /// transition tolerance (no asymmetric key configured in non-Development) leaves
    /// the symmetric path working.
    /// </summary>
    [Collection("JwtSigningKeyProviderEnv")]
    public class JwtSigningKeyProviderAsymmetricTests : IDisposable
    {
        private const string StrongSecret = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@";

        private readonly string _origSecretEnv;
        private readonly string _origPrivateKeyEnv;
        private readonly string _origKidEnv;

        public JwtSigningKeyProviderAsymmetricTests()
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

        // A config that returns the given symmetric secret plus (optionally) a PEM
        // private key and a kid override. Unset keys return null (Moq loose default).
        private static IConfiguration Config(string secret, string pem = null, string kid = null)
        {
            var mock = new Mock<IConfiguration>();
            mock.Setup(c => c["JwtSettings:Secret"]).Returns(secret);
            mock.Setup(c => c["JwtSettings:PrivateKey"]).Returns(pem);
            mock.Setup(c => c["JwtSettings:KeyId"]).Returns(kid);
            return mock.Object;
        }

        // Generates a fresh PKCS#8 PEM-encoded RSA-2048 private key for a test.
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

        [Fact]
        public void ConfiguredPrivateKey_ExposesRs256CredentialsAndPublicJwk()
        {
            string pem = GeneratePkcs8Pem();
            var provider = new JwtSigningKeyProvider(Config(StrongSecret, pem), isDevelopment: false);

            provider.HasAsymmetricKey.Should().BeTrue();
            provider.GetActiveKeyId().Should().NotBeNullOrEmpty();

            SigningCredentials creds = provider.GetAsymmetricSigningCredentials();
            creds.Should().NotBeNull();
            creds.Algorithm.Should().Be(SecurityAlgorithms.RsaSha256);
            creds.Key.KeyId.Should().Be(provider.GetActiveKeyId());

            JsonWebKey jwk = provider.GetPublicJwk();
            jwk.Should().NotBeNull();
            jwk.Kty.Should().Be("RSA");
            jwk.Use.Should().Be("sig");
            jwk.Alg.Should().Be(SecurityAlgorithms.RsaSha256);
            jwk.Kid.Should().Be(provider.GetActiveKeyId());
            jwk.N.Should().NotBeNullOrEmpty();
            jwk.E.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void TokenSignedWithPrivateKey_ValidatesAgainstPublicKey()
        {
            string pem = GeneratePkcs8Pem();
            var provider = new JwtSigningKeyProvider(Config(StrongSecret, pem), isDevelopment: false);

            var handler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken(
                issuer: "febris",
                audience: "febris",
                claims: new[] { new Claim(ClaimTypes.Name, "device-1") },
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: provider.GetAsymmetricSigningCredentials());
            string jwt = handler.WriteToken(token);

            var tvp = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = provider.GetPublicValidationKey(),
            };

            handler.ValidateToken(jwt, tvp, out SecurityToken validated);
            ((JwtSecurityToken)validated).Header.Kid.Should().Be(provider.GetActiveKeyId(),
                "the kid is stamped in the header so verifiers can select the right JWKS key");
        }

        [Fact]
        public void PublicJwk_DoesNotLeakPrivateMaterial()
        {
            string pem = GeneratePkcs8Pem();
            var provider = new JwtSigningKeyProvider(Config(StrongSecret, pem), isDevelopment: false);

            JsonWebKey jwk = provider.GetPublicJwk();
            jwk.D.Should().BeNullOrEmpty("the published JWK must be public-only");
            jwk.P.Should().BeNullOrEmpty();
            jwk.Q.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Kid_IsDeterministicForTheSameKey()
        {
            string pem = GeneratePkcs8Pem();
            var a = new JwtSigningKeyProvider(Config(StrongSecret, pem), isDevelopment: false);
            var b = new JwtSigningKeyProvider(Config(StrongSecret, pem), isDevelopment: false);
            a.GetActiveKeyId().Should().Be(b.GetActiveKeyId(),
                "the same key must always advertise the same kid across restarts");
        }

        [Fact]
        public void ConfiguredKid_OverridesTheDerivedKid()
        {
            string pem = GeneratePkcs8Pem();
            var provider = new JwtSigningKeyProvider(Config(StrongSecret, pem, kid: "rotation-2026-06"), isDevelopment: false);
            provider.GetActiveKeyId().Should().Be("rotation-2026-06");
            provider.GetPublicJwk().Kid.Should().Be("rotation-2026-06");
        }

        [Fact]
        public void EnvVarPrivateKey_TakesPrecedenceOverConfig()
        {
            string envPem = GeneratePkcs8Pem();
            string configPem = GeneratePkcs8Pem();
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.PrivateKeyEnvVarName, envPem);

            var fromEnv = new JwtSigningKeyProvider(Config(StrongSecret, configPem), isDevelopment: false);
            var configOnly = new JwtSigningKeyProvider(Config(StrongSecret, configPem), isDevelopment: false);

            // The env-var key resolves before the config key, so its kid differs
            // from the config-only provider's kid.
            Environment.SetEnvironmentVariable(JwtSigningKeyProvider.PrivateKeyEnvVarName, null);
            var configOnly2 = new JwtSigningKeyProvider(Config(StrongSecret, configPem), isDevelopment: false);

            fromEnv.GetActiveKeyId().Should().NotBe(configOnly2.GetActiveKeyId());
        }

        [Fact]
        public void NoAsymmetricKey_InProduction_IsTransitionTolerant()
        {
            // Non-Development with no asymmetric key configured: the provider does NOT
            // throw (transition state), HasAsymmetricKey is false, accessors are null,
            // and the symmetric path still works so existing HMAC tokens keep flowing.
            var provider = new JwtSigningKeyProvider(Config(StrongSecret), isDevelopment: false);
            provider.HasAsymmetricKey.Should().BeFalse();
            provider.GetAsymmetricSigningCredentials().Should().BeNull();
            provider.GetPublicValidationKey().Should().BeNull();
            provider.GetPublicJwk().Should().BeNull();
            provider.GetSigningKey().Should().NotBeNull();
        }

        [Fact]
        public void NoAsymmetricKey_InDevelopment_GeneratesEphemeralKey()
        {
            // Development generates an ephemeral pair so the asymmetric path and the
            // JWKS endpoint are exercisable locally without configuring a key.
            var provider = new JwtSigningKeyProvider(Config(StrongSecret), isDevelopment: true);
            provider.HasAsymmetricKey.Should().BeTrue();
            provider.GetAsymmetricSigningCredentials().Should().NotBeNull();
            provider.GetPublicJwk().Should().NotBeNull();
        }

        [Fact]
        public void MalformedPrivateKey_FailsFast()
        {
            Action act = () => new JwtSigningKeyProvider(Config(StrongSecret, "-----BEGIN PRIVATE KEY-----\nnot-base64!!!\n-----END PRIVATE KEY-----"), isDevelopment: false);
            act.Should().Throw<InvalidOperationException>("a configured-but-broken key is a misconfiguration that must fail at startup");
        }

        [Fact]
        public void GetAllValidationKeys_IncludesSymmetricAndRsa_WhenAsymmetricPresent()
        {
            string pem = GeneratePkcs8Pem();
            var provider = new JwtSigningKeyProvider(Config(StrongSecret, pem), isDevelopment: false);
            var keys = provider.GetAllValidationKeys();
            keys.Should().HaveCount(2, "both the legacy HMAC key and the RSA public key validate during the transition");
            keys.Should().Contain(k => k is SymmetricSecurityKey);
            keys.Should().Contain(k => k is RsaSecurityKey);
        }

        [Fact]
        public void GetAllValidationKeys_IsSymmetricOnly_WhenNoAsymmetricKey()
        {
            var provider = new JwtSigningKeyProvider(Config(StrongSecret), isDevelopment: false);
            var keys = provider.GetAllValidationKeys();
            keys.Should().ContainSingle().Which.Should().BeAssignableTo<SymmetricSecurityKey>();
        }

        [Fact]
        public void BothHmacAndRs256Tokens_ValidateAgainstTheDualKeySet()
        {
            // The transition contract: an issuer flipped to RS256 (T1.4) and a legacy
            // HMAC issuer both produce tokens that validate against the dual key set a
            // validator now uses (T1.3). This is what keeps in-flight HMAC tokens alive
            // while new tokens go asymmetric.
            string pem = GeneratePkcs8Pem();
            var provider = new JwtSigningKeyProvider(Config(StrongSecret, pem), isDevelopment: false);
            var handler = new JwtSecurityTokenHandler();

            var rs256 = new JwtSecurityToken(
                claims: new[] { new Claim("k", "rsa") },
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: provider.GetAsymmetricSigningCredentials());
            string rsaJwt = handler.WriteToken(rs256);

            var hs256 = new JwtSecurityToken(
                claims: new[] { new Claim("k", "hmac") },
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(provider.GetSigningKey(), SecurityAlgorithms.HmacSha256Signature));
            string hmacJwt = handler.WriteToken(hs256);

            var tvp = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = provider.GetAllValidationKeys(),
            };

            Action validateRsa = () => handler.ValidateToken(rsaJwt, tvp, out _);
            Action validateHmac = () => handler.ValidateToken(hmacJwt, tvp, out _);
            validateRsa.Should().NotThrow("RS256 tokens validate against the RSA key in the dual set");
            validateHmac.Should().NotThrow("legacy HMAC tokens validate against the symmetric key in the dual set");
        }
    }
}
