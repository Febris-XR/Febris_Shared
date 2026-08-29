// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;
using System.Threading.Tasks;
using Febris.ModelLibrary.Models.DataModels;
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="NameGenerator.GenerateName"/>.
    ///
    /// <para>
    /// The generator is non-deterministic (uses <c>System.Random</c>), so these tests assert
    /// on the SHAPE of the output rather than exact values: presence of fields, expected
    /// formats, valid GUIDs, etc. A separate "produces variety" test exercises the random
    /// generator many times to verify it is not always returning the same user.
    /// </para>
    /// </summary>
    public class NameGeneratorTests
    {
        [Fact]
        public async Task GenerateName_ReturnsNonNullTestUser()
        {
            var user = await NameGenerator.GenerateName();

            user.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateName_FirstNameIsPopulated()
        {
            var user = await NameGenerator.GenerateName();

            user.FirstName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GenerateName_LastNameIsPopulated()
        {
            var user = await NameGenerator.GenerateName();

            user.LastName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GenerateName_IdentificationNumberIsValidGuid()
        {
            // The generator builds the identification number with Guid.NewGuid().ToString().
            var user = await NameGenerator.GenerateName();

            Guid parsed;
            Guid.TryParse(user.IdentificationNumber, out parsed).Should().BeTrue();
            parsed.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task GenerateName_PhoneNumberIsNineDigits()
        {
            // The generator uses Random.Next(100_000_000, 999_999_999), so the formatted value
            // is always 9 digits in the range [100000000, 999999999].
            var user = await NameGenerator.GenerateName();

            user.PhoneNumber.Should().MatchRegex("^[1-9][0-9]{8}$");
        }

        [Fact]
        public async Task GenerateName_EmailMatchesExpectedPattern()
        {
            // Format produced: <firstName>_<lastName><guid>@email.com
            var user = await NameGenerator.GenerateName();

            user.EmailAddress.Should().StartWith(user.FirstName + "_" + user.LastName);
            user.EmailAddress.Should().EndWith("@email.com");
            user.EmailAddress.Should().Contain("_");
        }

        [Fact]
        public async Task GenerateName_UserNameComposesFirstUnderscoreLastIdentificationNumber()
        {
            // Format produced: <firstName>_<lastName><identificationNumber>
            var user = await NameGenerator.GenerateName();

            user.UserName.Should().Be(user.FirstName + "_" + user.LastName + user.IdentificationNumber);
        }

        [Fact]
        public async Task GenerateName_AcrossManyInvocations_ProducesAtLeastSomeVariety()
        {
            // Generate 25 users and confirm there's more than one distinct first name.
            // With ~1000+ first names in the pool, identical draws happen but the *whole* batch
            // being the same first name is astronomically unlikely.
            const int sampleSize = 25;
            var users = new TestUser[sampleSize];
            for (var i = 0; i < sampleSize; i++)
            {
                users[i] = await NameGenerator.GenerateName();
            }

            var distinctFirstNames = users.Select(u => u.FirstName).Distinct().Count();
            distinctFirstNames.Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task GenerateName_AcrossManyInvocations_ProducesUniqueIdentificationNumbers()
        {
            // GUIDs should never collide across a small batch.
            const int sampleSize = 25;
            var users = new TestUser[sampleSize];
            for (var i = 0; i < sampleSize; i++)
            {
                users[i] = await NameGenerator.GenerateName();
            }

            users.Select(u => u.IdentificationNumber).Distinct().Should().HaveCount(sampleSize);
        }
    }
}
