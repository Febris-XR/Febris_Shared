// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Febris.SharedServices
{
    // AffiliateType enum moved to Febris.EnumLibrary per the "all enums live in FebrisEnumLibrary" rule.

    /// <summary>
    /// Audit A-05 deferred fix (2026-05-20): class-level gate for SSO controllers
    /// whose actions branch on caller TYPE (Febris-internal staff vs. an affiliate
    /// org admin). The class-level gate ensures a new action that forgets the
    /// per-action <c>User.IsFebrisUser() || User.IsContentDeveloper()</c> check
    /// still fails closed for callers outside the union.
    /// <para>
    /// Allows if: caller is in any Febris-internal role (FebrisDeveloper,
    /// FebrisEngineer, FebrisSales, FebrisSupport, SystemAdmin, SuperAdmin)
    /// OR the specified affiliate (e.g., ContentDeveloper -- meaning the caller
    /// has the ContentDeveloper claim AND a User/Admin/ITAdmin role within
    /// that org).
    /// </para>
    /// <para>
    /// Per-action code still branches on which type the caller is, to decide
    /// what content to return. This attribute does not replace those branches --
    /// it just stops an unauthenticated or unrelated-tenant caller from reaching
    /// the action body at all.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FebrisOrAffiliateAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly AffiliateType _affiliate;

        public FebrisOrAffiliateAuthorizeAttribute(AffiliateType affiliate)
        {
            _affiliate = affiliate;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Febris-internal staff always allowed.
            if (user.IsFebrisUser())
            {
                return;
            }

            // Affiliate admins allowed for their respective endpoints.
            bool affiliateOk = _affiliate switch
            {
                AffiliateType.ContentDeveloper => user.IsContentDeveloper(),
                AffiliateType.AccreditationBody => user.IsAccreditationBody(),
                _ => false,
            };

            if (affiliateOk)
            {
                return;
            }

            context.Result = new ForbidResult();
        }
    }
}
