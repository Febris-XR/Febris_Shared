// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.IO;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Xunit;

namespace Febris.SharedServices.Tests
{
    // This class swaps the process-global Console.Error and toggles Serilog's process-global
    // SelfLog, so it must not run in parallel with any other collection (cf. TEST-B1 flakiness).
    [CollectionDefinition("SerilogStartupValidator", DisableParallelization = true)]
    public class SerilogStartupValidatorCollection { }

    /// <summary>
    /// LOG-B1: pins <see cref="SerilogStartupValidator"/>. The regression it guards is a Serilog
    /// sink named in <c>WriteTo</c> whose package is missing from the build output -- Serilog drops
    /// it silently (SelfLog only) and the host logs NOWHERE. These tests assert the validator (a)
    /// returns the built logger untouched, (b) shouts to stderr when Serilog reports a binding
    /// problem, (c) hard-fails only when the opt-in key is set, and (d) that the real "File" sink
    /// resolves by name through ReadFrom.Configuration and actually writes -- the exact host path.
    /// </summary>
    [Collection("SerilogStartupValidator")]
    public class SerilogStartupValidatorTests
    {
        private static IConfiguration BuildConfig(Dictionary<string, string> values)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        /// <summary>Runs <paramref name="action"/> with Console.Error redirected, returns what was written.</summary>
        private static string CaptureStdErr(Action action)
        {
            TextWriter original = Console.Error;
            StringWriter buffer = new StringWriter();
            Console.SetError(buffer);
            try
            {
                action();
            }
            finally
            {
                Console.SetError(original);
            }
            return buffer.ToString();
        }

        [Fact]
        public void CreateAndValidate_NullFactory_Throws()
        {
            Action act = () => SerilogStartupValidator.CreateAndValidate(null, BuildConfig(new Dictionary<string, string>()));

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateAndValidate_CleanLogger_ReturnsItAndWritesNoWarning()
        {
            IConfiguration config = BuildConfig(new Dictionary<string, string>());
            Logger produced = new LoggerConfiguration().CreateLogger();

            Logger returned = null;
            string stderr = CaptureStdErr(() =>
            {
                returned = SerilogStartupValidator.CreateAndValidate(() => produced, config);
            });

            returned.Should().BeSameAs(produced);
            stderr.Should().BeEmpty();
            returned.Dispose();
        }

        [Fact]
        public void CreateAndValidate_SinkBindingError_WarnsToStderrByDefault()
        {
            // A sink that cannot bind is exactly what Serilog reports via SelfLog while building the
            // logger; simulate that from inside the factory (SelfLog is enabled by the validator here).
            IConfiguration config = BuildConfig(new Dictionary<string, string>());
            Logger produced = new LoggerConfiguration().CreateLogger();

            Logger returned = null;
            string stderr = CaptureStdErr(() =>
            {
                returned = SerilogStartupValidator.CreateAndValidate(
                    () =>
                    {
                        SelfLog.WriteLine("Unable to find a method called File for supplied arguments.");
                        return produced;
                    },
                    config);
            });

            returned.Should().BeSameAs(produced);          // never swallows the logger
            stderr.Should().Contain("[LOG-B1]");           // shouts loudly...
            stderr.Should().Contain("logging NOWHERE");    // ...with the actionable reason
            returned.Dispose();
        }

        [Fact]
        public void CreateAndValidate_SinkBindingError_ThrowsWhenFailFastEnabled()
        {
            IConfiguration config = BuildConfig(new Dictionary<string, string>
            {
                { SerilogStartupValidator.FailFastKey, "true" },
            });

            Action act = () => CaptureStdErr(() =>
                SerilogStartupValidator.CreateAndValidate(
                    () =>
                    {
                        SelfLog.WriteLine("Unable to find a method called Bogus for supplied arguments.");
                        return new LoggerConfiguration().CreateLogger();
                    },
                    config));

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*LOG-B1*");
        }

        [Fact]
        public void CreateAndValidate_DisablesSelfLog_AfterBuild()
        {
            // The validator enables SelfLog only around the factory call; afterward it must be off so
            // later runtime sink errors aren't captured by a stale writer. Prove it behaviorally: a
            // writer installed before the call receives nothing written to SelfLog after the call.
            IConfiguration config = BuildConfig(new Dictionary<string, string>());
            StringWriter preInstalled = new StringWriter();
            SelfLog.Enable(preInstalled);
            try
            {
                using Logger logger = SerilogStartupValidator.CreateAndValidate(
                    () => new LoggerConfiguration().CreateLogger(), config);

                SelfLog.WriteLine("post-build diagnostic that must go nowhere");
                preInstalled.ToString().Should().NotContain("post-build",
                    "the validator must leave SelfLog disabled, not routed to any writer");
            }
            finally
            {
                SelfLog.Disable();
            }
        }

        [Fact]
        public void CreateAndValidate_FileSinkFromConfiguration_ResolvesByNameAndWrites()
        {
            // The real LOG-B1 outcome: the production Serilog block ("Name": "File", path, daily
            // rolling, JSON formatter) resolves the Serilog.Sinks.File sink by name and writes a file.
            string dir = Path.Combine(Path.GetTempPath(), "febris-logb1-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                IConfiguration config = BuildConfig(new Dictionary<string, string>
                {
                    { "Serilog:MinimumLevel:Default", "Information" },
                    { "Serilog:WriteTo:0:Name", "File" },
                    { "Serilog:WriteTo:0:Args:path", Path.Combine(dir, "log-.json") },
                    { "Serilog:WriteTo:0:Args:rollingInterval", "Day" },
                    { "Serilog:WriteTo:0:Args:formatter", "Serilog.Formatting.Json.JsonFormatter, Serilog" },
                });

                string stderr = CaptureStdErr(() =>
                {
                    Logger logger = SerilogStartupValidator.CreateAndValidate(
                        () => new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger(),
                        config);
                    logger.Information("logb1 smoke {Marker}", "MARK-42");
                    logger.Dispose(); // flushes + releases the file
                });

                stderr.Should().NotContain("[LOG-B1]"); // a valid config produces no binding warning

                string[] written = Directory.GetFiles(dir, "log-*.json");
                written.Should().NotBeEmpty("the File sink named in config must resolve and create a dated file");
                File.ReadAllText(written[0]).Should().Contain("MARK-42");
            }
            finally
            {
                try { Directory.Delete(dir, recursive: true); } catch { /* best-effort temp cleanup */ }
            }
        }
    }
}
