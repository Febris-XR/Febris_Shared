// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Audit T9: the device authentication credential was stored in cleartext, rendered in three
    /// Portal views, and was whatever free text an admin typed. It is now MINTED by the node with
    /// 256 bits of entropy, shown once, and stored only as a hash.
    ///
    /// <para>
    /// Generation and hashing ship together on purpose. Hashing an admin-chosen string would be
    /// worse than the status quo: low entropy AND unrecoverable.
    /// </para>
    /// </summary>
    public class DeviceCredentialTests
    {
        /// <summary>
        /// THE most important test in this file.
        ///
        /// <para>
        /// The migration that converts existing rows computes the hash in SQL as
        /// <c>encode(sha256(convert_to("PhysicalLicense", 'UTF8')), 'hex')</c>. If that disagrees
        /// with <see cref="DeviceCredential.Hash"/> by so much as a case difference, every
        /// already-provisioned device silently stops authenticating -- the node would look fine and
        /// no headset would connect.
        /// </para>
        ///
        /// <para>
        /// The expected value below is the published SHA-256 of "test", and the same expression was
        /// run against the node's own Postgres 16 and returned exactly this string.
        /// </para>
        /// </summary>
        [Fact]
        public void HashMatchesThePostgresExpressionUsedByTheMigration()
        {
            DeviceCredential.Hash("test")
                .Should().Be("9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08");
        }

        [Fact]
        public void HashIsLowercaseHexOfTheExpectedLength()
        {
            string hash = DeviceCredential.Hash(DeviceCredential.Generate());

            hash.Should().HaveLength(DeviceCredential.HashLength);
            hash.Should().MatchRegex("^[0-9a-f]+$", "the SQL side produces lowercase hex, and the two must agree");
        }

        [Fact]
        public void HashIsDeterministic()
        {
            // Required for lookup: GetByKey resolves a device by hashing the incoming credential and
            // matching it. A salted hash would force a table scan and defeat the unique index.
            string credential = DeviceCredential.Generate();

            DeviceCredential.Hash(credential).Should().Be(DeviceCredential.Hash(credential));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AnAbsentCredentialHashesToItself(string value)
        {
            // An unregistered device carries no credential and must NOT be given the hash of an
            // empty string -- that would be one shared value every such device could authenticate
            // with, and the filtered unique index deliberately excludes empties for the same reason.
            DeviceCredential.Hash(value).Should().Be(value);
        }

        [Fact]
        public void GeneratedCredentialsAreUnique()
        {
            List<string> generated = Enumerable.Range(0, 500).Select(_ => DeviceCredential.Generate()).ToList();

            generated.Distinct().Should().HaveCount(500, "a collision here would mean two devices share an identity");
        }

        [Fact]
        public void GeneratedCredentialsCarryTheFullEntropy()
        {
            string credential = DeviceCredential.Generate();

            // base64url of 32 bytes, padding stripped: 43 characters.
            credential.Should().HaveLength(43);
            credential.Should().MatchRegex("^[A-Za-z0-9_-]+$",
                "base64url so it survives a URL, a form post and a copy-paste without escaping");
        }

        [Fact]
        public void LooksHashedRecognisesOurOwnOutput()
        {
            // Keeps the conversion idempotent. Re-hashing an already-hashed value would lock every
            // device out, and a migration that can be run twice is worth being defensive about.
            DeviceCredential.LooksHashed(DeviceCredential.Hash("anything")).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("PL-123")]
        [InlineData("9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08")] // uppercase
        [InlineData("9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a0")]  // 63 chars
        [InlineData("zzz6d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08")]  // not hex
        public void LooksHashedRejectsAnythingElse(string value)
        {
            DeviceCredential.LooksHashed(value).Should().BeFalse();
        }

        [Fact]
        public void AGeneratedCredentialIsNotMistakenForAHash()
        {
            // The two must never be confused: treating a fresh credential as already-hashed would
            // store it in cleartext, which is the defect this exists to close.
            DeviceCredential.LooksHashed(DeviceCredential.Generate()).Should().BeFalse();
        }
    }
}
