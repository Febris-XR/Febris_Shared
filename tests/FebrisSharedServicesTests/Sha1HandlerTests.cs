// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="Sha1Handler"/>.
    ///
    /// <para>
    /// <see cref="Sha1Handler.TextToHash"/> wraps <c>SHA1.Create()</c> and renders the digest
    /// as a lowercase hex string. SHA-1 produces 160 bits = 40 hex characters.
    /// </para>
    /// </summary>
    public class Sha1HandlerTests
    {
        [Fact]
        public void TextToHash_ReturnsFortyCharacterHexString()
        {
            // SHA-1 always produces a 160-bit digest -> 40 lowercase hex chars.
            var result = Sha1Handler.TextToHash("anything");

            result.Should().HaveLength(40);
            result.Should().MatchRegex("^[0-9a-f]{40}$");
        }

        [Fact]
        public void TextToHash_IsDeterministic_SameInputProducesSameDigest()
        {
            // The hash must be a pure function: identical inputs always produce identical outputs.
            var first  = Sha1Handler.TextToHash("febris");
            var second = Sha1Handler.TextToHash("febris");

            second.Should().Be(first);
        }

        [Fact]
        public void TextToHash_DifferentInputs_ProduceDifferentDigests()
        {
            // Different inputs should produce different outputs (with overwhelming probability).
            var first  = Sha1Handler.TextToHash("febris");
            var second = Sha1Handler.TextToHash("Febris");

            second.Should().NotBe(first);
        }

        [Theory]
        // Well-known SHA-1 test vectors. If these change, the implementation has drifted.
        [InlineData("",      "da39a3ee5e6b4b0d3255bfef95601890afd80709")]
        [InlineData("abc",   "a9993e364706816aba3e25717850c26c9cd0d89d")]
        [InlineData("hello", "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d")]
        public void TextToHash_KnownVectors_MatchExpectedDigests(string input, string expectedDigest)
        {
            var result = Sha1Handler.TextToHash(input);

            result.Should().Be(expectedDigest);
        }

        [Fact]
        public void TextToHash_WithUnicodeInput_ProducesDifferentDigestThanAsciiEquivalent()
        {
            // UTF-8 encoding is implied by `Encoding.UTF8.GetBytes(text)` in the implementation.
            // To prove the encoding is UTF-8 (not, say, ASCII with stripped accents), we verify
            // that the digest of a unicode string differs from the digest of its ASCII
            // transliteration. We deliberately avoid pinning a specific digest because the
            // exact bytes depend on Unicode normalization (NFC vs NFD) of the source file.
            var unicode = Sha1Handler.TextToHash("Café");  // C, a, f, é (U+00E9)
            var ascii   = Sha1Handler.TextToHash("Cafe");

            unicode.Should().NotBe(ascii);
            unicode.Should().HaveLength(40);
            unicode.Should().MatchRegex("^[0-9a-f]{40}$");
        }

        [Fact]
        public void TextToHash_WithNullInput_ThrowsArgumentNullException()
        {
            // Edge case: the implementation does not null-check its argument and relies on
            // Encoding.UTF8.GetBytes(null) to throw. This pins that behavior. If the platform
            // adds null-safety later, this test should change to assert the new contract.
            Action act = () => Sha1Handler.TextToHash(null);

            act.Should().Throw<ArgumentNullException>();
        }
    }

    /// <summary>
    /// Tests for <see cref="ShaHandler"/>.
    ///
    /// <para>
    /// <see cref="ShaHandler.TextToSha2"/> wraps <c>SHA256.Create()</c> and renders the digest
    /// as a lowercase hex string. SHA-256 produces 256 bits = 64 hex characters.
    /// </para>
    /// </summary>
    public class ShaHandlerTests
    {
        [Fact]
        public void TextToSha2_ReturnsSixtyFourCharacterHexString()
        {
            // SHA-256 always produces a 256-bit digest -> 64 lowercase hex chars.
            var result = ShaHandler.TextToSha2("anything");

            result.Should().HaveLength(64);
            result.Should().MatchRegex("^[0-9a-f]{64}$");
        }

        [Theory]
        [InlineData("",      "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [InlineData("abc",   "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")]
        [InlineData("hello", "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")]
        public void TextToSha2_KnownVectors_MatchExpectedDigests(string input, string expectedDigest)
        {
            var result = ShaHandler.TextToSha2(input);

            result.Should().Be(expectedDigest);
        }

        [Fact]
        public void TextToSha2_IsDeterministic_SameInputProducesSameDigest()
        {
            var first  = ShaHandler.TextToSha2("febris");
            var second = ShaHandler.TextToSha2("febris");

            second.Should().Be(first);
        }

        [Fact]
        public void TextToSha2_WithNullInput_ThrowsArgumentNullException()
        {
            // Same null behavior as Sha1Handler.TextToHash -- pins current contract.
            Action act = () => ShaHandler.TextToSha2(null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
