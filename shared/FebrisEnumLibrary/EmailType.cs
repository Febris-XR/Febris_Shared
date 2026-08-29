// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    public enum EmailType
    {

        Generic,
        CampaignMessage,


        FirstContact,
        Welcome,
        Question,
        CustomContentDevelopment,
        CompanyRegistration,
        ContentDeveloperApplication,
        AccreditationBodyApplication,
        PricingRequest,

        PendingItemNotification,
        UserUpdated,
        EmailVerification,
        EmailAddressChanged,
        ForgotPassword,
        PasswordReset,
        PasswordChanged,
        LinkVerification,
        StatementSubmission,
        DemoRequest,
        NewsletterSignup,
        NewsletterRemoval,
        Purchase,
        Unsubscribe,

        // Content-developer self-signup notifications. The verification
        // step itself uses EmailVerification (above); these two cover the
        // approval/rejection outcome from the AdminPortal queue.
        DeveloperApproved,
        DeveloperRejected,

        // Invite-to-existing-org email. Sent when an org admin issues a
        // ContentDeveloperUserInvite. SpecialHyperlink is the accept-link
        // (carries the token); Message field carries the inviter's name.
        DeveloperInvite,

        // Account-activation email. Sent when an admin creates an
        // ApplicationUser via the SSO API (CreateUser endpoints on the
        // ContentDeveloperUser / FebrisUser / AccreditationBodyUser
        // controllers). The new user clicks the SpecialHyperlink to
        // arrive at /Identity/Account/SetPassword where they pick their
        // own initial password -- replaces the previous flow of
        // generating a random password and emailing it.
        AccountActivation,

        // User-node invitation email. Sent when a node admin or educator issues
        // a NodeUserInvite from the portal's Invitations page.
        // SpecialHyperlink is the accept-link, which carries the token.
        //
        // Deliberately NOT reusing DeveloperInvite: that one says "join a Febris
        // developer team", which is wrong for a school node, and a node must
        // never send mail describing central concepts its recipients have no
        // relationship with.
        //
        // APPENDED LAST on purpose. Nothing persists this enum as an ordinal
        // today (it appears only on TestEmailViewModel and travels between hosts
        // as a NAME via IEmailSender), so inserting mid-list would have been
        // safe -- but appending removes the question entirely, and the cost of
        // being wrong about "nothing persists it" is silently sending the wrong
        // template.
        NodeUserInvite
    }
}
