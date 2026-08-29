// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins <c>RoleRankPolicy.CanAssign</c> -- the role-ASSIGNMENT rank gate (audit C-05/C-06).
    ///
    /// <para>
    /// Rank was enforced for LOCKING and not for assignment, so an Educator -- the lowest staff
    /// role -- could promote ITSELF to ITAdmin through the shipped UI in two clicks, and could
    /// create Admin accounts outright. ITAdmin is the node's top local role, so that is escalation
    /// to the ceiling with no crafted request.
    /// </para>
    ///
    /// <para>
    /// The rule checks BOTH ends: the actor must strictly outrank the role being GRANTED (else
    /// self-promotion) and strictly outrank the target's CURRENT roles (else an Admin demotes an
    /// ITAdmin and takes the account over by lowering it first).
    /// </para>
    /// </summary>
    public class RoleRankPolicyCanAssignTests
    {
        private static readonly string[] Unranked = new string[0];

        // --- The reported defect ---

        [Fact]
        public void Educator_CannotPromoteItselfToITAdmin()
        {
            RoleRankPolicy.CanAssign(new[] { "Educator" }, new[] { "Educator" }, "ITAdmin")
                .Should().BeFalse("this was reachable through the shipped Edit UI in two clicks");
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("ITAdmin")]
        public void Educator_CannotCreateStaffAboveItself(string granted)
        {
            RoleRankPolicy.CanAssign(new[] { "Educator" }, Unranked, granted)
                .Should().BeFalse("the Create view rendered an unfiltered role list");
        }

        // --- Granting a role the actor does not strictly outrank ---

        [Theory]
        [InlineData("Educator", "Educator")]
        [InlineData("Admin", "Admin")]
        [InlineData("Admin", "ITAdmin")]
        public void CannotGrantARoleTheActorDoesNotStrictlyOutrank(string actor, string granted)
        {
            RoleRankPolicy.CanAssign(new[] { actor }, Unranked, granted)
                .Should().BeFalse("granting your own rank or higher is escalation, self included");
        }

        [Theory]
        [InlineData("Admin", "Educator")]
        [InlineData("ITAdmin", "Educator")]
        [InlineData("ITAdmin", "Admin")]
        public void CanGrantAStrictlyLowerRole(string actor, string granted)
        {
            RoleRankPolicy.CanAssign(new[] { actor }, Unranked, granted)
                .Should().BeTrue();
        }

        // --- The target end: no lateral takeover by demotion ---

        [Fact]
        public void Admin_CannotDemoteAnITAdmin()
        {
            RoleRankPolicy.CanAssign(new[] { "Admin" }, new[] { "ITAdmin" }, "Educator")
                .Should().BeFalse("lowering an account you do not outrank is how you take it over");
        }

        [Fact]
        public void PeersBelowTheCeilingCannotReRolePeers()
        {
            RoleRankPolicy.CanAssign(new[] { "Admin" }, new[] { "Admin" }, "User")
                .Should().BeFalse("strictly outrank, so peers and self are both refused");
            RoleRankPolicy.CanAssign(new[] { "Educator" }, new[] { "Educator" }, "User")
                .Should().BeFalse();
        }

        // --- The ceiling: the node's top local rank must stay administrable ---
        //
        // SuperAdmin was removed from NodeIdentityRoles.Required (owner ruling 2026-08-01) and the
        // bootstrap admin reseated as ITAdmin, so ITAdmin is the highest rank any node principal
        // can hold. A strictly-outrank rule with no carve-out therefore made ITAdmin ungrantable
        // and every ITAdmin account uneditable by anyone -- an administrative lockout, shipped in
        // e94e5c7 and caught on review. These pin the carve-out AND its limits.

        [Fact]
        public void TopRankPeer_CanGrantItsOwnRank_SoASecondItAdminCanBeProvisioned()
        {
            RoleRankPolicy.CanAssign(new[] { "ITAdmin" }, Unranked, "ITAdmin")
                .Should().BeTrue("otherwise a node can never provision a second IT admin in-app");
        }

        [Fact]
        public void TopRankPeer_CanReRoleAPeer_SoItAdminAccountsStayEditable()
        {
            RoleRankPolicy.CanAssign(new[] { "ITAdmin" }, new[] { "ITAdmin" }, "ITAdmin")
                .Should().BeTrue("saving an ITAdmin's profile with an unchanged role must work");
            RoleRankPolicy.CanAssign(new[] { "ITAdmin" }, new[] { "ITAdmin" }, "Admin")
                .Should().BeTrue("and a departed ITAdmin must be demotable by a peer");
        }

        [Fact]
        public void TheCeilingCarveOutDoesNotReopenEscalation()
        {
            // Only the top rank reaches the carve-out. Everyone below is still refused, which is
            // the whole point of C-05/C-06.
            RoleRankPolicy.CanAssign(new[] { "Educator" }, Unranked, "ITAdmin").Should().BeFalse();
            RoleRankPolicy.CanAssign(new[] { "Admin" }, Unranked, "ITAdmin").Should().BeFalse();
            RoleRankPolicy.CanAssign(new[] { "Educator" }, new[] { "ITAdmin" }, "Educator").Should().BeFalse();
            RoleRankPolicy.CanAssign(new[] { "Admin" }, new[] { "ITAdmin" }, "Educator").Should().BeFalse();
        }

        [Fact]
        public void TopRankPeer_StillCannotTouchARankAboveTheLocalCeiling()
        {
            // A legacy node whose bootstrap account predates the SuperAdmin removal.
            RoleRankPolicy.CanAssign(new[] { "ITAdmin" }, Unranked, "SuperAdmin")
                .Should().BeFalse("the carve-out is the LOCAL ceiling, not a blank cheque");
            RoleRankPolicy.CanAssign(new[] { "ITAdmin" }, new[] { "SuperAdmin" }, "Educator")
                .Should().BeFalse();
        }

        [Fact]
        public void LegacySuperAdmin_StillAdministersEveryoneBelowIt()
        {
            // Existing nodes still carry a SuperAdmin-roled bootstrap account (ROADMAP 13); it must
            // keep working under the general strict rule rather than falling into the carve-out.
            RoleRankPolicy.CanAssign(new[] { "SuperAdmin" }, Unranked, "ITAdmin").Should().BeTrue();
            RoleRankPolicy.CanAssign(new[] { "SuperAdmin" }, new[] { "ITAdmin" }, "Admin").Should().BeTrue();
        }

        // --- Unranked actors and unranked grants ---

        [Theory]
        [InlineData("User")]
        [InlineData("UserParent")]
        [InlineData("NotARole")]
        public void UnrankedActor_CanAssignNothing(string actor)
        {
            RoleRankPolicy.CanAssign(new[] { actor }, Unranked, "User").Should().BeFalse();
            RoleRankPolicy.CanAssign(new[] { actor }, Unranked, "Educator").Should().BeFalse();
        }

        [Fact]
        public void NullOrEmptyActorRoles_CanAssignNothing()
        {
            RoleRankPolicy.CanAssign(null, Unranked, "User").Should().BeFalse();
            RoleRankPolicy.CanAssign(Unranked, Unranked, "User").Should().BeFalse();
        }

        [Theory]
        [InlineData("Educator")]
        [InlineData("Admin")]
        [InlineData("ITAdmin")]
        public void AnyRankedActor_CanAssignAnUnrankedRole(string actor)
        {
            // User and UserParent confer no staff privilege, so an Educator provisioning learners
            // -- the ordinary case -- must keep working.
            RoleRankPolicy.CanAssign(new[] { actor }, Unranked, "User").Should().BeTrue();
            RoleRankPolicy.CanAssign(new[] { actor }, Unranked, "UserParent").Should().BeTrue();
        }

        // --- The vendor-era CanLock exception must NOT be inherited ---

        [Fact]
        public void Educator_CannotPromoteItselfToITAdmin_EvenAfterTheCeilingCarveOut()
        {
            // Guards the regression the carve-out could have caused: the reported defect must stay
            // fixed. Educator is nowhere near the ceiling, so it never reaches the carve-out.
            RoleRankPolicy.CanAssign(new[] { "Educator" }, new[] { "Educator" }, "ITAdmin")
                .Should().BeFalse();
            RoleRankPolicy.CanAssign(new[] { "Educator" }, new[] { "Educator" }, "Admin")
                .Should().BeFalse();
        }

        [Fact]
        public void ITAdmin_MayLockASuperAdmin_ButMayNotReRoleOne()
        {
            // CanLock carries "an ITAdmin may lock a SuperAdmin" so a tenant can revoke VENDOR
            // access to its own tenant -- a containment action. Changing a SuperAdmin's ROLE is a
            // different decision and the audit says explicitly not to copy the exception across.
            RoleRankPolicy.CanLock(new[] { "ITAdmin" }, new[] { "SuperAdmin" }).Should().BeTrue();
            RoleRankPolicy.CanAssign(new[] { "ITAdmin" }, new[] { "SuperAdmin" }, "Educator")
                .Should().BeFalse("an ITAdmin does not outrank a SuperAdmin for assignment");
            RoleRankPolicy.CanAssign(new[] { "ITAdmin" }, Unranked, "SuperAdmin")
                .Should().BeFalse("and may not grant one either");
        }
    }
}
