// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins RoleRankPolicy.CanLock -- the EndUser lockout rank gate (audit B-07). Rank order:
    /// Educator &lt; Admin &lt; ITAdmin &lt; SuperAdmin; User/UserParent below Educator. An actor may lock
    /// a target only if it strictly outranks it; peers cannot lock peers (no self-lockout); single
    /// exception: ONLY an ITAdmin may lock a SuperAdmin (a peer SuperAdmin may not).
    /// </summary>
    public class RoleRankPolicyTests
    {
        // --- Actor strictly outranks target: allowed ---

        [Theory]
        [InlineData("Admin", "Educator")]
        [InlineData("ITAdmin", "Educator")]
        [InlineData("ITAdmin", "Admin")]
        [InlineData("SuperAdmin", "Educator")]
        [InlineData("SuperAdmin", "Admin")]
        [InlineData("SuperAdmin", "ITAdmin")]
        public void HigherRank_CanLock_LowerRank(string actor, string target)
        {
            RoleRankPolicy.CanLock(new[] { actor }, new[] { target })
                .Should().BeTrue($"{actor} strictly outranks {target}");
        }

        // --- Peers cannot lock peers (also blocks self-lockout), including SuperAdmin ---

        [Theory]
        [InlineData("Educator")]
        [InlineData("Admin")]
        [InlineData("ITAdmin")]
        [InlineData("SuperAdmin")]
        public void Peer_CannotLock_Peer(string role)
        {
            RoleRankPolicy.CanLock(new[] { role }, new[] { role })
                .Should().BeFalse($"a {role} cannot lock another {role} (peers, and self-lockout, are denied)");
        }

        // --- Lower rank cannot lock higher rank (the B-07 privilege escalation) ---

        [Theory]
        [InlineData("Educator", "Admin")]
        [InlineData("Educator", "ITAdmin")]
        [InlineData("Educator", "SuperAdmin")]
        [InlineData("Admin", "ITAdmin")]
        [InlineData("Admin", "SuperAdmin")]
        public void LowerRank_CannotLock_HigherRank(string actor, string target)
        {
            RoleRankPolicy.CanLock(new[] { actor }, new[] { target })
                .Should().BeFalse($"{actor} does not outrank {target} -- this is the B-07 escalation");
        }

        // --- The single exception: ITAdmin (and only ITAdmin) MAY lock SuperAdmin ---

        [Fact]
        public void ITAdmin_CanLock_SuperAdmin_ByException()
        {
            RoleRankPolicy.CanLock(new[] { "ITAdmin" }, new[] { "SuperAdmin" })
                .Should().BeTrue("a tenant ITAdmin may revoke the Febris SuperAdmin's access to their own tenant");
        }

        // --- Below-Educator and unknown actors can never lock ---

        [Theory]
        [InlineData("User", "Educator")]
        [InlineData("UserParent", "Educator")]
        [InlineData("User", "User")]
        [InlineData("UserParent", "Admin")]
        [InlineData("SomethingUnknown", "Educator")]
        public void BelowEducatorOrUnknownActor_CannotLock(string actor, string target)
        {
            RoleRankPolicy.CanLock(new[] { actor }, new[] { target })
                .Should().BeFalse($"{actor} ranks below Educator (or is unknown) and can lock nothing");
        }

        // --- Null / empty role sets ---

        [Fact]
        public void NullOrEmptyActorRoles_CannotLock()
        {
            RoleRankPolicy.CanLock(null, new[] { "Educator" }).Should().BeFalse("a null actor role set has no rank");
            RoleRankPolicy.CanLock(new string[0], new[] { "Educator" }).Should().BeFalse("an empty actor role set has no rank");
        }

        [Fact]
        public void NullOrEmptyTargetRoles_AreLockableByAnyRankedActor()
        {
            // A target with no recognized role ranks at NoRank; any ranked actor outranks it.
            RoleRankPolicy.CanLock(new[] { "Admin" }, null).Should().BeTrue();
            RoleRankPolicy.CanLock(new[] { "Admin" }, new string[0]).Should().BeTrue();
            RoleRankPolicy.CanLock(new[] { "Educator" }, new[] { "User" }).Should().BeTrue("User ranks below Educator");
        }

        // --- Mixed role sets use the highest rank on each side ---

        [Fact]
        public void MixedRoles_UseHighestRank_OnEachSide()
        {
            RoleRankPolicy.CanLock(new[] { "Educator", "ITAdmin" }, new[] { "User", "Admin" })
                .Should().BeTrue();
            RoleRankPolicy.CanLock(new[] { "User", "Admin" }, new[] { "Educator", "ITAdmin" })
                .Should().BeFalse();
        }

        // --- Case-insensitive role matching (GetRolesAsync casing must not matter) ---

        [Fact]
        public void RoleMatching_IsCaseInsensitive()
        {
            RoleRankPolicy.CanLock(new[] { "admin" }, new[] { "EDUCATOR" }).Should().BeTrue();
        }

        // --- RankOf direct checks ---

        [Fact]
        public void RankOf_OrdersRolesCorrectly()
        {
            RoleRankPolicy.RankOf(new[] { "Educator" }).Should().BeLessThan(RoleRankPolicy.RankOf(new[] { "Admin" }));
            RoleRankPolicy.RankOf(new[] { "Admin" }).Should().BeLessThan(RoleRankPolicy.RankOf(new[] { "ITAdmin" }));
            RoleRankPolicy.RankOf(new[] { "ITAdmin" }).Should().BeLessThan(RoleRankPolicy.RankOf(new[] { "SuperAdmin" }));
            RoleRankPolicy.RankOf(new[] { "User" }).Should().Be(RoleRankPolicy.RankOf(new[] { "UserParent" }));
            RoleRankPolicy.RankOf(null).Should().Be(0);
        }
    
        // --- User-list visibility rank comparison (UserLogic.Get) ---
        // The list filter hides accounts the viewer does not outrank, using RankOf directly.
        // It replaced a literal "is the target a SuperAdmin" check that silently stopped matching
        // when the node's bootstrap admin moved from SuperAdmin to ITAdmin, exposing the node's
        // sole administrator in the Educator-visible user index.

        [Theory]
        [InlineData("Educator", "ITAdmin")]   // the regression: bootstrap admin must stay hidden
        [InlineData("Educator", "Admin")]
        [InlineData("Educator", "SuperAdmin")]
        [InlineData("Admin", "ITAdmin")]
        [InlineData("Admin", "SuperAdmin")]
        [InlineData("ITAdmin", "SuperAdmin")]
        [InlineData("User", "Educator")]
        public void HigherRankedAccountsAreHiddenFromLowerRankedViewers(string viewer, string target)
        {
            RoleRankPolicy.RankOf(new[] { target })
                .Should().BeGreaterThan(RoleRankPolicy.RankOf(new[] { viewer }),
                    "the list filter drops the row when target rank exceeds viewer rank");
        }

        [Theory]
        [InlineData("Admin", "Admin")]        // peers stay visible: an Admin still manages Admins
        [InlineData("ITAdmin", "ITAdmin")]
        [InlineData("ITAdmin", "Admin")]
        [InlineData("Admin", "Educator")]
        [InlineData("SuperAdmin", "ITAdmin")]
        public void PeersAndLowerRankedAccountsStayVisible(string viewer, string target)
        {
            RoleRankPolicy.RankOf(new[] { target })
                .Should().BeLessThanOrEqualTo(RoleRankPolicy.RankOf(new[] { viewer }),
                    "the filter drops only accounts ABOVE the viewer, never peers");
        }
}
}
