// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;

namespace Febris.SharedServices.Launcher
{
    public class SimulationReflector
    {
        private ILogger _log;
        private IConfiguration _config;

        public SimulationReflector(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public SimulationReflector()
        {
        }

        public SimulationReflector(ILogger log)
        {
            _log = log;
        }

        public bool SimulationRunning()
        {
            try
            {
                return LauncherSharedDetails.SimulationIsRunning;
            }
            catch (System.Exception ex)
            {
                _log.LogError(ex, "SimulationReflector.SimulationRunning: suppressed exception");
                return false;
            }
        }

        public void SimulationRunningInitalization(string name, int id)
        {
            try
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("simulationRunningCheck", 10000))
                {
                    bool simulationCheck = true;
                    bool mutexCreated;
                    Mutex mutex = new Mutex(true, "simulationrunningmutex", out mutexCreated);
                    using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                    {
                        BinaryWriter writer = new BinaryWriter(stream);
                        // Audit P-15 note (2026-05-20): Thread.Sleep is
                        // intentional here. SimulationReflector runs in the
                        // launcher PROCESS (separate exe spawned per simulation),
                        // not in any web request thread. The 2-second polling
                        // loop is fine because we own the whole process.
                        // DO NOT instantiate this class from any API request --
                        // it would pin a Kestrel worker for the simulation
                        // lifetime. If the launcher ever needs to be hosted
                        // in-process (e.g., container deploy), convert to
                        // Task.Delay + async/await before that change.
                        while (simulationCheck)
                        {
                            simulationCheck = SimulationIsRunningCheck(name, id);
                            // FIX (PC-B4): reset stream position so each poll overwrites the same status byte instead of appending.
                            stream.Position = 0;
                            if (simulationCheck)
                            {
                                writer.Write(1);
                            }
                            else
                            {
                                writer.Write(0);
                            }
                            // FIX (PC-B4): flush so the reader process sees the latest byte on each poll.
                            writer.Flush();
                            //wait two seconds
                            Thread.Sleep(2000);
                        }
                    }
                    mutex.ReleaseMutex();

                    //this is my custom code
                    //SharedDetails.SharedDetails.SimulationIsRunning = simulationCheck;
                    //while (simulationCheck)
                    //{
                    //    simulationCheck = SimulationIsRunningCheck(name, id);
                    //    SharedDetails.SharedDetails.SimulationIsRunning = simulationCheck;
                    //    //wait two seconds
                    //    Thread.Sleep(2000);
                    //}                
                    mmf.Dispose();
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
        }

        public bool SimulationIsRunning()
        {
            bool simulationIsRunning = false;
            try
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("simulationRunningCheck"))
                {
                    Mutex mutex = Mutex.OpenExisting("simulationrunningmutex");
                    // FIX (PC-B5): acquire the mutex before reading so the paired ReleaseMutex below owns the lock (releasing an unheld mutex throws). Restores cross-process sync.
                    mutex.WaitOne();
                    using (MemoryMappedViewStream stream = mmf.CreateViewStream(0,1))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        // FIX (PC-B4): reset stream position before reading the status byte so each poll reads byte 0.
                        stream.Position = 0;
                        if (reader.ReadBoolean())
                        {
                            simulationIsRunning = true;
                        }
                    }
                    mutex.ReleaseMutex();
                }
                return simulationIsRunning;
            }
            catch (System.Exception ex)
            {
                _log.LogError(ex, "SimulationReflector.SimulationIsRunning: suppressed exception");
                //_log.LogError(ex.Message);
            return simulationIsRunning;
            }
        }
                

        private bool SimulationIsRunningCheck(string name, int id)
        {
            try
            {
                bool running = false;
                Process process = Process.GetProcessById(id);
                if (process != null)
                {
                    if (process.ProcessName != name)
                    {
                        running= false;
                    }
                    else
                    {
                        running= true;
                    }
                }
                else { running= false; }
                return running;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            
            return false;
            }
        }
    }
}
