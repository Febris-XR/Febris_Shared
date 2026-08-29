// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Febris.SharedServices.Launcher
{
    /// <summary>
    /// This is not currently in use but could be useful in the future.
    /// </summary>
    internal static class AsyncAwaiter
    {
        /// <summary>
        /// A semaphore to lock the semaphore list
        /// </summary>
        private static SemaphoreSlim SelfLock = new SemaphoreSlim(1, 20);

        /// <summary>
        /// list of all semaphore locks(one per key)
        /// </summary>
        private static Dictionary<string, SemaphoreSlim> Semaphores = new Dictionary<string, SemaphoreSlim>();

        public static async Task<T> AwaitResultAsync<T>(string key, Func<Task<T>> task, int maxAccessCount = 1)
        {
            #region Create Semaphore
            ///Asynchronously wait to enter the semaphore
            ///
            ///It no-one has been granted access to the semaphore 
            ///code execution will proceed
            ///otherwise this thread waits here until the semaphore is released
            await SelfLock.WaitAsync();

            try
            {
                //create semaphore if does not already exist
                if (!Semaphores.ContainsKey(key)) 
                    Semaphores.Add(key, new SemaphoreSlim(maxAccessCount, maxAccessCount));
            }
            finally
            {
                //when the task is ready, release the semaphore
                //
                //If semaphore is not released it will forever be locked
                SelfLock.Release();
            }
            #endregion

            var semaphore = Semaphores[key];
            
            await semaphore.WaitAsync();
            try
            {
                return await task();
            }            
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task AwaitAsync(string key, Func<Task> task, int maxAccessCount = 1)
        {
            #region Create Semaphore
            ///Asynchronously wait to enter the semaphore
            ///
            ///It no-one has been granted access to the semaphore 
            ///code execution will proceed
            ///otherwise this thread waits here until the semaphore is released
            await SelfLock.WaitAsync();

            try
            {
                //create semaphore if does not already exist
                if (!Semaphores.ContainsKey(key)) Semaphores.Add(key, new SemaphoreSlim(maxAccessCount, maxAccessCount));
            }
            finally
            {
                //when the task is ready, release the semaphore
                //
                //If semaphore is not released it will forever be locked
                SelfLock.Release();
            }
            #endregion

            var semaphore = Semaphores[key];
            await semaphore.WaitAsync();
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "AsyncAwaiter.AwaitAsync: suppressed exception");
                var error = ex.Message;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}