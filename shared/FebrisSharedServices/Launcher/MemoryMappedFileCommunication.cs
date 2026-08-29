// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Threading;

namespace Febris.SharedServices.Launcher
{
    class MemoryMappedFileCommunication
    {
        public bool CheckingMappedFiles()
        {
            return false;
        }

        private void OpenMemoryFile()
        {

        }

        private void CreateNewMemoryFile(string name, int id)
        {
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateNew(name, 10000))
            {
                bool mutexCreated;
                Mutex mutex = new Mutex(true, name, out mutexCreated);
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(1);
                }
                mutex.ReleaseMutex();
            }
        }

        private void DataFileCleanUp()
        {

        }
    }
}
