// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="PasswordGenerator"/>.
    ///
    /// <para>
    /// The generator behaves identically in every build configuration: a random 20+ character
    /// password meeting Identity password policy, drawn from a CSPRNG. These tests carry no
    /// <c>#if</c> guards on purpose -- a configuration that generated weaker or predictable
    /// passwords would be a defect, so there is no branch for the tests to mirror.
    /// </para>
    /// </summary>
    public class PasswordGeneratorTests
    {
        private readonly IPasswordGenerator _sut = new PasswordGenerator();

        [Fact]
        public void PasswordGenerator_ImplementsIPasswordGeneratorInterface()
        {
            // Sanity check on the contract: callers should depend on the interface, not the concrete class.
            _sut.Should().BeAssignableTo<IPasswordGenerator>();
        }

        [Fact]
        public void PasswordRandomize_HasAtLeastTwentyCharacters()
        {
            // PasswordOptions.RequiredLength = 20.
            var password = _sut.PasswordRandomize();

            password.Length.Should().BeGreaterOrEqualTo(20);
        }

        [Fact]
        public void PasswordRandomize_ContainsUppercaseLowercaseDigitAndSymbol()
        {
            // PasswordOptions requires all four character classes.
            var password = _sut.PasswordRandomize();

            password.Any(char.IsUpper).Should().BeTrue("password must include uppercase");
            password.Any(char.IsLower).Should().BeTrue("password must include lowercase");
            password.Any(char.IsDigit).Should().BeTrue("password must include a digit");
            password.Any(c => "!@$?_-".Contains(c)).Should().BeTrue("password must include a non-alphanumeric");
        }

        [Fact]
        public void PasswordRandomize_HasAtLeastFourDistinctCharacters()
        {
            // PasswordOptions.RequiredUniqueChars = 4.
            var password = _sut.PasswordRandomize();

            password.Distinct().Count().Should().BeGreaterOrEqualTo(4);
        }

        [Fact]
        public void PasswordRandomize_ProducesVarietyAcrossCalls()
        {
            // With a 20+ char random string drawn from ~80 char classes, two consecutive calls
            // should not produce the same string. (A flaky test in principle; in practice the
            // probability of collision is astronomical.)
            var first  = _sut.PasswordRandomize();
            var second = _sut.PasswordRandomize();

            second.Should().NotBe(first);
        }

        [Fact]
        public void PasswordRandomize_NeverReturnsTheRetiredDebugConstant()
        {
            // Regression guard for SEC-PWGEN-1: DEBUG builds used to return this literal for
            // every generated password. No configuration may reintroduce a fixed password.
            for (int i = 0; i < 32; i++)
            {
                _sut.PasswordRandomize().Should().NotBe("Password123!!!");
            }
        }
    }
}
