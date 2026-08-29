// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Febris.SharedServices
{
    /// <summary>
    /// Centralizes JWT signing-key resolution for both token-issuance and
    /// token-validation paths across the Febris APIs (SharedAPI, DeveloperApi,
    /// EndUserApi) plus the License middleware.
    ///
    /// <para>
    /// Two key families are exposed during the SSO Tier 1 migration to asymmetric
    /// signing:
    /// <list type="bullet">
    ///   <item><b>Symmetric (HMAC-SHA256)</b> -- the legacy shared secret, retained
    ///         so in-flight HS256 tokens keep validating during the dual-validation
    ///         transition window. Removed once the drain window closes (T1.6).</item>
    ///   <item><b>Asymmetric (RS256)</b> -- an RSA key pair. The private half signs
    ///         new tokens; the public half validates them and is published at the
    ///         SSO JWKS endpoint. Validators that only need the public half no longer
    ///         hold a secret capable of forging tokens.</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// Symmetric resolution order: env var <see cref="EnvVarName"/> (preferred), then
    /// <c>JwtSettings:Secret</c>. Asymmetric resolution order: env var
    /// <see cref="PrivateKeyEnvVarName"/> (PEM), then <c>JwtSettings:PrivateKey</c>.
    /// During the transition the asymmetric key is OPTIONAL: if none is configured in
    /// a non-Development environment, <see cref="HasAsymmetricKey"/> is false and the
    /// asymmetric accessors return null (issuers fall back to HMAC). In Development an
    /// ephemeral key pair is generated so the asymmetric path and the JWKS endpoint
    /// are exercisable locally.
    /// </para>
    ///
    /// <para>
    /// Validation runs at construction time so app boot fails fast on misconfiguration
    /// rather than on the first authentication attempt. Registration is unchanged:
    /// <code>
    /// var jwtKeyProvider = new JwtSigningKeyProvider(Configuration, _env.IsDevelopment());
    /// services.AddSingleton&lt;IJwtSigningKeyProvider&gt;(jwtKeyProvider);
    /// </code>
    /// </para>
    /// </summary>
    public interface IJwtSigningKeyProvider
    {
        /// <summary>Cached <see cref="SymmetricSecurityKey"/> for legacy HMAC token
        /// signing + validation. Allocated once at construction.</summary>
        SymmetricSecurityKey GetSigningKey();

        /// <summary>The raw symmetric secret string. Use only when an existing API
        /// requires the bytes (e.g., populating <c>JwtSettings.Secret</c> in legacy
        /// code). Prefer <see cref="GetSigningKey"/> for new code.</summary>
        string GetSecret();

        /// <summary>True when an RSA key pair is available (configured, or generated
        /// in Development). When false, the asymmetric accessors return null and
        /// callers fall back to the symmetric path during the transition.</summary>
        bool HasAsymmetricKey { get; }

        /// <summary>The <c>kid</c> stamped into the JWT header of asymmetric tokens
        /// and published in the JWKS, used by verifiers to select the right public
        /// key during rotation. Null when <see cref="HasAsymmetricKey"/> is false.</summary>
        string GetActiveKeyId();

        /// <summary>RS256 signing credentials (private key + <c>kid</c>) for ISSUERS.
        /// Null when <see cref="HasAsymmetricKey"/> is false.</summary>
        SigningCredentials GetAsymmetricSigningCredentials();

        /// <summary>The public RSA key (with <c>kid</c>) for VALIDATORS that hold the
        /// key material locally. Null when <see cref="HasAsymmetricKey"/> is false.
        /// Pure validate-only consumers should instead fetch the key from the SSO
        /// JWKS endpoint rather than depend on this provider.</summary>
        RsaSecurityKey GetPublicValidationKey();

        /// <summary>The public key serialized as a JWK (RSA, <c>use=sig</c>,
        /// <c>alg=RS256</c>, with <c>kid</c>/<c>n</c>/<c>e</c>) for the JWKS endpoint.
        /// Null when <see cref="HasAsymmetricKey"/> is false.</summary>
        JsonWebKey GetPublicJwk();

        /// <summary>All keys a validator should accept during the dual-validation
        /// transition: the symmetric key plus (when present) the RSA public key.
        /// RS256 tokens are matched to the RSA key by <c>kid</c>; legacy HMAC tokens
        /// are matched to the symmetric key. Once the transition completes (T1.6) the
        /// symmetric key is dropped so only asymmetric tokens validate.</summary>
        IList<SecurityKey> GetAllValidationKeys();
    }

    public class JwtSigningKeyProvider : IJwtSigningKeyProvider
    {
        /// <summary>Environment variable consulted first for the symmetric secret.</summary>
        public const string EnvVarName = "FEBRIS_JWT_SIGNING_SECRET";

        /// <summary>Environment variable consulted first for the RSA private key
        /// (PEM, either PKCS#8 "BEGIN PRIVATE KEY" or PKCS#1 "BEGIN RSA PRIVATE KEY").</summary>
        public const string PrivateKeyEnvVarName = "FEBRIS_JWT_SIGNING_PRIVATE_KEY";

        /// <summary>Optional environment variable overriding the derived <c>kid</c>.
        /// When unset, the kid is derived deterministically from the public key.</summary>
        public const string KeyIdEnvVarName = "FEBRIS_JWT_SIGNING_KID";

        // HMAC-SHA256 requires 256 bits = 32 bytes of key material as the floor.
        private const int MinSecretByteLength = 32;

        // RSA below 2048 bits is rejected in non-Development.
        private const int MinRsaKeySizeBits = 2048;

        private readonly string _secret;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly string _developmentSecretWaiver;

        /// <summary>
        /// Non-null when, and only when, the symmetric secret in use would have been REJECTED
        /// outside Development and was accepted solely because this host is running as Development.
        /// The value names the check that was waived. Null when the secret is production-grade, and
        /// null for a publisher-only provider, which carries no symmetric secret.
        ///
        /// <para>
        /// This exists so the Development carve-out is OBSERVABLE rather than a silent early return
        /// (ROADMAP 18). Before it, a node booted on the literal string <c>{JwtTokenSecret}</c> as its
        /// HMAC key and nothing anywhere said so; the audit even recorded this provider as "fails
        /// closed on an unsubstituted placeholder" without the carve-out, because the carve-out was
        /// invisible in the one environment most people run. Hosts log this at boot.
        /// </para>
        /// </summary>
        public string DevelopmentSecretWaiver
        {
            get { return _developmentSecretWaiver; }
        }

        private readonly bool _hasAsymmetric;
        private readonly string _keyId;
        private readonly SigningCredentials _asymmetricSigningCredentials;
        private readonly RsaSecurityKey _publicValidationKey;
        private readonly JsonWebKey _publicJwk;

        /// <summary>
        /// Full provider for hosts that both sign and validate device JWTs (the API tiers: SharedAPI,
        /// DeveloperApi, EndUserApi). Requires a symmetric secret and fails fast at construction when it
        /// is missing or weak. Behavior is unchanged from before the publisher-only split.
        /// </summary>
        public JwtSigningKeyProvider(IConfiguration configuration, bool isDevelopment)
            : this(configuration, isDevelopment, publisherOnly: false)
        {
        }

        /// <summary>
        /// Publisher-only provider for a host whose ONLY JWT role is publishing the JWKS (the central
        /// SSO). It never mints or validates device JWTs, so it must NOT be forced to carry a symmetric
        /// HMAC secret. In this mode the symmetric path is skipped entirely: no secret is read, the
        /// fail-fast does not run, and GetSigningKey/GetSecret return null. The asymmetric (JWKS) path is
        /// identical to the full provider (audit X-02).
        /// </summary>
        public static JwtSigningKeyProvider CreatePublisherOnly(IConfiguration configuration, bool isDevelopment)
        {
            return new JwtSigningKeyProvider(configuration, isDevelopment, publisherOnly: true);
        }

        private JwtSigningKeyProvider(IConfiguration configuration, bool isDevelopment, bool publisherOnly)
        {
            // --- Symmetric (legacy, retained for the transition) ---
            // A publisher-only host (the SSO) signs and validates nothing, so it carries no symmetric
            // secret and skips the fail-fast entirely (audit X-02).
            if (!publisherOnly)
            {
                string envValue = Environment.GetEnvironmentVariable(EnvVarName);
                string configValue = configuration?["JwtSettings:Secret"];
                string secret = !string.IsNullOrWhiteSpace(envValue) ? envValue : configValue;

                _developmentSecretWaiver = ValidateOrThrow(secret, isDevelopment);

                _secret = secret;
                _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
            }

            // --- Asymmetric (RS256) ---
            string pemEnv = Environment.GetEnvironmentVariable(PrivateKeyEnvVarName);
            string pemConfig = configuration?["JwtSettings:PrivateKey"];
            string pem = !string.IsNullOrWhiteSpace(pemEnv) ? pemEnv : pemConfig;

            RSA rsa = null;
            if (!string.IsNullOrWhiteSpace(pem))
            {
                // A key was explicitly configured: a parse/strength failure is a
                // misconfiguration and must fail fast, even in Development.
                rsa = LoadRsaFromPem(pem);
                if (!isDevelopment && rsa.KeySize < MinRsaKeySizeBits)
                {
                    throw new InvalidOperationException(
                        "JWT RSA signing key is too small (" + rsa.KeySize + " bits, minimum " +
                        MinRsaKeySizeBits + "). Generate at least a 2048-bit RSA key.");
                }
            }
            else if (isDevelopment)
            {
                // No key configured locally: generate an ephemeral pair so the
                // asymmetric path and JWKS endpoint can be exercised in dev. The
                // pair is regenerated each run, which is fine for local development.
                rsa = RSA.Create(MinRsaKeySizeBits);
            }
            // else: non-Development with no asymmetric key configured -> transition
            // state. HasAsymmetricKey stays false; issuers fall back to HMAC until a
            // key is rolled out.

            if (rsa != null)
            {
                string kidOverride = Environment.GetEnvironmentVariable(KeyIdEnvVarName);
                if (string.IsNullOrWhiteSpace(kidOverride))
                {
                    kidOverride = configuration?["JwtSettings:KeyId"];
                }
                _keyId = !string.IsNullOrWhiteSpace(kidOverride) ? kidOverride : DeriveKeyId(rsa);

                // Private key (signing). RsaSecurityKey wraps the RSA including its
                // private parameters; the kid lets verifiers select it.
                var privateKey = new RsaSecurityKey(rsa) { KeyId = _keyId };
                _asymmetricSigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

                // Public key (validation + JWKS). Re-import the public parameters
                // only, so neither the validation key nor the JWK leaks private bits.
                RSAParameters pub = rsa.ExportParameters(false);
                var publicRsa = RSA.Create();
                publicRsa.ImportParameters(pub);
                _publicValidationKey = new RsaSecurityKey(publicRsa) { KeyId = _keyId };

                _publicJwk = new JsonWebKey
                {
                    Kty = "RSA",
                    Use = "sig",
                    Alg = SecurityAlgorithms.RsaSha256, // "RS256"
                    Kid = _keyId,
                    N = Base64UrlEncoder.Encode(pub.Modulus),
                    E = Base64UrlEncoder.Encode(pub.Exponent),
                };

                _hasAsymmetric = true;
            }
        }

        public SymmetricSecurityKey GetSigningKey() => _signingKey;
        public string GetSecret() => _secret;

        public bool HasAsymmetricKey => _hasAsymmetric;
        public string GetActiveKeyId() => _keyId;
        public SigningCredentials GetAsymmetricSigningCredentials() => _asymmetricSigningCredentials;
        public RsaSecurityKey GetPublicValidationKey() => _publicValidationKey;
        public JsonWebKey GetPublicJwk() => _publicJwk;

        public IList<SecurityKey> GetAllValidationKeys()
        {
            var keys = new List<SecurityKey>();
            if (_signingKey != null)
            {
                keys.Add(_signingKey);
            }
            if (_publicValidationKey != null)
            {
                keys.Add(_publicValidationKey);
            }
            return keys;
        }

        /// <summary>
        /// The reason a secret fails PRODUCTION validation, or null when it passes. Evaluated in
        /// every environment, so that Development can report what it is waiving instead of simply
        /// not looking.
        /// </summary>
        private static string ProductionRejectionReason(string secret)
        {
            if (IsUnsubstitutedTemplate(secret))
            {
                return "JWT signing secret looks like an unsubstituted template " +
                    "placeholder ('" + secret + "').";
            }

            int byteLength = Encoding.ASCII.GetByteCount(secret);
            if (byteLength < MinSecretByteLength)
            {
                return "JWT signing secret is too short (" + byteLength + " bytes, minimum " +
                    MinSecretByteLength + "). HMAC-SHA256 requires at least 256 bits of key material.";
            }

            return null;
        }

        /// <summary>
        /// Validates the symmetric secret. A MISSING secret throws in every environment. A secret that
        /// would fail production validation throws outside Development, and inside Development is
        /// accepted and the waived reason is RETURNED so the host can say so at boot.
        ///
        /// <para>
        /// This used to be <c>if (isDevelopment) return;</c> placed before both checks, which made
        /// the carve-out invisible: a node booted with the literal <c>{JwtTokenSecret}</c> as its
        /// HMAC key and nothing recorded it. The carve-out itself is kept, deliberately -- it is
        /// why a fresh clone can start at all -- but it is now a named decision with an observable
        /// result rather than a bare early return (ROADMAP 18). Semantics are unchanged for every
        /// environment.
        /// </para>
        /// </summary>
        /// <returns>Null when the secret is production-grade, otherwise the waived reason (Development only).</returns>
        private static string ValidateOrThrow(string secret, bool isDevelopment)
        {
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "JWT signing secret is not configured. Set the " + EnvVarName +
                    " environment variable, or populate JwtSettings:Secret in appsettings.");
            }

            string reason = ProductionRejectionReason(secret);
            if (reason == null)
            {
                return null;
            }

            if (isDevelopment)
            {
                // THE DEVELOPMENT CARVE-OUT. Accept the weak or placeholder secret so local
                // development boots, and hand the reason back to be logged.
                return reason;
            }

            throw new InvalidOperationException(
                reason + " Set the " + EnvVarName +
                " environment variable with the real signing secret in this environment.");
        }

        /// <summary>
        /// Parses a PEM-encoded RSA private key into an <see cref="RSA"/> instance.
        /// .NET Core 3.1 has no one-call <c>ImportFromPem</c>, so the armor is
        /// stripped and the DER body imported via the PKCS#8 or PKCS#1 importer
        /// (both present in 3.1) depending on the header.
        /// </summary>
        private static RSA LoadRsaFromPem(string pem)
        {
            try
            {
                bool isPkcs1 = pem.IndexOf("BEGIN RSA PRIVATE KEY", StringComparison.Ordinal) >= 0;

                var base64 = new StringBuilder();
                foreach (string rawLine in pem.Split('\n'))
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0) continue;
                    if (line.StartsWith("-----", StringComparison.Ordinal)) continue;
                    base64.Append(line);
                }

                byte[] der = Convert.FromBase64String(base64.ToString());
                var rsa = RSA.Create();
                if (isPkcs1)
                {
                    rsa.ImportRSAPrivateKey(der, out _);
                }
                else
                {
                    rsa.ImportPkcs8PrivateKey(der, out _);
                }
                return rsa;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "JWT RSA private key could not be parsed. Provide a PEM-encoded RSA " +
                    "private key (PKCS#8 'BEGIN PRIVATE KEY' or PKCS#1 'BEGIN RSA PRIVATE KEY') " +
                    "via the " + PrivateKeyEnvVarName + " environment variable or JwtSettings:PrivateKey.",
                    ex);
            }
        }

        /// <summary>
        /// Derives a deterministic, opaque <c>kid</c> from the public key so the same
        /// key always advertises the same id (stable across restarts) without
        /// requiring a separately-configured id. SHA-256 over modulus + exponent,
        /// base64url, truncated. A configured <see cref="KeyIdEnvVarName"/> overrides.
        /// </summary>
        private static string DeriveKeyId(RSA rsa)
        {
            RSAParameters p = rsa.ExportParameters(false);
            using (var sha = SHA256.Create())
            {
                byte[] modExp = new byte[p.Modulus.Length + p.Exponent.Length];
                Buffer.BlockCopy(p.Modulus, 0, modExp, 0, p.Modulus.Length);
                Buffer.BlockCopy(p.Exponent, 0, modExp, p.Modulus.Length, p.Exponent.Length);
                byte[] hash = sha.ComputeHash(modExp);
                string encoded = Base64UrlEncoder.Encode(hash);
                return encoded.Length > 16 ? encoded.Substring(0, 16) : encoded;
            }
        }

        /// <summary>
        /// Detects Octopus / Helm / K8s-style placeholders such as
        /// <c>"{JwtTokenSecret}"</c>. Conservative: exactly one outer <c>{ ... }</c>
        /// pair, no nested braces, no whitespace in the body.
        /// </summary>
        public static bool IsUnsubstitutedTemplate(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (value.Length < 2) return false;
            if (value[0] != '{') return false;
            if (value[value.Length - 1] != '}') return false;
            for (int i = 1; i < value.Length - 1; i++)
            {
                char c = value[i];
                if (c == '{' || c == '}' || char.IsWhiteSpace(c)) return false;
            }
            return true;
        }
    }
}
