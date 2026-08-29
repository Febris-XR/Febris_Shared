// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>One selectable cohort on the invitation form. A flat pair rather than the Cohort
    /// entity, so the model library's page models stay free of data-model shapes.</summary>
    public class InvitationCohortOption
    {
        /// <summary>The cohort's UUID, which is what the invitation stores.</summary>
        public Guid Uuid { get; set; }

        /// <summary>Display name.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Form input for issuing a node account invitation.
    /// <para>
    /// Named for the node deliberately: <c>InviteIssueOutcome</c> and friends already exist in the
    /// central developer-org flow, and the duplicate-type ratchet counts simple names across
    /// projects.
    /// </para>
    /// </summary>
    public class InvitationIssueInputModel
    {
        /// <summary>Address to invite. Also the recipient binding -- the invitee must state this
        /// same address to redeem, so a forwarded link is not a transferable account.</summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        /// <summary>Given name, so the invitee is not asked to retype what the inviter knows.</summary>
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        /// <summary>Family name.</summary>
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        /// <summary>Role to grant on acceptance, as an <c>InstitutionUserAccountType</c> NAME. The
        /// issuer may only pick a role they outrank.</summary>
        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        /// <summary>Lifetime in days. Null uses the default; out-of-range values are clamped rather
        /// than rejected.</summary>
        [Display(Name = "Expires in (days)")]
        public int? ExpiresInDays { get; set; }

        /// <summary>OPTIONAL cohort to add the accepted account to. Null means no linkage, which is
        /// the default.</summary>
        [Display(Name = "Add to cohort (optional)")]
        public Guid? CohortUUID { get; set; }
    }

    /// <summary>
    /// Outcome of issuing an invitation. Carries the RAW TOKEN, which exists nowhere else: the
    /// stored row holds only its hash, and this object is the one chance to put it in a link.
    /// <para>
    /// Nothing may log or persist <see cref="RawToken"/>. It is handed to the caller so the accept
    /// URL can be built, and it is shown to the ISSUING admin once so a node with no working SMTP
    /// still has a usable invitation flow.
    /// </para>
    /// </summary>
    public class NodeInviteIssueResult
    {
        /// <summary>Whether the invitation was created.</summary>
        public bool Success { get; set; }

        /// <summary>Why not, for display. Null on success.</summary>
        public string Error { get; set; }

        /// <summary>The persisted row. Null on failure.</summary>
        public NodeUserInvite Invite { get; set; }

        /// <summary>The raw token, for building the accept link. Null on failure.</summary>
        public string RawToken { get; set; }

        /// <summary>A failure with a displayable reason.</summary>
        public static NodeInviteIssueResult Failed(string error)
        {
            return new NodeInviteIssueResult() { Success = false, Error = error };
        }

        /// <summary>A success carrying the row and the one-time token.</summary>
        public static NodeInviteIssueResult Succeeded(NodeUserInvite invite, string rawToken)
        {
            return new NodeInviteIssueResult() { Success = true, Invite = invite, RawToken = rawToken };
        }
    }

    /// <summary>
    /// Classification of a token presented to the accept page. <see cref="Invite"/> is populated
    /// ONLY when <see cref="State"/> is <see cref="InviteState.Active"/>, so a dead token yields no
    /// email address, no role and no name -- a probe cannot learn who an expired invitation was for.
    /// </summary>
    public class NodeInviteValidation
    {
        /// <summary>What the token is.</summary>
        public InviteState State { get; set; }

        /// <summary>The invitation, on the active path only.</summary>
        public NodeUserInvite Invite { get; set; }

        /// <summary>Message for the invitee. Says what to do next and never reveals whether the
        /// address has an account.</summary>
        public string Message
        {
            get
            {
                switch (State)
                {
                    case InviteState.Active:
                        return null;
                    case InviteState.Expired:
                        return "This invitation has expired. Ask whoever invited you to send a new one.";
                    case InviteState.AlreadyConsumed:
                        return "This invitation has already been used. If that was you, sign in instead.";
                    case InviteState.Revoked:
                        return "This invitation was cancelled. Ask whoever invited you if you think that is a mistake.";
                    default:
                        // NotFound and anything the shared enum grows later. Says nothing about
                        // whether a link ever existed.
                        return "This invitation link is not valid.";
                }
            }
        }
    }

    /// <summary>One row of the admin invitation list, flattened with its state already decided so
    /// the view holds no lifecycle logic.</summary>
    public class InvitationRowViewModel
    {
        /// <summary>Admin-side handle, used by the revoke action.</summary>
        public Guid Uuid { get; set; }

        /// <summary>Who it was sent to.</summary>
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>Name on the invitation, blank when the inviter did not supply one.</summary>
        [Display(Name = "Name")]
        public string Name { get; set; }

        /// <summary>Role granted on acceptance.</summary>
        [Display(Name = "Role")]
        public string Role { get; set; }

        /// <summary>Name of the cohort the accepted account joins, or null when none was chosen.
        /// Resolved for display; a cohort deleted since issue reads as null rather than blowing up
        /// the list.</summary>
        [Display(Name = "Cohort")]
        public string CohortName { get; set; }

        /// <summary>Who issued it.</summary>
        [Display(Name = "Invited by")]
        public string IssuedByEmail { get; set; }

        /// <summary>When it was issued (UTC).</summary>
        [Display(Name = "Sent (UTC)")]
        public DateTime IssuedAtUtc { get; set; }

        /// <summary>When it stops working (UTC).</summary>
        [Display(Name = "Expires (UTC)")]
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>Current lifecycle state.</summary>
        public InviteState State { get; set; }

        /// <summary>Whether the revoke button applies.</summary>
        public bool CanRevoke
        {
            get { return State == InviteState.Active; }
        }

        /// <summary>When it was accepted or cancelled, for the history column. Null while active.</summary>
        [Display(Name = "Closed (UTC)")]
        public DateTime? ClosedAtUtc { get; set; }

        /// <summary>Who cancelled it, when that is how it closed.</summary>
        [Display(Name = "Cancelled by")]
        public string RevokedByEmail { get; set; }
    }

    /// <summary>Page model for the admin invitation list.</summary>
    public class InvitationsPageViewModel
    {
        /// <summary>Every invitation, newest first.</summary>
        public List<InvitationRowViewModel> Invitations { get; set; } = new List<InvitationRowViewModel>();

        /// <summary>Cohorts offered by the issue form, newest first. Empty when the node has none,
        /// in which case the picker is hidden rather than shown empty.</summary>
        public List<InvitationCohortOption> AvailableCohorts { get; set; } = new List<InvitationCohortOption>();

        /// <summary>Roles the signed-in operator is permitted to grant. Computed with the same rank
        /// policy the issue path enforces, so the form cannot offer a choice the POST would refuse.</summary>
        public List<string> AssignableRoles { get; set; } = new List<string>();

        /// <summary>The effective registration mode, shown for context. Invitations work in every
        /// mode, and the page says so, because that is the opposite of what most people assume.</summary>
        [Display(Name = "Registration mode")]
        public string EffectiveRegistrationMode { get; set; }

        /// <summary>
        /// Accept link for an invitation just issued, rendered ONCE and never stored.
        ///
        /// <para>
        /// Shown deliberately, and it is the one place this flow puts a live token in a browser.
        /// <c>RegisterConfirmation</c> hard-disables its equivalent, but that page shows a token to
        /// whoever just used a public form; this shows it to the authenticated operator who minted
        /// it seconds earlier and is entitled to hand it over by any channel. Without it, a node
        /// whose SMTP is unconfigured -- the normal state of a fresh self-hosted deployment -- has
        /// an invitation feature that silently does nothing.
        /// </para>
        /// </summary>
        public string IssuedAcceptUrl { get; set; }

        /// <summary>Address the one-time link above belongs to, so the operator knows who to send it to.</summary>
        public string IssuedForEmail { get; set; }

        /// <summary>True when the invitation was created but the email could not be sent, so the
        /// page can say to use the link instead of assuming it arrived.</summary>
        public bool IssuedEmailFailed { get; set; }
    }
}
