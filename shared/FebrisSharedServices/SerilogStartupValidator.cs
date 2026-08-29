// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Debugging;
using System;
using System.IO;

namespace Febris.SharedServices
{
    /// <summary>
    /// LOG-B1 guard against the silent "logs nowhere" failure mode. Serilog binds the sinks named
    /// in <c>Serilog:WriteTo</c> by reflection; if a named sink's package is absent from the build
    /// output, Serilog SILENTLY drops that sink -- writing only a diagnostic to its internal
    /// <see cref="SelfLog"/> -- and the host can end up with zero live sinks, logging nothing with
    /// no visible error. (This is exactly how three hosts shipped configured for a "RollingFile"
    /// sink whose package was never referenced.) This wrapper captures that SelfLog during logger
    /// construction and, if Serilog reported a binding problem, shouts on the one channel that never
    /// depends on a sink -- <see cref="Console.Error"/> -- so the failure is loud instead of
    /// invisible. It is the runtime-logging sibling of <see cref="ConfigurationPlaceholderValidator"/>
    /// (MED-6) and follows the same opt-in fail-fast convention.
    /// </summary>
    public static class SerilogStartupValidator
    {
        /// <summary>
        /// Config key for the opt-in hard-fail switch. When set to <c>true</c>, a sink/enricher
        /// binding diagnostic throws <see cref="InvalidOperationException"/> instead of writing a
        /// stderr warning. Default (absent/false) is warn-only, so wiring this in never changes
        /// startup behavior until an operator opts in.
        /// </summary>
        public const string FailFastKey = "Serilog:FailFastOnSinkBindingErrors";

        /// <summary>
        /// Builds the Serilog logger via <paramref name="loggerFactory"/> while capturing Serilog's
        /// internal <see cref="SelfLog"/>. If the configuration named a sink/enricher that could not
        /// be bound (usually a missing package -- the host would then log NOWHERE, LOG-B1), writes a
        /// loud warning to stderr -- or throws if <see cref="FailFastKey"/> is set. Returns the built
        /// logger unchanged so call sites can assign it straight to <c>Log.Logger</c>. The factory is
        /// passed as a delegate (rather than this method calling <c>ReadFrom.Configuration</c> itself)
        /// so the <c>Serilog.Settings.Configuration</c> dependency stays in the host and does not
        /// enlarge this foundation project's footprint.
        /// </summary>
        /// <param name="loggerFactory">Creates the configured logger (e.g. <c>() => new
        /// LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger()</c>).</param>
        /// <param name="configuration">Root configuration, used only to read <see cref="FailFastKey"/>.</param>
        /// <returns>The logger produced by <paramref name="loggerFactory"/>.</returns>
        public static Logger CreateAndValidate(Func<Logger> loggerFactory, IConfiguration configuration)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            StringWriter captured = new StringWriter();
            Logger logger;
            SelfLog.Enable(captured);
            try
            {
                logger = loggerFactory();
            }
            finally
            {
                SelfLog.Disable();
            }

            string diagnostics = captured.ToString();
            if (!string.IsNullOrWhiteSpace(diagnostics))
            {
                string message =
                    "[LOG-B1] Serilog reported a sink/enricher binding problem while building the logger -- " +
                    "a configured sink may be missing its package and therefore logging NOWHERE. Verify every " +
                    "Serilog:WriteTo entry has its sink package referenced. Serilog SelfLog: " +
                    diagnostics.Trim();

                bool failFast = configuration != null
                    && bool.TryParse(configuration[FailFastKey], out bool parsed) && parsed;
                if (failFast)
                {
                    logger?.Dispose();
                    throw new InvalidOperationException(
                        message + " (set " + FailFastKey + "=false to downgrade this to a startup warning.)");
                }

                Console.Error.WriteLine(message);
            }

            return logger;
        }
    }
}
