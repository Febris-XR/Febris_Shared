// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// Invite for a 2nd-or-later user to join an existing
    /// <see cref="ContentDeveloper"/> org via the SSO Register-with-invite
    /// page. Issued by an authenticated org admin (or by a Febris user on
    /// the admin's behalf) and consumed exactly once when the invitee
    /// completes signup. <see cref="BaseModel.UUID"/> is the invite token
    /// itself -- 128 bits of CSPRNG-quality randomness courtesy of
    /// <c>Guid.NewGuid()</c>, unguessable in practice.
    ///
    /// Lifecycle:
    /// <list type="bullet">
    ///   <item><c>ConsumedAt == null AND ExpiresAt &gt; now</c> -- active</item>
    ///   <item><c>ConsumedAt != null</c> -- accepted; cannot be reused</item>
    ///   <item><c>ExpiresAt &lt;= now</c> -- expired; cannot be accepted</item>
    /// </list>
    /// </summary>
    public class ContentDeveloperUserInvite : BaseModel
    {
        /// <summary>
        /// UUID of the <see cref="ContentDeveloper"/> the invitee will be
        /// linked to upon acceptance.
        /// </summary>
        [Required]
        public Guid ContentDeveloperUUID { get; set; }

        /// <summary>
        /// Email address the invite was issued to. The intended control is
        /// that the Accept page checks the invitee provided the same address,
        /// preventing token forwarding to a different recipient. That
        /// enforcement is NOT wired up yet (see the FIX note below), so today
        /// any holder of the GET-query token can redeem the invite.
        /// </summary>
        // FIX (DEV-B10): The recipient-binding equality check is now available as the pure,
        // unit-tested helper RecipientEmailMatches below (InvoiceMath-style lift so it is testable
        // without the DB-backed BLL). It is intentionally NOT yet called by ConsumeAsync: wiring it
        // in is the STRUCTURAL part of the fix (initiative DEV-M9) because it adds an Email field to
        // ContentDeveloperUserInviteAcceptViewModel and the OrdinalIgnoreCase comparison inside
        // ContentDeveloperInviteLogic.ConsumeAsync (both other files) and changes an authorization
        // decision (who may redeem the token), and it should POST the token instead of GET. Until
        // that lands the gate stays exactly as deferred.
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Role to grant on acceptance. Org admins can only issue invites
        /// for roles &lt;= their own (enforced by the BLL). Default is
        /// <see cref="UserAccountType.User"/> for new team members.
        /// </summary>
        public UserAccountType Role { get; set; } = UserAccountType.User;

        /// <summary>
        /// ApplicationUser.Id (Identity Guid) of the user who issued the
        /// invite. Used for audit + to surface "who invited you" in the
        /// accept page.
        /// </summary>
        public Guid IssuedByUserId { get; set; }

        /// <summary>
        /// Hard expiry. After this timestamp the invite cannot be consumed
        /// even if still un-redeemed. Default policy in the BLL: 7 days
        /// from issue.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Set when the invite is accepted by the invitee. Null means
        /// active (subject to expiry check). Once non-null the invite
        /// cannot be reused.
        /// </summary>
        public DateTime? ConsumedAt { get; set; }

        /// <summary>
        /// ApplicationUser.Id of the user who consumed the invite. Null
        /// while the invite is active. Provides a back-reference from
        /// invite to created account for audit / cleanup.
        /// </summary>
        public Guid? ConsumedByUserId { get; set; }
    }

    /// <summary>
    /// Pure recipient-binding helper for <see cref="ContentDeveloperUserInvite"/>, lifted out so
    /// the DEV-B10 email-match control can be unit-tested without standing up the DB-backed
    /// ContentDeveloperInviteLogic (whose ConsumeAsync news its own queries and reads the user
    /// manager, leaving no injection seam until the DI refactor lands). This is the same
    /// "extract the pure logic" pattern as InvoiceMath. It is intentionally not yet called by the
    /// consume path: wiring it in is the deferred structural work (DEV-M9), so this changes no
    /// authorization decision today. Public (not internal like InvoiceMath) only because
    /// Febris.ModelLibrary does not expose InternalsVisibleTo to the test assembly and adding it
    /// would touch the csproj, which is outside this single-file fix.
    /// </summary>
    public static class InviteRecipientMatch
    {
        // FIX (DEV-B10): true when the address the invitee supplied on the Accept page is the same
        // address the invite was issued to, compared case-insensitively and trimmed (email local
        // and domain parts are treated case-insensitively here, matching the canonical-username and
        // claim-dictionary OrdinalIgnoreCase convention used elsewhere in the SSO BLL). A null or
        // whitespace-only supplied address never matches, so a missing form field fails closed.
        public static bool RecipientEmailMatches(string invitedEmail, string suppliedEmail)
        {
            if (string.IsNullOrWhiteSpace(invitedEmail) || string.IsNullOrWhiteSpace(suppliedEmail))
            {
                return false;
            }

            return string.Equals(invitedEmail.Trim(), suppliedEmail.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
