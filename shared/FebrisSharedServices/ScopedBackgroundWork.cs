// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Febris.SharedServices
{
    /// <summary>
    /// Helper for the SCBA-B3 fire-and-forget pattern: a BLL wants to record an
    /// analytics write on a background thread without making the caller wait, but
    /// it must NOT capture the request-scoped DbContext -- the request scope is
    /// disposed when the request returns, and the background task would then race a
    /// disposed (or concurrently-used) context.
    /// <para>
    /// <see cref="FireAndForget{TService}"/> resolves a fresh <typeparamref name="TService"/>
    /// from a new DI scope created inside the background task, so the write owns and
    /// disposes its own scoped DbContext. Exceptions are swallowed and logged (an
    /// analytics write must never crash or fault an unobserved task).
    /// </para>
    /// <para>
    /// When <paramref name="scopeFactory"/> is null -- a BLL constructed through a
    /// legacy (non-DI) constructor that did not receive an IServiceScopeFactory --
    /// the supplied <paramref name="legacyFallback"/> runs instead, preserving the
    /// pre-fix behavior exactly. This keeps the change strangler-safe.
    /// </para>
    /// <para>
    /// EXTRACTED from FebrisSharedLogicLayer to SharedServices (node hygiene D) so the
    /// EndUser tier -- whose BLL deliberately does NOT reference the central shared BLL --
    /// can port its analytics fire-and-forget sites to the same pattern. Extraction, not a
    /// copy, per the duplicate-type drift guard's own prescription; the shared BLL call
    /// sites resolve it unchanged through their existing <c>using Febris.SharedServices</c>.
    /// </para>
    /// </summary>
    public static class ScopedBackgroundWork
    {
        public static void FireAndForget<TService>(
            IServiceScopeFactory scopeFactory,
            Func<TService, Task> work,
            Func<Task> legacyFallback = null)
        {
            if (scopeFactory != null)
            {
                _ = Task.Run(async () =>
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        try
                        {
                            await work(scope.ServiceProvider.GetRequiredService<TService>());
                        }
                        catch (Exception ex)
                        {
                            Febris.SharedServices.FebrisLog.Error(ex, "Scoped background work failed (" + typeof(TService).Name + ").");
                        }
                    }
                });
            }
            else if (legacyFallback != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await legacyFallback();
                    }
                    catch (Exception ex)
                    {
                        Febris.SharedServices.FebrisLog.Error(ex, "Background work (legacy fallback) failed.");
                    }
                });
            }
        }
    }
}
