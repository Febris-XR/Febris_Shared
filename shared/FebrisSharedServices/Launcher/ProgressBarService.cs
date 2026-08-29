// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary.LauncherEnums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Febris.SharedServices.Launcher
{
    public class ProgressBarService
    {
        #region progress bar call backs
        /// <summary>
        /// Starts the cosmetic progress-bar window, and returns null if it cannot be started.
        ///
        /// FOUND 2026-08-25. This used to let a Win32Exception escape, and two callers on the
        /// recording path invoke it OUTSIDE their try block, so a missing or unresolvable
        /// Febris.ConsoleProgressBar.exe did not degrade the progress bar, it destroyed the work:
        ///
        ///   pc/FebrisPCScreenRecorder/CoreOperations/ScreenRecorder.cs -- called immediately
        ///   before SaveVideo, so the throw meant the captured frames were NEVER encoded and the
        ///   outer catch swallowed the reason. A recorded session silently produced no mp4.
        ///
        ///   pc/FebrisPCStatementManager/Services/VideoFileProcessing.cs -- called on the line
        ///   before the try, so the throw propagated to the folder checker and aborted video
        ///   processing for that cycle, every cycle, permanently.
        ///
        /// The path is resolved from the current working directory, so this is reachable whenever
        /// the CWD is not the install directory -- which for a Topshelf service running as
        /// LocalSystem is the normal case, not an edge case.
        ///
        /// A decorative window must never be able to prevent a recording from being encoded or
        /// uploaded. It fails soft now, and the callers null-check.
        /// </summary>
        public static Process StartProgressBar(string fileName, StatusType statusType)
        {
            string launchPathString = PCFileSystem.ProgressBarPath;
            string args = "Title|" + fileName + " Status|" + statusType.ToString();
            //string title = "Title|" + fileName;
            //string status = "Status|" + false.ToString();
            try
            {
                Process process = new Process();

                process.StartInfo = new ProcessStartInfo(launchPathString);
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = true;
                process.Start();
                return process;
            }
            catch (Exception ex)
            {
                // Deliberately not rethrown. The caller's real work continues without a progress
                // bar, which is the whole point of this change.
                Febris.SharedServices.FebrisLog.Warn(
                    "ProgressBarService.StartProgressBar: could not start the progress bar at '"
                    + launchPathString + "', continuing without it. " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Null-safe by design: <see cref="StartProgressBar"/> returns null when it could not start,
        /// and every caller passes that result straight back here.
        /// </summary>
        public static void StopProgressBar(Process process)
        {
            if (process == null)
            {
                return;
            }

            try
            {
                process.CloseMainWindow();
            }
            catch (Exception ex)
            {
                // A progress bar that has already exited throws here. Same rule as starting it:
                // never let the decoration fail the work.
                Febris.SharedServices.FebrisLog.Warn(
                    "ProgressBarService.StopProgressBar: could not close the progress bar. " + ex.Message);
            }
        }

        #endregion
    }
}
