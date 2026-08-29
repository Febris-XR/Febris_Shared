// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// An invitation to create an account on THIS user node, issued by an admin or educator and
    /// redeemed exactly once by the recipient. The account does not exist until the invitation is
    /// accepted, so an invitation that is never taken up leaves nothing behind.
    ///
    /// <para>
    /// NODE-ONLY. Not to be confused with <see cref="ContentDeveloperUserInvite"/>, which is the
    /// central tier's invite into a developer org. This is modelled on it, but deliberately differs
    /// on three points that the older type documents as unfixed defects:
    /// </para>
    ///
    /// <list type="number">
    /// <item><b>The token is HASHED at rest.</b> The central invite stores the token as
    /// <c>BaseModel.UUID</c> in plaintext, so anyone who can read the table can redeem any
    /// outstanding invitation. Here the row holds only <see cref="TokenHash"/>, and the token itself
    /// exists only in the emailed link.</item>
    /// <item><b>Recipient binding is ENFORCED.</b> The central type carries the note "any holder of
    /// the GET-query token can redeem the invite" and ships the equality helper
    /// <see cref="InviteRecipientMatch.RecipientEmailMatches"/> deliberately uncalled. The node's
    /// accept page calls it: the invitee must state the address the invitation was sent to, so a
    /// forwarded link is not a transferable account.</item>
    /// <item><b>It can be REVOKED.</b> The central type has no revocation, so an invitation sent to
    /// the wrong address can only be waited out. See <see cref="RevokedAt"/>.</item>
    /// </list>
    ///
    /// <para>
    /// Lifecycle, in the order the accept page checks it: revoked, then consumed, then expired, then
    /// active. Every non-active state is a friendly error and none of them reveals whether the
    /// address is registered.
    /// </para>
    /// </summary>
    public class NodeUserInvite : BaseModel
    {
        /// <summary>
        /// The address the invitation was issued to, and the address the invitee must state on the
        /// accept page. This is the recipient binding: without it, an invitation link is a bearer
        /// token for an account of the granted role, and forwarding the email is account transfer.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Invited email")]
        public string Email { get; set; }

        /// <summary>
        /// Lowercase hex SHA-256 of the invitation token, produced by
        /// <c>Febris.SharedServices.DeviceCredential.Hash</c>. The token itself is 256 bits from a
        /// CSPRNG and is NEVER stored, logged, or shown again after the email is sent.
        ///
        /// <para>
        /// The same reasoning that makes a fast unsalted SHA-256 correct for the device credential
        /// applies here and is worth restating rather than assuming: the input is high-entropy
        /// random rather than human-chosen, so there is nothing for a slow salted KDF to protect
        /// against, and the lookup has to be deterministic because redemption finds the row BY the
        /// token.
        /// </para>
        /// </summary>
        [Required]
        [Display(Name = "Token hash")]
        public string TokenHash { get; set; }

        /// <summary>
        /// Role granted on acceptance, held as the <c>InstitutionUserAccountType</c> NAME. The
        /// issuer may only grant a role they outrank, enforced by <c>RoleRankPolicy.CanAssign</c> at
        /// issue time -- the same gate <c>UserLogic.Create</c> applies. A name rather than an
        /// ordinal for the usual reason: an ordinal stops meaning what it meant the moment anyone
        /// reorders the enum, and here that would silently change which role an outstanding
        /// invitation grants.
        /// </summary>
        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        /// <summary>
        /// OPTIONAL cohort to add the accepted account to (2026-08-21). Null means no linkage, which
        /// is the default and the pre-existing behaviour.
        ///
        /// <para>
        /// Carried on the INVITATION rather than chosen by the invitee, because the person issuing
        /// it is the one who knows which class this is. It turns inviting a class from two steps
        /// (invite, then add everyone to the cohort afterwards) into one.
        /// </para>
        ///
        /// <para>
        /// A UUID rather than a foreign key, deliberately. Days can pass between issue and
        /// acceptance, and a cohort that is archived or deleted in the meantime must not make the
        /// invitation unredeemable -- the account still gets created, the linkage is skipped, and
        /// that is logged. A real FK would turn a tidy-up of the cohort table into a broken
        /// invitation.
        /// </para>
        /// </summary>
        [Display(Name = "Cohort")]
        public Guid? CohortUUID { get; set; }

        /// <summary>Given name, so the account can be created without asking the invitee to retype
        /// what the person inviting them already knows.</summary>
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        /// <summary>Family name. See <see cref="FirstName"/>.</summary>
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        /// <summary>Identity id of the issuing user, for audit and for the back-reference from a
        /// created account to the invitation that produced it.</summary>
        [Display(Name = "Issued by")]
        public Guid IssuedByUserId { get; set; }

        /// <summary>Email of the issuing user, denormalized so the admin list reads without a join
        /// and still reads after that account is deleted.</summary>
        [Display(Name = "Issued by")]
        public string IssuedByEmail { get; set; }

        /// <summary>Hard expiry (UTC). After this the invitation cannot be accepted even though it
        /// was never used.</summary>
        [Display(Name = "Expires (UTC)")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>Set when the invitation is accepted. Non-null means it cannot be used again.</summary>
        [Display(Name = "Accepted (UTC)")]
        public DateTime? ConsumedAt { get; set; }

        /// <summary>Identity id of the account created by accepting this invitation. Null while the
        /// invitation is outstanding.</summary>
        [Display(Name = "Accepted by")]
        public Guid? ConsumedByUserId { get; set; }

        /// <summary>
        /// Set when an administrator cancels the invitation. A revoked invitation is kept rather
        /// than deleted, so "we sent an invitation to the wrong address and cancelled it" stays
        /// answerable. Distinct from expiry, which happens on its own.
        /// </summary>
        [Display(Name = "Revoked (UTC)")]
        public DateTime? RevokedAt { get; set; }

        /// <summary>Email of the administrator who revoked it. Null while outstanding.</summary>
        [Display(Name = "Revoked by")]
        public string RevokedByEmail { get; set; }
    }
}
