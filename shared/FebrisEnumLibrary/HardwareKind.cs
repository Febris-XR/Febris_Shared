// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    /// <summary>
    /// The node's source of truth for what kind of machine a registered device is.
    ///
    /// <para>
    /// WHY AN ENUM, AND WHAT IT REVERSES. The <c>HardwareType</c> lookup table was introduced as a
    /// "replacement for enum for a more dynamic setup"
    /// (<c>Febris.ModelLibrary.Models.DataModels.HardwareType</c>). On a standalone node that
    /// dynamism was never realised: there is no node UI or API that can create a hardware type, the
    /// only writer is the startup seeder, and nothing anywhere branches on which type a device is.
    /// A closed enum is therefore what the node actually has, and saying so in the type system
    /// makes it uniform with its siblings <see cref="HardwareCondition"/>,
    /// <see cref="XRHardwareClass"/> and <see cref="LocalSoftwarePackageType"/>.
    /// </para>
    ///
    /// <para>
    /// THE TRADE, STATED. A node operator can no longer add a fourth kind without a code change and
    /// a redeploy. That capability was advertised by the seeder's idempotency guard but was never
    /// reachable from any surface the node ships.
    /// </para>
    ///
    /// <para>
    /// THIS DOES NOT REPLACE THE CARRIERS. <c>LocalHardware.HardwareTypeId</c> and
    /// <c>.HardwareTypeUUID</c> are deliberately retained as inert hub-reconciliation keys, so a
    /// node device can still be matched against the hub's own hardware-type catalog if the hub
    /// returns. The member set below mirrors the seeded rows in
    /// <c>Febris.ModelLibrary.Models.DataModels.HardwareTypeCatalog</c>, which owns the frozen
    /// UUIDs those carriers hold. Keep the two in step.
    /// </para>
    ///
    /// <para>
    /// Values are explicit and 100-spaced with a zero sentinel, matching
    /// <see cref="XRHardwareClass"/>. The sentinel matters: the persisted column is a NOT NULL
    /// integer, so every pre-existing device row lands on 0 before backfill, and 0 must mean
    /// "not yet determined" rather than a real kind.
    /// </para>
    /// </summary>
    public enum HardwareKind
    {
        [Display(Name = "Unknown")] Unknown = 0,
        [Display(Name = "Laptop PC")] LaptopPC = 100,
        [Display(Name = "Desktop PC")] DesktopPC = 200,
        [Display(Name = "Mobile Server")] MobileServer = 300
    }
}
