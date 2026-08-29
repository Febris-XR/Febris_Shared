// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// MED-6: pins the unresolved-placeholder detection. The security-relevant assertions are that
    /// a whole-value {Token} is flagged (it would otherwise reach prod as a literal connection
    /// string / secret) while an EMBEDDED brace token (a Serilog "log-{Date}.json" pathFormat) is
    /// NOT flagged, and that the throw is opt-in + skipped in Development.
    /// </summary>
    public class ConfigurationPlaceholderValidatorTests
    {
        private static IConfiguration BuildConfig(Dictionary<string, string> values)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        [Fact]
        public void FindUnresolvedPlaceholders_FlagsWholeValueTokens_IgnoresRealAndEmbedded()
        {
            IConfiguration config = BuildConfig(new Dictionary<string, string>
            {
                { "ConnectionStrings:DataDBConnection", "{DataDBConnectionString}" }, // placeholder -> flag
                { "Smb:Secret", "{SmbClientSecret}" },                                 // placeholder -> flag
                { "ConnectionStrings:Real", "Server=127.0.0.1;Db=x;" },                // real -> ignore
                { "Serilog:pathFormat", "C:\\Logs\\log-{Date}.json" },                 // embedded token -> ignore
                { "Empty", "" },                                                        // empty -> ignore
            });

            List<string> result = ConfigurationPlaceholderValidator.FindUnresolvedPlaceholders(config);

            result.Should().BeEquivalentTo(new[] { "ConnectionStrings:DataDBConnection", "Smb:Secret" });
        }

        [Fact]
        public void Validate_InDevelopment_IsNoOp_EvenWithPlaceholders()
        {
            IConfiguration config = BuildConfig(new Dictionary<string, string>
            {
                { "ConnectionStrings:DataDBConnection", "{DataDBConnectionString}" },
            });

            ConfigurationPlaceholderValidator.Validate(config, isDevelopment: true).Should().BeEmpty();
        }

        [Fact]
        public void Validate_NonDevelopment_NoFailFastFlag_ReturnsKeysWithoutThrowing()
        {
            IConfiguration config = BuildConfig(new Dictionary<string, string>
            {
                { "ConnectionStrings:DataDBConnection", "{DataDBConnectionString}" },
            });

            List<string> result = ConfigurationPlaceholderValidator.Validate(config, isDevelopment: false);

            result.Should().ContainSingle().Which.Should().Be("ConnectionStrings:DataDBConnection");
        }

        [Fact]
        public void Validate_NonDevelopment_FailFastEnabled_Throws_WithOffendingKeys()
        {
            IConfiguration config = BuildConfig(new Dictionary<string, string>
            {
                { "ConnectionStrings:DataDBConnection", "{DataDBConnectionString}" },
                { ConfigurationPlaceholderValidator.FailFastKey, "true" },
            });

            System.Action act = () => ConfigurationPlaceholderValidator.Validate(config, isDevelopment: false);

            act.Should().Throw<System.InvalidOperationException>()
               .WithMessage("*ConnectionStrings:DataDBConnection*");
        }

        [Fact]
        public void Validate_NonDevelopment_AllResolved_ReturnsEmpty()
        {
            IConfiguration config = BuildConfig(new Dictionary<string, string>
            {
                { "ConnectionStrings:DataDBConnection", "Server=db;Database=x;" },
            });

            ConfigurationPlaceholderValidator.Validate(config, isDevelopment: false).Should().BeEmpty();
        }
    }
}
