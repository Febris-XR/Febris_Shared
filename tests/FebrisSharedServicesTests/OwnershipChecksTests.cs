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
    /// Tests for <see cref="OwnershipChecks"/> -- the shared BLL
    /// ownership-gate helpers introduced for audit A-10 / A-11 (CRM +
    /// AdminPortal IDOR rollout). All methods are pure extensions on
    /// <see cref="ClaimsPrincipal"/> so no DI gymnastics required.
    /// </summary>
    public class OwnershipChecksTests
    {
        // ----- IsCurrentUser -----

        [Fact]
        public void IsCurrentUser_PrincipalNull_ReturnsFalse()
        {
            ClaimsPrincipal nullPrincipal = null;
            bool result = nullPrincipal.IsCurrentUser(Guid.NewGuid());
            result.Should().BeFalse("null principal can't match anything");
        }

        [Fact]
        public void IsCurrentUser_EmptyOwnerList_ReturnsFalse()
        {
            var principal = PrincipalWithUserId(Guid.NewGuid());
            bool result = principal.IsCurrentUser(new Guid?[0]);
            result.Should().BeFalse("empty owner-id list has nothing to match");
        }

        [Fact]
        public void IsCurrentUser_NameIdentifierMissing_ReturnsFalse()
        {
            // Principal with claims but no NameIdentifier.
            var identity = new ClaimsIdentity(new[] { new Claim("Foo", "bar") }, "Cookie");
            var principal = new ClaimsPrincipal(identity);
            bool result = principal.IsCurrentUser(Guid.NewGuid());
            result.Should().BeFalse("no NameIdentifier claim means we can't extract current user");
        }

        [Fact]
        public void IsCurrentUser_NameIdentifierNotGuid_ReturnsFalse()
        {
            // Some flows put usernames in NameIdentifier; only Guid values match.
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "alice@example.com") }, "Cookie");
            var principal = new ClaimsPrincipal(identity);
            bool result = principal.IsCurrentUser(Guid.NewGuid());
            result.Should().BeFalse("non-Guid NameIdentifier can't match a Guid owner");
        }

        [Fact]
        public void IsCurrentUser_MatchesFirstOwner_ReturnsTrue()
        {
            Guid userId = Guid.NewGuid();
            var principal = PrincipalWithUserId(userId);
            bool result = principal.IsCurrentUser(userId, Guid.NewGuid());
            result.Should().BeTrue("user matches first owner-id position");
        }

        [Fact]
        public void IsCurrentUser_MatchesSecondOwner_ReturnsTrue()
        {
            // Real use case: (AssignedToUserId, CreatedByUserId) -- caller
            // might be either.
            Guid userId = Guid.NewGuid();
            var principal = PrincipalWithUserId(userId);
            bool result = principal.IsCurrentUser(Guid.NewGuid(), userId);
            result.Should().BeTrue("user matches any owner-id position, not just first");
        }

        [Fact]
        public void IsCurrentUser_NullOwnerInArray_DoesNotMatch()
        {
            // Mixed null + real owner ids -- match logic must skip nulls.
            Guid userId = Guid.NewGuid();
            var principal = PrincipalWithUserId(userId);
            bool resultMatchSkippingNull = principal.IsCurrentUser(null, userId);
            resultMatchSkippingNull.Should().BeTrue("nulls in owner array must not prevent later real-match");

            bool resultOnlyNulls = principal.IsCurrentUser(new Guid?[] { null, null });
            resultOnlyNulls.Should().BeFalse("all-null array is the same as empty");
        }

        // ----- EnsureAdmin -----

        [Fact]
        public void EnsureAdmin_FebrisAdminRole_DoesNotThrow()
        {
            // IsFebrisAdmin = FebrisEngineer / SystemAdmin / SuperAdmin.
            var principal = PrincipalWithRoles(FebrisUserType.FebrisEngineer.ToString());
            Action act = () => principal.EnsureAdmin("test");
            act.Should().NotThrow();
        }

        [Fact]
        public void EnsureAdmin_NonAdminFebrisRole_Throws()
        {
            // FebrisSales is a Febris-internal role but NOT in IsFebrisAdmin.
            var principal = PrincipalWithRoles(FebrisUserType.FebrisSales.ToString());
            Action act = () => principal.EnsureAdmin("test");
            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*Febris admin role required*");
        }

        [Fact]
        public void EnsureAdmin_NullPrincipal_Throws()
        {
            ClaimsPrincipal nullPrincipal = null;
            Action act = () => nullPrincipal.EnsureAdmin("test");
            act.Should().Throw<UnauthorizedAccessException>();
        }

        // ----- EnsureFebrisUser -----

        [Fact]
        public void EnsureFebrisUser_FebrisSales_DoesNotThrow()
        {
            // FebrisSales was added to IsFebrisUser in audit A-09.
            var principal = PrincipalWithRoles(FebrisUserType.FebrisSales.ToString());
            Action act = () => principal.EnsureFebrisUser("test");
            act.Should().NotThrow("FebrisSales counts as Febris-internal per audit A-09 fix");
        }

        [Fact]
        public void EnsureFebrisUser_NonFebrisRole_Throws()
        {
            // EndUserPortal roles (e.g., Educator) are not Febris-internal.
            var principal = PrincipalWithRoles("Educator");
            Action act = () => principal.EnsureFebrisUser("test");
            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*Febris-internal staff role required*");
        }

        // ----- EnsureOwnerOrAdmin -----

        [Fact]
        public void EnsureOwnerOrAdmin_AdminWithNoMatchingOwners_DoesNotThrow()
        {
            // Admin bypass: even if caller is not in owner list, admin role passes.
            var principal = PrincipalWithRoles(FebrisUserType.SuperAdmin.ToString());
            Action act = () => principal.EnsureOwnerOrAdmin("delete", Guid.NewGuid(), Guid.NewGuid());
            act.Should().NotThrow("admins bypass per-resource ownership");
        }

        [Fact]
        public void EnsureOwnerOrAdmin_OwnerNonAdmin_DoesNotThrow()
        {
            Guid userId = Guid.NewGuid();
            // Non-admin Febris role; user is in owner list.
            var principal = PrincipalWithRolesAndUserId(userId, FebrisUserType.FebrisSales.ToString());
            Action act = () => principal.EnsureOwnerOrAdmin("complete", userId);
            act.Should().NotThrow("user-is-owner path admits non-admins");
        }

        [Fact]
        public void EnsureOwnerOrAdmin_NonOwnerNonAdmin_Throws()
        {
            // Worst case: non-admin user, not in owner list. The IDOR-blocking path.
            var principal = PrincipalWithRolesAndUserId(Guid.NewGuid(), FebrisUserType.FebrisSales.ToString());
            Action act = () => principal.EnsureOwnerOrAdmin("complete", Guid.NewGuid(), Guid.NewGuid());
            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*Only the owner or a Febris admin*");
        }

        [Fact]
        public void EnsureOwnerOrAdmin_NullPrincipal_Throws()
        {
            ClaimsPrincipal nullPrincipal = null;
            Action act = () => nullPrincipal.EnsureOwnerOrAdmin("delete", Guid.NewGuid());
            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*no signed-in user*");
        }

        // ----- helpers -----

        private static ClaimsPrincipal PrincipalWithUserId(Guid userId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "Cookie");
            return new ClaimsPrincipal(identity);
        }

        private static ClaimsPrincipal PrincipalWithRoles(params string[] roles)
        {
            List<Claim> claims = new List<Claim>();
            foreach (string r in roles) { claims.Add(new Claim(ClaimTypes.Role, r)); }
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookie"));
        }

        private static ClaimsPrincipal PrincipalWithRolesAndUserId(Guid userId, params string[] roles)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            foreach (string r in roles) { claims.Add(new Claim(ClaimTypes.Role, r)); }
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookie"));
        }
    }
}
