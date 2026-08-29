// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// A one-time token that lets whoever can read the node's startup output claim it by creating
    /// the first ITAdmin account (first-run claim, 2026-08-21).
    ///
    /// <para>
    /// WHY THIS REPLACED A HARDCODED SEED. The node used to seed a bootstrap admin at a compiled-in
    /// default address. That is a reasonable shape for unattended automation and a poor one for an
    /// open-source project: it puts an admin password in a file on disk, it requires editing
    /// configuration BEFORE first boot, and in Release with nothing configured it produced an
    /// account at a reserved example.com address with no password -- an account nobody could ever
    /// sign in to, because that domain cannot receive the password-reset mail it depended on.
    /// </para>
    ///
    /// <para>
    /// THE TRUST BOUNDARY IS THE NODE'S STDOUT, deliberately and by owner decision. The token is
    /// written with <c>Console.WriteLine</c> and NOT through Serilog, so it reaches
    /// <c>docker compose logs</c> and never the file sink, never a log shipper, and never disk. The
    /// claim window is therefore not open to anyone who can reach the node over the network. It is
    /// open to whoever can read the server's console, which is the operator by definition.
    /// </para>
    ///
    /// <para>
    /// Same primitive as <see cref="NodeUserInvite"/> and the device credential: 256 bits from a
    /// CSPRNG, stored ONLY as a lowercase-hex SHA-256, single use, and expiring. The row survives
    /// after consumption on purpose, as the audit record of when the node was claimed and by whom.
    /// </para>
    /// </summary>
    public class NodeSetupToken : BaseModel
    {
        /// <summary>
        /// Lowercase hex SHA-256 of the setup token, produced by
        /// <c>Febris.SharedServices.DeviceCredential.Hash</c>. The token itself is never stored,
        /// never logged through Serilog, and never shown again after the boot that minted it.
        /// </summary>
        [Required]
        [Display(Name = "Token hash")]
        public string TokenHash { get; set; }

        /// <summary>
        /// Hard expiry (UTC). A short window is the SAFE direction here rather than an
        /// inconvenience: once it lapses the setup page refuses everything, so an unclaimed node
        /// left running is less claimable over time, not more. Restarting the node mints a fresh
        /// token and prints it again.
        /// </summary>
        [Display(Name = "Expires (UTC)")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>Set when the token is redeemed. Non-null means it cannot be used again.</summary>
        [Display(Name = "Claimed (UTC)")]
        public DateTime? ConsumedAt { get; set; }

        /// <summary>Identity id of the ITAdmin account created by claiming the node. Null while the
        /// token is outstanding.</summary>
        [Display(Name = "Claimed by")]
        public Guid? ConsumedByUserId { get; set; }

        /// <summary>Email of the account created by claiming the node, denormalized so the audit
        /// record stays readable after that account is renamed or removed.</summary>
        [Display(Name = "Claimed by")]
        public string ConsumedByEmail { get; set; }
    }
}
