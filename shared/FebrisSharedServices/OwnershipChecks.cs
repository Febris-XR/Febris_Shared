// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Linq;
using System.Security.Claims;

namespace Febris.SharedServices
{
    /// <summary>
    /// Audit A-10 / A-11 helper (2026-05-20): uniform ownership-check
    /// building blocks for BLL methods that mutate a resource. Designed
    /// to be composed per-resource policy.
    /// <para>
    /// Two enforcement patterns:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="EnsureAdmin(ClaimsPrincipal,string)"/> -- admin-only mutations (Invoice, Module, ContentDeveloper, etc).</description></item>
    ///   <item><description><see cref="EnsureOwnerOrAdmin(ClaimsPrincipal,string,Guid?[])"/> -- assignee/creator/owner mutations (Tasks, Notes, Correspondence).</description></item>
    /// </list>
    /// <para>
    /// Throws <see cref="UnauthorizedAccessException"/> on failure; BLL
    /// callers let the exception propagate, and the controller layer
    /// catches it and returns <c>Forbid()</c>. Pattern established by
    /// <c>LeadTaskLogic.EnsureAuthorizedToMutate</c>; this is the
    /// generalized shared form for the A-11 rollout.
    /// </para>
    /// <para>
    /// "Admin" here means a Febris-internal admin
    /// (<see cref="ClaimsPrincipalExtension.IsFebrisAdmin"/>:
    /// <c>FebrisEngineer + SystemAdmin + SuperAdmin</c>). Resources owned
    /// by external orgs (e.g. ContentDeveloperUser scoped to a specific
    /// ContentDeveloper) need a custom check on top of these helpers --
    /// see <see cref="IsCurrentUser(ClaimsPrincipal,Guid?[])"/> for the
    /// owner-only primitive, then compose.
    /// </para>
    /// </summary>
    public static class OwnershipChecks
    {
        /// <summary>
        /// True if the caller's <see cref="ClaimTypes.NameIdentifier"/>
        /// claim parses as a Guid and matches any of the supplied owner
        /// ids. Empty owner-id arg array returns false.
        /// </summary>
        public static bool IsCurrentUser(this ClaimsPrincipal principal, params Guid?[] ownerIds)
        {
            if (principal == null || ownerIds == null || ownerIds.Length == 0) return false;
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return false;
            if (!Guid.TryParse(idClaim.Value, out Guid currentUserId)) return false;
            return ownerIds.Any(o => o.HasValue && o.Value == currentUserId);
        }

        /// <summary>
        /// Throw <see cref="UnauthorizedAccessException"/> unless the
        /// caller is a Febris admin
        /// (<see cref="ClaimsPrincipalExtension.IsFebrisAdmin"/>).
        /// Use for resources where no per-user ownership exists -- the
        /// only legitimate mutator is internal staff with admin tier
        /// (e.g., Invoice, ContentDeveloper record, reference data).
        /// </summary>
        /// <param name="principal">Calling ClaimsPrincipal.</param>
        /// <param name="verb">Human verb for the error message ("edit", "delete", "approve", ...).</param>
        public static void EnsureAdmin(this ClaimsPrincipal principal, string verb)
        {
            if (principal != null && principal.IsFebrisAdmin()) return;
            throw new UnauthorizedAccessException(
                $"Not authorized to {verb} this resource. Febris admin role required " +
                $"(FebrisEngineer, SystemAdmin, or SuperAdmin).");
        }

        /// <summary>
        /// Throw <see cref="UnauthorizedAccessException"/> unless the
        /// caller is any Febris-internal staff
        /// (<see cref="ClaimsPrincipalExtension.IsFebrisUser"/>:
        /// FebrisSales, FebrisSupport, FebrisDeveloper, FebrisEngineer,
        /// SystemAdmin, SuperAdmin).
        /// Use for resources that any Febris staffer may mutate but
        /// non-Febris callers (ContentDeveloper, AccreditationBody,
        /// EndUser) must not (e.g., Module deletes -- ContentDevs can
        /// edit their own modules via the Update path but only Febris
        /// staff can delete).
        /// </summary>
        public static void EnsureFebrisUser(this ClaimsPrincipal principal, string verb)
        {
            if (principal != null && principal.IsFebrisUser()) return;
            throw new UnauthorizedAccessException(
                $"Not authorized to {verb} this resource. Febris-internal staff role required.");
        }

