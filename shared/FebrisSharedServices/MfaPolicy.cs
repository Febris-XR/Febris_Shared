// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Febris.SharedServices
{
    /// <summary>
    /// SSO Tier 2 policy: decides whether a local-password account MUST use
    /// multi-factor authentication, and whether a required-but-unenrolled user must be
    /// held on the enrollment flow. A pure, dependency-free class so it is trivially
    /// unit-testable AND shareable across the central SSO and every shared-cookie portal
    /// (AdminPortal, DeveloperPortal) -- they all consume the same SSO-issued cookie, so
    /// they must apply the SAME enrollment rule. Lives in FebrisSharedServices for exactly
    /// that reason (referenced by the SSO API and both portals).
    ///
    /// Two triggers (decision B, 2026-06-22):
    ///   - the account holds a privileged Febris-staff role, or
    ///   - the account's institution has opted into required MFA.
    /// Federated (OIDC/SAML) logins are exempt -- the customer IdP handles their MFA; they
    /// are detected by the absence of a local password.
    /// </summary>
    public static class MfaPolicy
    {
        /// <summary>
        /// Default for the <c>Mfa:Enabled</c> master switch when the setting is absent: OFF.
        /// MFA enforcement is dormant until it is explicitly enabled per host (staged rollout),
        /// via the <c>Mfa__Enabled</c> environment variable (or <c>Mfa:Enabled</c> in appsettings),
        /// set consistently on every enforcing host (SSO + Admin + Developer portals). When off,
        /// MFA is never required anywhere -- the login gate, the SSO middleware, and the portal
        /// middlewares all pass this through. NOTE: this is the OPS default for the config setting;
        /// the policy methods below keep a parameter default of <c>true</c> (they assume enforcement
        /// is active when a caller omits the flag), so the real call sites must pass the resolved
        /// <c>Mfa:Enabled</c> value.
        /// </summary>
        // static readonly (not const) so the value is read from this assembly at runtime rather
        // than inlined into each consuming host at their compile time -- the SSO + both portals
        // must all observe the same default even if built/deployed at different times.
        public static readonly bool EnforcementEnabledByDefault = false;

        // The privileged internal roles. Mirrors ClaimsPrincipalExtension.IsFebrisUser()
        // so "Febris staff" means the same set everywhere.
        private static readonly HashSet<string> StaffRolesRequiringMfa = new HashSet<string>
        {
            FebrisUserType.FebrisDeveloper.ToString(),
            FebrisUserType.FebrisEngineer.ToString(),
            FebrisUserType.FebrisSales.ToString(),
            FebrisUserType.FebrisSupport.ToString(),
            FebrisUserType.SystemAdmin.ToString(),
            FebrisUserType.SuperAdmin.ToString(),
        };

        /// <summary>
        /// True when MFA is mandatory for an account with the given roles and institution flag.
        /// When the master switch <paramref name="enforcementEnabled"/> is false (Mfa:Enabled),
        /// nothing is ever required. Otherwise institution opt-in forces it for everyone in the
        /// org, and the privileged Febris-staff roles are always forced.
        /// </summary>
        public static bool IsRequired(IEnumerable<string> roles, bool institutionRequiresMfa, bool enforcementEnabled = true)
        {
            if (!enforcementEnabled)
            {
                return false; // master switch off (Mfa:Enabled) -> MFA is never required
            }
            if (institutionRequiresMfa)
            {
                return true;
            }
            if (roles == null)
            {
                return false;
            }
            return roles.Any(r => StaffRolesRequiringMfa.Contains(r));
        }

        /// <summary>
        /// True when the user's institution has opted into required MFA, i.e. its UUID
        /// appears in the configured set (the <c>Mfa:RequiredInstitutions</c> appsettings
        /// list). Config-driven on purpose so the per-institution opt-in needs NO schema
        /// change and no migration; a DB-backed org-admin runtime toggle
        /// (InstitutionSettings.RequireMfa) is the deferred enhancement to batch with a
        /// future migration.
        /// </summary>
        public static bool InstitutionRequiresMfa(Guid? institution, IEnumerable<string> requiredInstitutionUuids)
        {
            if (institution == null || institution == Guid.Empty || requiredInstitutionUuids == null)
            {
                return false;
            }
            string target = institution.Value.ToString();
            return requiredInstitutionUuids.Any(u => string.Equals(u?.Trim(), target, StringComparison.OrdinalIgnoreCase));
        }

        // Paths a required-but-unenrolled user is still allowed to reach: the authenticator
        // enrollment flow, recovery-code display, login/2fa, logout, and static assets. Any
        // other path is blocked until they enroll. Loop-safe: the enrollment + logout pages
        // are on this list so redirecting to them never re-triggers the redirect.
        private static readonly string[] EnrollmentAllowedPrefixes =
        {
            "/Identity/Account/Manage/EnableAuthenticator",
            "/Identity/Account/Manage/ShowRecoveryCodes",
            "/Identity/Account/Logout",
            "/Identity/Account/Login",
            "/Identity/Account/LoginWith2fa",
            "/Identity/Account/LoginWithRecoveryCode",
            "/Identity/Account/AccessDenied",
        };

        private static readonly string[] StaticPrefixes =
        {
            "/css", "/js", "/lib", "/images", "/img", "/fonts", "/favicon", "/_framework", "/_content", "/.well-known",
        };

        /// <summary>
        /// True when a request path is always allowed during forced enrollment (the
        /// enrollment/login/logout pages and static assets). Anything else is blocked for a
        /// required-but-unenrolled user. See <see cref="ShouldForceEnrollment"/>.
        /// </summary>
        public static bool IsEnrollmentAllowedPath(string requestPath)
        {
            if (string.IsNullOrEmpty(requestPath))
            {
                return true; // be permissive on odd/empty paths rather than risk a redirect loop
            }
            foreach (string p in EnrollmentAllowedPrefixes)
            {
                if (requestPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return true;
            }
            foreach (string p in StaticPrefixes)
            {
                if (requestPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return true;
            }
            // Treat any path whose last segment has a file extension as a static asset.
            int lastSlash = requestPath.LastIndexOf('/');
            string lastSegment = lastSlash >= 0 ? requestPath.Substring(lastSlash + 1) : requestPath;
            return lastSegment.Contains(".");
        }

        /// <summary>
        /// The request-level hard-block decision (SSO Tier 2 follow-up). True when an
        /// authenticated local-password user who is REQUIRED to use MFA has not enrolled and
        /// is requesting a non-allowed path -- they must be redirected to authenticator setup.
        /// Exemptions: anonymous requests, federated-only accounts (no local password -- their
        /// MFA is the IdP's job), already-enrolled users, non-required users, and the
        /// enrollment/login/logout/static allow-list. Pure so it is unit-testable; callers
        /// (the SSO middleware via a DB lookup, the portal middleware via cookie claims) supply
        /// the inputs.
        /// </summary>
        public static bool ShouldForceEnrollment(
            string requestPath,
            bool isAuthenticated,
            IEnumerable<string> roles,
            bool institutionRequiresMfa,
            bool hasLocalPassword,
            bool twoFactorEnabled,
            bool enforcementEnabled = true)
        {
            if (!enforcementEnabled) return false;        // master switch off (Mfa:Enabled)
            if (!isAuthenticated) return false;
            if (!hasLocalPassword) return false;          // federated-only account -> IdP handles MFA
            if (twoFactorEnabled) return false;           // already enrolled
            if (!IsRequired(roles, institutionRequiresMfa, enforcementEnabled)) return false;
            if (IsEnrollmentAllowedPath(requestPath)) return false;
            return true;
        }
    }
}
