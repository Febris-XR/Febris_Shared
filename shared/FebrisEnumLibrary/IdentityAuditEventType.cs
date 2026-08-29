// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
namespace Febris.EnumLibrary
{
    /// <summary>
    /// SSO Tier 6: the kind of identity event recorded in the IdentityAuditEvent trail.
    /// APPEND-ONLY -- never renumber or remove a value (existing rows persist the int); add new
    /// kinds at the end. Covers local authentication, MFA, credential changes, role grants,
    /// IdP / SCIM administration, account lifecycle, and federated logins (the unified trail that
    /// complements the federated-specific ExternalLoginAudit). See the SSO roadmap Tier 6.
    /// </summary>
    public enum IdentityAuditEventType
    {
        LocalLoginSucceeded = 1,
        LocalLoginFailed = 2,
        LocalLoginLockedOut = 3,
        PasswordChanged = 4,
        PasswordResetRequested = 5,
        PasswordResetCompleted = 6,
        MfaEnrolled = 7,
        MfaDisabled = 8,
        MfaChallengeSucceeded = 9,
        MfaChallengeFailed = 10,
        RoleGranted = 11,
        RoleRevoked = 12,
        IdpBindingCreated = 13,
        IdpBindingUpdated = 14,
        IdpBindingDeleted = 15,
        ScimTokenIssued = 16,
        ScimTokenRevoked = 17,
        UserCreated = 18,
        UserDeleted = 19,
        FederatedLoginSucceeded = 20,
        FederatedLoginFailed = 21,
    }
}
