// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using Serilog;

namespace Febris.SharedServices
{
    /// <summary>
    /// Static logging facade (audit MED-2). Replaces the pervasive
    /// catch-and-print-to-stdout anti-pattern so swallowed
    /// exceptions reach the configured structured logger instead of stdout.
    /// Backed by Serilog's global Log, which the hosts configure via UseSerilog
    /// at startup. A single facade keeps the backing swappable and avoids
    /// injecting an ILogger into hundreds of shared classes.
    /// <para>
    /// If no Serilog logger has been configured in the running host, Serilog's
    /// global Log is a silent no-op, so this never throws. In every deployed
    /// host UseSerilog is wired, so errors are captured.
    /// </para>
    /// </summary>
    public static class FebrisLog
    {
        /// <summary>
        /// Log an exception. This is the dominant replacement for the catch-block
        /// "write the stack trace or message to stdout" pattern.
        /// </summary>
        public static void Error(Exception ex, string context = null)
        {
            if (ex == null)
            {
                Log.Error("{Message}", context ?? "Error reported with no exception.");
                return;
            }
            Log.Error(ex, "{Message}", context ?? "Unhandled exception.");
        }

        /// <summary>Log an error message that has no associated exception.</summary>
        public static void ErrorMessage(string message) => Log.Error("{Message}", message);

        /// <summary>Log an informational message (replacement for non-error Console.WriteLine).</summary>
        public static void Info(object message) => Log.Information("{Message}", message?.ToString() ?? string.Empty);

        /// <summary>Log a warning.</summary>
        public static void Warn(object message) => Log.Warning("{Message}", message?.ToString() ?? string.Empty);
    }
}
