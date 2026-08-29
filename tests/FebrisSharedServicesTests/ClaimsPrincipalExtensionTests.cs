// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.SharedServices;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="ClaimsPrincipalExtension"/> -- the
    /// role/claim helpers that gate every controller's Febris-staff
    /// check. Pins the audit A-09 fix (FebrisSales is now a member of
    /// IsFebrisUser) so a future regression is loud.
    /// </summary>
    public class IsFebrisUserTests
    {
        [Theory]
        [InlineData("FebrisDeveloper")]
        [InlineData("FebrisEngineer")]
        [InlineData("FebrisSales")]    // Added by audit A-09 -- regression guard.
        [InlineData("FebrisSupport")]
        [InlineData("SystemAdmin")]
        [InlineData("SuperAdmin")]
        public void IsFebrisUser_FebrisInternalRoles_ReturnsTrue(string role)
        {
            var principal = WithRole(role);
            principal.IsFebrisUser().Should().BeTrue(
                $"{role} is a Febris-internal role and must pass IsFebrisUser");
        }

        [Theory]
        [InlineData("User")]          // EndUserPortal local user
        [InlineData("Educator")]      // EndUserPortal local educator
        [InlineData("Admin")]         // Affiliate-org admin (NOT FebrisAdmin)
        [InlineData("ITAdmin")]       // Affiliate-org IT admin
        [InlineData("UserParent")]    // EndUserPortal parent
        public void IsFebrisUser_NonFebrisRoles_ReturnsFalse(string role)
        {
            var principal = WithRole(role);
            principal.IsFebrisUser().Should().BeFalse(
                $"{role} is not a Febris-internal role");
        }

        [Fact]
        public void IsFebrisUser_NoRoles_ReturnsFalse()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            principal.IsFebrisUser().Should().BeFalse("roleless principal has no Febris role");
        }

        private static ClaimsPrincipal WithRole(string role)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role)
            }, "Cookie");
            return new ClaimsPrincipal(identity);
        }
    }

    /// <summary>
    /// Tests for <see cref="ClaimsPrincipalExtension.IsFebrisAdmin"/>.
    /// Admin tier is strictly tighter than IsFebrisUser -- FebrisSales /
    /// FebrisSupport / FebrisDeveloper are Febris-internal but NOT admin.
    /// </summary>
    public class IsFebrisAdminTests
    {
        [Theory]
        [InlineData("FebrisEngineer")]
        [InlineData("SystemAdmin")]
        [InlineData("SuperAdmin")]
        public void IsFebrisAdmin_AdminRoles_ReturnsTrue(string role)
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }, "Cookie"));
            principal.IsFebrisAdmin().Should().BeTrue($"{role} is an admin-tier role");
        }

        [Theory]
        [InlineData("FebrisSales")]
        [InlineData("FebrisSupport")]
        [InlineData("FebrisDeveloper")]
        public void IsFebrisAdmin_NonAdminFebrisRoles_ReturnsFalse(string role)
        {
            // These are Febris-internal but NOT admin per the documented
            // tier order in DESIGN_NOTES.md "Role-gate intent" section.
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }, "Cookie"));
            principal.IsFebrisAdmin().Should().BeFalse(
                $"{role} is Febris-internal but not admin tier");
        }
    }

    /// <summary>
    /// Tests for <see cref="DeviceKeyClaimsPrincipalExtension"/> --
    /// the License/Hardware claim accessors introduced by audit A-02
    /// Stage 2. These pull TenantId / LicenseKey / LicenseRole /
    /// LicenseLocked from the ClaimsPrincipal that the JWT middleware
    /// builds after validating a license/hardware token.
    /// </summary>
    public class DeviceKeyClaimsPrincipalExtensionTests
    {
        [Fact]
        public void GetTenantId_ClaimMissing_ReturnsNull()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            principal.GetTenantId().Should().BeNull();
        }

        [Fact]
        public void GetTenantId_ClaimNotGuid_ReturnsNull()
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("TenantId", "not-a-guid") }, "License"));
            principal.GetTenantId().Should().BeNull(
                "unparseable claim value returns null, not throw");
        }

        [Fact]
        public void GetTenantId_ValidGuid_ReturnsParsed()
        {
            Guid tenant = Guid.NewGuid();
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("TenantId", tenant.ToString()) }, "License"));
            principal.GetTenantId().Should().Be(tenant);
        }

        [Fact]
        public void GetLicenseKey_ValidGuid_ReturnsParsed()
        {
            Guid key = Guid.NewGuid();
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("LicenseKey", key.ToString()) }, "License"));
            principal.GetLicenseKey().Should().Be(key);
        }

        [Fact]
        public void GetLicenseRole_ClaimSet_ReturnsValue()
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("LicenseRole", "Febris") }, "License"));
            principal.GetLicenseRole().Should().Be("Febris");
        }

        [Fact]
        public void GetLicenseRole_ClaimMissing_ReturnsNull()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            principal.GetLicenseRole().Should().BeNull();
        }

        [Fact]
        public void IsLicenseLockedFromClaim_True_ReturnsTrue()
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("LicenseLocked", "True") }, "License"));
            principal.IsLicenseLockedFromClaim().Should().BeTrue();
        }

        [Fact]
        public void IsLicenseLockedFromClaim_False_ReturnsFalse()
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("LicenseLocked", "False") }, "License"));
            principal.IsLicenseLockedFromClaim().Should().BeFalse();
        }

        [Fact]
        public void IsLicenseLockedFromClaim_ClaimMissing_ReturnsFalse()
        {
            // Fail-open on a Hardware-authed request (no License claims):
            // a missing LicenseLocked claim is treated as "not locked" so
            // hardware paths aren't accidentally rejected.
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            principal.IsLicenseLockedFromClaim().Should().BeFalse();
        }

        [Fact]
        public void IsLicenseLockedFromClaim_GarbageValue_ReturnsFalse()
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("LicenseLocked", "maybe") }, "License"));
            principal.IsLicenseLockedFromClaim().Should().BeFalse(
                "unparseable bool value treated as not-locked rather than throwing");
        }
    }

    /// <summary>
    /// Pins the Institution-claim fix. SupplementalClaimFactory (central SSO and
    /// EndUser) writes the user's organization as a claim named "Institution".
    /// HasInstiution/GetInstiution previously read the misspelled "Instiution",
    /// so they always returned false/null and org-scoping silently broke.
    /// </summary>
    public class InstitutionClaimTests
    {
        private static ClaimsPrincipal With(params Claim[] claims)
            => new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookie"));

        [Fact]
        public void GetInstiution_ReadsTheInstitutionClaimTheFactoryWrites()
        {
            string institution = Guid.NewGuid().ToString();
            var principal = With(new Claim("Institution", institution));
            principal.HasInstiution().Should().BeTrue("the factory writes the claim as 'Institution'");
            principal.GetInstiution().Should().Be(institution);
        }

        [Fact]
        public void HasInstiution_FalseWhenClaimMissing()
        {
            var principal = With(new Claim("FirstName", "Test"));
            principal.HasInstiution().Should().BeFalse();
            principal.GetInstiution().Should().BeNull();
        }

        [Fact]
        public void GetInstiution_DoesNotResolveTheOldMisspelledClaim()
        {
            var principal = With(new Claim("Instiution", "should-not-resolve"));
            principal.HasInstiution().Should().BeFalse();
            principal.GetInstiution().Should().BeNull();
        }
    }
}
