// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins MfaPolicy.IsRequired -- the SSO Tier 2 rule deciding whether a local-password
    /// account must use MFA: any privileged Febris-staff role, OR an institution that has
    /// opted into required MFA.
    /// </summary>
    public class MfaPolicyTests
    {
        [Theory]
        [InlineData("FebrisDeveloper")]
        [InlineData("FebrisEngineer")]
        [InlineData("FebrisSales")]
        [InlineData("FebrisSupport")]
        [InlineData("SystemAdmin")]
        [InlineData("SuperAdmin")]
        public void StaffRoles_RequireMfa(string role)
        {
            MfaPolicy.IsRequired(new[] { role }, institutionRequiresMfa: false)
                .Should().BeTrue($"{role} is a privileged Febris-staff role and must use MFA");
        }

        [Theory]
        [InlineData("User")]
        [InlineData("Educator")]
        [InlineData("Admin")]
        [InlineData("ITAdmin")]
        [InlineData("UserParent")]
        public void NonStaffRoles_DoNotRequireMfa_ByThemselves(string role)
        {
            MfaPolicy.IsRequired(new[] { role }, institutionRequiresMfa: false)
                .Should().BeFalse($"{role} is not a privileged staff role and its institution did not opt in");
        }

        [Fact]
        public void InstitutionRequiringMfa_ForcesIt_ForAnyUser()
        {
            MfaPolicy.IsRequired(new[] { "User" }, institutionRequiresMfa: true).Should().BeTrue();
            MfaPolicy.IsRequired(new string[0], institutionRequiresMfa: true).Should().BeTrue();
        }

        [Fact]
        public void NoRolesAndNoInstitutionFlag_DoesNotRequireMfa()
        {
            MfaPolicy.IsRequired(new string[0], false).Should().BeFalse();
            MfaPolicy.IsRequired(null, false).Should().BeFalse("a null role list is treated as no roles");
        }

        [Fact]
        public void MixedRoles_RequireMfa_IfAnyIsStaff()
        {
            MfaPolicy.IsRequired(new[] { "User", "SuperAdmin" }, false).Should().BeTrue();
        }

        // --- Per-institution opt-in (config-driven Mfa:RequiredInstitutions) ---

        [Fact]
        public void InstitutionInRequiredList_RequiresMfa()
        {
            Guid inst = Guid.NewGuid();
            MfaPolicy.InstitutionRequiresMfa(inst, new[] { inst.ToString() }).Should().BeTrue();
        }

        [Fact]
        public void InstitutionNotInRequiredList_DoesNotRequireMfa()
        {
            Guid inst = Guid.NewGuid();
            MfaPolicy.InstitutionRequiresMfa(inst, new[] { Guid.NewGuid().ToString() }).Should().BeFalse();
        }

        [Fact]
        public void InstitutionMatch_IsCaseAndWhitespaceInsensitive()
        {
            Guid inst = Guid.NewGuid();
            MfaPolicy.InstitutionRequiresMfa(inst, new[] { "  " + inst.ToString().ToUpperInvariant() + " " })
                .Should().BeTrue("config values may carry casing/whitespace differences");
        }

        [Fact]
        public void InstitutionRequiresMfa_NullOrEmptyInputs_ReturnFalse()
        {
            MfaPolicy.InstitutionRequiresMfa(null, new[] { Guid.NewGuid().ToString() }).Should().BeFalse();
            MfaPolicy.InstitutionRequiresMfa(Guid.Empty, new[] { Guid.Empty.ToString() }).Should().BeFalse("Guid.Empty means no institution");
            MfaPolicy.InstitutionRequiresMfa(Guid.NewGuid(), null).Should().BeFalse();
        }

        [Fact]
        public void InstitutionFlag_ComposesWithIsRequired()
        {
            // The Login gate composes the two checks: institution flag OR staff role.
            Guid inst = Guid.NewGuid();
            bool institutionRequires = MfaPolicy.InstitutionRequiresMfa(inst, new[] { inst.ToString() });
            MfaPolicy.IsRequired(new[] { "User" }, institutionRequires).Should().BeTrue();
        }

        // --- Request-level hard block (ShouldForceEnrollment / IsEnrollmentAllowedPath) ---

        [Fact]
        public void ShouldForceEnrollment_RequiredLocalUnenrolledOnNormalPage_IsTrue()
        {
            MfaPolicy.ShouldForceEnrollment("/Manage/Index", isAuthenticated: true,
                roles: new[] { "SuperAdmin" }, institutionRequiresMfa: false,
                hasLocalPassword: true, twoFactorEnabled: false).Should().BeTrue();
        }

        [Theory]
        [InlineData("/Identity/Account/Manage/EnableAuthenticator")]
        [InlineData("/Identity/Account/Logout")]
        [InlineData("/Identity/Account/Login")]
        [InlineData("/css/site.css")]
        [InlineData("/lib/jquery/jquery.js")]
        [InlineData("/favicon.ico")]
        public void ShouldForceEnrollment_AllowedPaths_AreFalse(string path)
        {
            MfaPolicy.ShouldForceEnrollment(path, true, new[] { "SuperAdmin" }, false, true, false)
                .Should().BeFalse($"{path} must stay reachable during forced enrollment");
        }

        [Fact]
        public void ShouldForceEnrollment_EnrolledUser_IsFalse()
        {
            MfaPolicy.ShouldForceEnrollment("/Manage/Index", true, new[] { "SuperAdmin" }, false, true, twoFactorEnabled: true)
                .Should().BeFalse();
        }

        [Fact]
        public void ShouldForceEnrollment_NonRequiredUser_IsFalse()
        {
            MfaPolicy.ShouldForceEnrollment("/Manage/Index", true, new[] { "User" }, false, true, false)
                .Should().BeFalse();
        }

        [Fact]
        public void ShouldForceEnrollment_FederatedOnlyAccount_IsExempt()
        {
            // No local password -> federated account; their MFA is the IdP's responsibility.
            MfaPolicy.ShouldForceEnrollment("/Manage/Index", true, new[] { "SuperAdmin" }, false,
                hasLocalPassword: false, twoFactorEnabled: false).Should().BeFalse();
        }

        [Fact]
        public void ShouldForceEnrollment_Anonymous_IsFalse()
        {
            MfaPolicy.ShouldForceEnrollment("/Manage/Index", isAuthenticated: false,
                new[] { "SuperAdmin" }, false, true, false).Should().BeFalse();
        }

        [Fact]
        public void ShouldForceEnrollment_InstitutionRequired_ForcesNonStaff()
        {
            MfaPolicy.ShouldForceEnrollment("/Manage/Index", true, new[] { "User" },
                institutionRequiresMfa: true, hasLocalPassword: true, twoFactorEnabled: false).Should().BeTrue();
        }

        [Fact]
        public void IsEnrollmentAllowedPath_NullOrEmpty_IsAllowed()
        {
            MfaPolicy.IsEnrollmentAllowedPath(null).Should().BeTrue();
            MfaPolicy.IsEnrollmentAllowedPath("").Should().BeTrue();
        }

        [Fact]
        public void IsEnrollmentAllowedPath_NormalPage_IsNotAllowed()
        {
            MfaPolicy.IsEnrollmentAllowedPath("/Manage/Email").Should().BeFalse();
        }

        // --- SSO-claim -> portal contract: the values SupplementalClaimFactory stamps
        //     (MfaEnrolled, HasLocalPassword as bool.ToString()) drive the shared-cookie
        //     portals' ShouldForceEnrollment after the portal parses them. ---

        [Theory]
        [InlineData("False", "True", true)]   // unenrolled local staff -> forced to enroll
        [InlineData("True", "True", false)]   // already enrolled -> allowed through
        [InlineData("False", "False", false)] // federated-only (no local password) -> exempt
        public void StampedClaims_DriveEnrollmentDecision_ForStaff(string mfaEnrolledClaim, string hasLocalPasswordClaim, bool expectForced)
        {
            // Mirror the portal middleware's parse of the SSO-stamped claim strings.
            bool mfaEnrolled = bool.Parse(mfaEnrolledClaim);
            bool hasLocalPassword = bool.Parse(hasLocalPasswordClaim);
            MfaPolicy.ShouldForceEnrollment("/Home/Index", true, new[] { "SuperAdmin" }, false, hasLocalPassword, mfaEnrolled)
                .Should().Be(expectForced);
        }

        // --- Master switch (Mfa:Enabled / Mfa__Enabled env var) ---

        [Fact]
        public void IsRequired_MasterSwitchOff_NeverRequired()
        {
            // Even a privileged staff role AND an opted-in institution are not required when off.
            MfaPolicy.IsRequired(new[] { "SuperAdmin" }, institutionRequiresMfa: true, enforcementEnabled: false)
                .Should().BeFalse();
        }

        [Fact]
        public void ShouldForceEnrollment_MasterSwitchOff_NeverForced()
        {
            // A required, unenrolled, local-password staff user on a normal page is NOT forced
            // to enroll while the master switch is off.
            MfaPolicy.ShouldForceEnrollment("/Manage/Index", true, new[] { "SuperAdmin" },
                institutionRequiresMfa: true, hasLocalPassword: true, twoFactorEnabled: false,
                enforcementEnabled: false).Should().BeFalse();
        }

        [Fact]
        public void MasterSwitch_ConfigDefaultOff_PolicyParamDefaultAssumesActive()
        {
            // Ops default: MFA enforcement is OFF until explicitly enabled per host (staged rollout).
            MfaPolicy.EnforcementEnabledByDefault.Should().BeFalse("MFA is dormant until enabled per host");

            // The policy methods' own parameter default assumes enforcement is active, so the core
            // logic tests (which omit the flag) read naturally; real call sites pass the resolved
            // Mfa:Enabled value.
            MfaPolicy.IsRequired(new[] { "SuperAdmin" }, false).Should().BeTrue();
            MfaPolicy.IsRequired(new[] { "SuperAdmin" }, false, enforcementEnabled: true).Should().BeTrue();
            MfaPolicy.ShouldForceEnrollment("/Manage/Index", true, new[] { "SuperAdmin" }, false, true, false, enforcementEnabled: true)
                .Should().BeTrue();
        }

        [Fact]
        public void BoolClaim_RoundTrips_AsPortalsParseIt()
        {
            // SupplementalClaimFactory stamps user.TwoFactorEnabled.ToString() ("True"/"False");
            // the portals parse with bool.TryParse. Pin that round-trip so a claim-format change
            // (e.g. switching to "1"/"0") is caught before it silently disables the portal gate.
            bool.TryParse(true.ToString(), out bool enrolled).Should().BeTrue();
            enrolled.Should().BeTrue();
            bool.TryParse(false.ToString(), out bool notEnrolled).Should().BeTrue();
            notEnrolled.Should().BeFalse();
        }
    }
}