        /// <summary>
        /// Throw <see cref="UnauthorizedAccessException"/> unless the
        /// caller is either (a) one of the resource's owners (matches any
        /// supplied owner Guid against the caller's NameIdentifier) or
        /// (b) a Febris admin. Use for resources with per-user ownership
        /// (LeadTask, LeadNote, LeadCorrespondence).
        /// </summary>
        /// <param name="principal">Calling ClaimsPrincipal.</param>
        /// <param name="verb">Human verb for the error message.</param>
        /// <param name="ownerIds">All Guids on the resource that legitimately
        /// represent "owner" (e.g., AssignedToUserId, CreatedByUserId).
        /// Any match permits the action.</param>
        public static void EnsureOwnerOrAdmin(this ClaimsPrincipal principal, string verb, params Guid?[] ownerIds)
        {
            if (principal == null)
            {
                throw new UnauthorizedAccessException(
                    $"Cannot {verb} resource: no signed-in user.");
            }
            if (principal.IsFebrisAdmin()) return;
            if (principal.IsCurrentUser(ownerIds)) return;
            throw new UnauthorizedAccessException(
                $"Not authorized to {verb} this resource. Only the owner or a Febris admin may do so.");
        }

        /// <summary>
        /// Audit A-11 deferred-half helper (2026-05-20): throws
        /// <see cref="UnauthorizedAccessException"/> unless the caller is
        /// either (a) a Febris-internal staff member (any role in
        /// <see cref="ClaimsPrincipalExtension.IsFebrisUser"/>) or
        /// (b) a ContentDeveloper org-admin whose own
        /// <c>ContentDeveloper</c> claim matches the resource's
        /// <paramref name="resourceContentDeveloperUuid"/>.
        /// <para>
        /// This is the BLL-side analog to the controller-level
        /// <c>[FebrisOrAffiliateAuthorize(AffiliateType.ContentDeveloper)]</c>
        /// attribute introduced for audit A-05 deferred-half. Use for
        /// shared BLLs whose mutating methods accept ContentDeveloper-
        /// scoped resources (e.g. ContentDeveloperLinkedModule,
        /// ContentDeveloperLinkedCurriculum, ContentDeveloperLinkedDiscount,
        /// ContentDeveloperLinkedMarketplaceListing): a Febris staffer can
        /// touch any org's data, but a ContentDeveloper user can only
        /// touch their own org's.
        /// </para>
        /// </summary>
        /// <param name="principal">Calling ClaimsPrincipal.</param>
        /// <param name="verb">Human verb for the error message.</param>
        /// <param name="resourceContentDeveloperUuid">The
        /// <c>ContentDeveloperUUID</c> on the resource being mutated.
        /// Null means "resource has no org yet" (e.g., create-time before
        /// the org link is set); in that case only Febris staff pass.</param>
        public static void EnsureFebrisOrContentDeveloperOrg(this ClaimsPrincipal principal, string verb, Guid? resourceContentDeveloperUuid)
        {
            if (principal == null)
            {
                throw new UnauthorizedAccessException($"Cannot {verb}: no signed-in user.");
            }
            if (principal.IsFebrisUser()) return;
            if (principal.IsContentDeveloper())
            {
                string claimValue = principal.ContentDeveloper();
                if (!string.IsNullOrWhiteSpace(claimValue)
                    && Guid.TryParse(claimValue, out Guid callerOrg)
                    && resourceContentDeveloperUuid.HasValue
                    && callerOrg == resourceContentDeveloperUuid.Value)
                {
                    return;
                }
                throw new UnauthorizedAccessException(
                    $"Not authorized to {verb}: this resource belongs to a different ContentDeveloper org.");
            }
            throw new UnauthorizedAccessException(
                $"Not authorized to {verb}: must be Febris-internal staff or a ContentDeveloper admin.");
        }

        /// <summary>
        /// Audit A-11 deferred-half helper (2026-05-20): mirror of
        /// <see cref="EnsureFebrisOrContentDeveloperOrg"/> for resources
        /// scoped to an <c>AccreditationBody</c> org.
        /// </summary>
        public static void EnsureFebrisOrAccreditationBodyOrg(this ClaimsPrincipal principal, string verb, Guid? resourceAccreditationBodyUuid)
        {
            if (principal == null)
            {
                throw new UnauthorizedAccessException($"Cannot {verb}: no signed-in user.");
            }
            if (principal.IsFebrisUser()) return;
            if (principal.IsAccreditationBody())
            {
                string claimValue = principal.AccreditationBody();
                if (!string.IsNullOrWhiteSpace(claimValue)
                    && Guid.TryParse(claimValue, out Guid callerOrg)
                    && resourceAccreditationBodyUuid.HasValue
                    && callerOrg == resourceAccreditationBodyUuid.Value)
                {
                    return;
                }
                throw new UnauthorizedAccessException(
                    $"Not authorized to {verb}: this resource belongs to a different AccreditationBody org.");
            }
            throw new UnauthorizedAccessException(
                $"Not authorized to {verb}: must be Febris-internal staff or an AccreditationBody admin.");
        }
    }
}
