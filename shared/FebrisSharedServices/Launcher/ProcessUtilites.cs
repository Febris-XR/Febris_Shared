// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using FebrisLocalLibrary.SharedDetails;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Febris.SharedServices.Launcher
{
    //This obviously needs work.
    public class ProcessUtilites
    {
        public void StartProcess(ProcessOptions processType)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processType.ToString(),
                    CreateNoWindow = true,

                }

            };
            process.Start();
        }

        public void StopProcess(ProcessOptions processType)
        {
            string processName = Path.GetFileNameWithoutExtension(processType.ToString());

            foreach (var process in Process.GetProcessesByName(processName))
            {
                process.Kill();
            }
        }
                
    }
}
