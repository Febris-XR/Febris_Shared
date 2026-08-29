// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Security.Cryptography;
using System.Text;

namespace Febris.SharedServices
{
    /// <summary>
    /// Generation and hashing for the device authentication credential
    /// (<c>LocalHardware.PhysicalLicense</c>).
    ///
    /// <para>
    /// WHY (audit T9). The credential was stored in CLEARTEXT, rendered in three Portal views to
    /// Educator, Admin and ITAdmin, and -- until it was removed the same day -- copied into the
    /// Hardware claim of every JWT. It was also whatever free text an admin happened to type into a
    /// form field.
    /// </para>
    ///
    /// <para>
    /// HASHING ALONE WOULD HAVE MADE THINGS WORSE, and that is why generation lives here too. An
    /// admin-chosen string that is hashed is both low-entropy AND unrecoverable: you lose the
    /// ability to read it back for provisioning without gaining real resistance to guessing. The two
    /// changes only make sense together -- the node MINTS a high-entropy credential, shows it once,
    /// and stores only its hash.
    /// </para>
    ///
    /// <para>
    /// A PLAIN SHA-256 IS THE RIGHT PRIMITIVE HERE, which is not true of passwords. Slow, salted
    /// KDFs (PBKDF2, bcrypt, Argon2) exist to make GUESSING a human-chosen secret expensive. This
    /// input is 256 bits from a CSPRNG, so there is nothing to guess, and the properties that matter
    /// instead are:
    /// </para>
    /// <list type="bullet">
    /// <item>DETERMINISTIC, because <c>HardwareQueries.GetByKey</c> resolves a device by looking the
    /// credential up. A per-row salted hash would force a table scan and defeat the unique index
    /// added for exactly that lookup.</item>
    /// <item>FAST, because it runs on every device authentication.</item>
    /// <item>NO PEPPER, deliberately. A server-side pepper adds nothing against a 256-bit random
    /// input, and losing it would strand every device on the node with no way back.</item>
    /// </list>
    ///
    /// <para>
    /// The hash is lowercase hex of the UTF-8 bytes. That exact format matters: the migration that
    /// converts existing rows computes it in SQL as
    /// <c>encode(sha256(convert_to("PhysicalLicense", 'UTF8')), 'hex')</c>, which must agree with
    /// this method byte for byte or every already-provisioned device stops authenticating.
    /// </para>
    ///
    /// <para>
    /// SECOND CALLER, recorded so this doc does not become a lie: the node's invitation tokens
    /// (<c>NodeUserInvite.TokenHash</c>) use <see cref="Generate"/> and <see cref="Hash"/> unchanged.
    /// The primitive is shared rather than copied because every line of the reasoning above
    /// transfers exactly -- a 256-bit CSPRNG secret with nothing to guess, looked up BY its hash so
    /// the hash must be deterministic, and no pepper to lose. Only the migration paragraph is
    /// device-specific: invitation rows are hashed from birth, so nothing converts them.
    /// </para>
    /// </summary>
    public static class DeviceCredential
    {
        /// <summary>Bytes of entropy in a generated credential.</summary>
        public const int EntropyBytes = 32;

        /// <summary>Characters in the hex hash, used to recognise an already-hashed value.</summary>
        public const int HashLength = 64;

        /// <summary>
        /// A new credential: 256 bits from a CSPRNG, base64url encoded so it survives a URL, a form
        /// post and a copy-paste without escaping.
        /// </summary>
        public static string Generate()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(EntropyBytes);

            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        /// <summary>
        /// Lowercase hex SHA-256 of the UTF-8 bytes. Null or empty in, null or empty out -- an
        /// unregistered device carries no credential and must not be given the hash of an empty
        /// string, which would be a single shared value every such device could authenticate with.
        /// </summary>
        public static string Hash(string credential)
        {
            if (string.IsNullOrEmpty(credential))
            {
                return credential;
            }

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(credential));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
        /// Whether a stored value already looks like one of our hashes: exactly
        /// <see cref="HashLength"/> lowercase hex characters.
        ///
        /// <para>
        /// Used to keep the conversion idempotent. Re-hashing an already-hashed value would lock
        /// every device out, and a migration that can run twice is worth being defensive about.
        /// </para>
        /// </summary>
        public static bool LooksHashed(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != HashLength)
            {
                return false;
            }

            foreach (char c in value)
            {
                bool isHexDigit = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f');
                if (!isHexDigit)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
