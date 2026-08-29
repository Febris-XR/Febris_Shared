// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;

namespace Febris.SharedServices
{
    /// <summary>
    /// Pure rank policy for the EndUser lockout gate (audit B-07). Decides whether an actor (the
    /// signed-in operator) may lock out a target account from their role ranks alone, so the rule is
    /// unit-testable and identical wherever it is enforced. Lives in FebrisSharedServices with an
    /// EnumLibrary-only dependency, so it stays inside the EndUser island with no central/SSO coupling.
    ///
    /// Rank order (low to high): Educator &lt; Admin &lt; ITAdmin &lt; SuperAdmin. User and UserParent
    /// rank below Educator. Rule: an actor may lock a target ONLY if the actor STRICTLY outranks the
    /// target. Peers cannot lock peers (this also blocks self-lockout). Single EXCEPTION: an ITAdmin
    /// may lock a SuperAdmin -- the SuperAdmin is a Febris account, so a tenant ITAdmin can revoke
    /// Febris's access to their own tenant. A SuperAdmin may NOT lock a peer SuperAdmin.
    /// </summary>
    public static class RoleRankPolicy
    {
        // Higher = more privileged. Roles below Educator (User, UserParent) and any unrecognized role
        // rank at NoRank and can never lock anything.
        private const int NoRank = 0;
        private const int EducatorRank = 1;
        private const int AdminRank = 2;
        private const int ITAdminRank = 3;
        private const int SuperAdminRank = 4;

        /// <summary>
        /// The highest rank a NODE principal can actually hold. SuperAdmin was removed from
        /// <c>NodeIdentityRoles.Required</c> (owner ruling 2026-08-01) because it is a VENDOR staff
        /// role and support is not offered, and the bootstrap admin was reseated as ITAdmin. So on
        /// a node the ceiling is ITAdmin, and <see cref="CanAssign"/> needs to know that: a
        /// strictly-outrank rule with no ceiling carve-out makes the top role unassignable and
        /// every account holding it uneditable. Legacy nodes whose bootstrap account is still
        /// SuperAdmin rank ABOVE this and keep working under the general rule.
        /// </summary>
        private const int TopLocalRank = ITAdminRank;

        private static readonly Dictionary<string, int> Ranks = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { InstitutionUserAccountType.Educator.ToString(), EducatorRank },
            { InstitutionUserAccountType.Admin.ToString(), AdminRank },
            { InstitutionUserAccountType.ITAdmin.ToString(), ITAdminRank },
            { FebrisUserType.SuperAdmin.ToString(), SuperAdminRank },
        };

        /// <summary>
        /// Highest rank among the given role names. Roles below Educator (User, UserParent) and
        /// unknown roles contribute NoRank. A null/empty set is NoRank.
        /// </summary>
        public static int RankOf(IEnumerable<string> roles)
        {
            if (roles == null)
            {
                return NoRank;
            }
            int max = NoRank;
            foreach (string r in roles)
            {
                if (r != null && Ranks.TryGetValue(r, out int rank) && rank > max)
                {
                    max = rank;
                }
            }
            return max;
        }

        /// <summary>
        /// True when an actor with <paramref name="actorRoles"/> may lock a target with
        /// <paramref name="targetRoles"/>. Actor must strictly outrank target. Single exception: an
        /// ITAdmin (exactly) may lock a SuperAdmin. An actor with no rank can never lock; peers and
        /// self-lockout are denied (including SuperAdmin on SuperAdmin).
        /// </summary>
        public static bool CanLock(IEnumerable<string> actorRoles, IEnumerable<string> targetRoles)
        {
            int actor = RankOf(actorRoles);
            int target = RankOf(targetRoles);

            if (actor <= NoRank)
            {
                return false;
            }

            // Exception: ONLY an ITAdmin may lock a SuperAdmin (a tenant ITAdmin revoking the Febris
            // SuperAdmin's access). A peer SuperAdmin may not -- peers cannot lock peers.
            if (target == SuperAdminRank)
            {
                return actor == ITAdminRank;
            }

            return actor > target;
        }

        /// <summary>
        /// True when an actor with <paramref name="actorRoles"/> may give <paramref name="roleToAssign"/>
        /// to a target currently holding <paramref name="targetRoles"/> (audit C-05/C-06).
        ///
        /// <para>
        /// BOTH ends are checked, because either alone leaves a door open. The actor must strictly
        /// outrank the role being GRANTED -- otherwise an Educator promotes itself to ITAdmin, which
        /// was reachable through the shipped UI in two clicks. The actor must ALSO strictly outrank
        /// the target's CURRENT roles -- otherwise an Admin demotes an ITAdmin to Educator and takes
        /// the account over by lowering it first. Strictly, so peers cannot re-role peers and no one
        /// can re-role themselves.
        /// </para>
        ///
        /// <para>
        /// <see cref="CanLock"/>'s "an ITAdmin may lock a SuperAdmin" exception is deliberately NOT
        /// carried over. That exception exists so a tenant ITAdmin can revoke the VENDOR's access to
        /// their own tenant, which is a containment action; changing a SuperAdmin's ROLE is a
        /// different decision, and SuperAdmin has in any case been dropped from the node's seeded
        /// roles. Assigning an unranked role (User, UserParent) is allowed for any ranked actor who
        /// outranks the target, since it confers no staff privilege.
        /// </para>
        /// </summary>
        public static bool CanAssign(IEnumerable<string> actorRoles, IEnumerable<string> targetRoles, string roleToAssign)
        {
            int actor = RankOf(actorRoles);

            if (actor <= NoRank)
            {
                return false;
            }

            int granted = RankOf(new[] { roleToAssign });
            int target = RankOf(targetRoles);

            // CEILING RULE. At the node's top local rank, peers MUST be able to administer each
            // other. Without this the strict rule eats itself once SuperAdmin leaves the seeded
            // roles: ITAdmin becomes ungrantable (nobody outranks it), so a node can never provision
            // a second IT admin through the app, and every ITAdmin account -- including the
            // bootstrap one -- becomes uneditable by anyone, forever. That is an administrative
            // lockout, which is the same failure mode as the self-lockout the "Febris User" toggle
            // shipped. A top-rank actor may therefore grant its own rank and re-role a peer, but
            // still may not touch a rank ABOVE the local ceiling (a legacy vendor SuperAdmin).
            //
            // This does NOT reopen the escalation this policy exists to stop: an Educator or Admin
            // never reaches the carve-out, so only an ITAdmin can mint an ITAdmin.
            if (actor == TopLocalRank)
            {
                return granted <= TopLocalRank && target <= TopLocalRank;
            }

            // Granting a staff role the actor does not strictly outrank is escalation, self
            // included.
            if (granted >= actor)
            {
                return false;
            }

            // Re-roling an account the actor does not strictly outrank is lateral takeover.
            return actor > target;
        }
    }
}
