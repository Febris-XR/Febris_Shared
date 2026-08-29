// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    public static class ClaimsPrincipalExtension
    {
        //Generic User information
        public static string FirstName(this ClaimsPrincipal principal)
        {
            var item = principal.Claims.FirstOrDefault(c => c.Type == "FirstName");
            return item?.Value;
        }
        public static string LastName(this ClaimsPrincipal principal)
        {
            var item = principal.Claims.FirstOrDefault(c => c.Type == "LastName");
            return item?.Value;
        }
        public static bool HasProfilePicture(this ClaimsPrincipal principal)
        {
            bool output = false;
            var item = principal.Claims.FirstOrDefault(c => c.Type == "ProfilePicturePath");
            if (!string.IsNullOrEmpty(item?.Value))
            {
                output = true;
            }

            //var item = principal.Claims.FirstOrDefault(c => c.Type == "HasProfilePicture");//.Value;

            //output = Boolean.Parse(item?.Value);

            //return item?.Value;
            return output;
        }
        public static string ProfilePicturePath(this ClaimsPrincipal principal)
        {
            var item = principal.Claims.FirstOrDefault(c => c.Type == "ProfilePicturePath");
            return item?.Value;
        }
        public static bool HasLiabilityWaiver(this ClaimsPrincipal principal)
        {
            bool output = false;
            var item = principal.Claims.FirstOrDefault(c => c.Type == "LiabilityWaiver");
            if (item?.Value != Guid.Empty.ToString() && !string.IsNullOrEmpty(item?.Value))
            {
                output = true;
            }
            //return item?.Value;
            return output;
        }
        public static string GetLiabilityWaiver(this ClaimsPrincipal principal)
        {
            var item = principal.Claims.FirstOrDefault(c => c.Type == "LiabilityWaiver");
            return item?.Value;
        }
        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Email);
        }
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Name);
        }
        public static bool HasSignedLiabilityWaiver(this ClaimsPrincipal principal)
        {
            bool output = false;
            try
            {
                var item = principal.Claims.FirstOrDefault(c => c.Type == "LiabilityWaiver");
                if (item != null
                    && !string.IsNullOrEmpty(item.Value)
                    && Guid.Parse(item.Value) != Guid.Empty
                    //&& (
                    //    principal.IsInRole(UserAccountType.ITAdmin.ToString())
                    //    || principal.IsInRole(UserAccountType.Admin.ToString())
                    //    || principal.IsInRole(UserAccountType.User.ToString())
                    //)
                    )
                {
                    output = true;
                }
            }
            // intentional: best-effort claim probe; malformed/absent LiabilityWaiver claim degrades to false
            catch { Febris.SharedServices.FebrisLog.Warn("ClaimsPrincipalExtension.HasSignedLiabilityWaiver: claim parse failed, treating as unsigned"); }
            return output;
        }

        public static bool HasSignedServiceAgreement(this ClaimsPrincipal principal)
        {
            bool output = false;
            try
            {
                var item = principal.Claims.FirstOrDefault(c => c.Type == "ServiceAgreement");
                if (item != null
                    && !string.IsNullOrEmpty(item.Value)
                    && Guid.Parse(item.Value) != Guid.Empty
                    //&& (
                    //    principal.IsInRole(UserAccountType.ITAdmin.ToString())
                    //    || principal.IsInRole(UserAccountType.Admin.ToString())
                    //    || principal.IsInRole(UserAccountType.User.ToString())
                    //)
                    )
                {
                    output = true;
                }
            }
            // intentional: best-effort claim probe; malformed/absent ServiceAgreement claim degrades to false
            catch { Febris.SharedServices.FebrisLog.Warn("ClaimsPrincipalExtension.HasSignedServiceAgreement: claim parse failed, treating as unsigned"); }
            return output;
        }
        public static bool HasSignedEULA(this ClaimsPrincipal principal)
        {
            bool output = false;
            try
            {
                var item = principal.Claims.FirstOrDefault(c => c.Type == "EULA");
                if (item != null
                    && !string.IsNullOrEmpty(item.Value)
                    && Guid.Parse(item.Value) != Guid.Empty
                    //&& (
                    //    principal.IsInRole(UserAccountType.ITAdmin.ToString())
                    //    || principal.IsInRole(UserAccountType.Admin.ToString())
                    //    || principal.IsInRole(UserAccountType.User.ToString())
                    //)
                    )
                {
                    output = true;
                }
            }
            // intentional: best-effort claim probe; malformed/absent EULA claim degrades to false
            catch { Febris.SharedServices.FebrisLog.Warn("ClaimsPrincipalExtension.HasSignedEULA: claim parse failed, treating as unsigned"); }
            return output;
        }
        //Professional across different platforms
        //public static bool HasProfessional(this ClaimsPrincipal principal)
        //{
        //    bool output = false;
        //    var item = principal.Claims.FirstOrDefault(c => c.Type == "Professional");
        //    if (item?.Value != Guid.Empty.ToString() && !string.IsNullOrEmpty(item?.Value))
        //    {
        //        output = true;
        //    }
        //    //return item?.Value;
        //    return output;
        //}
        //public static string GetProfessional(this ClaimsPrincipal principal)
        //{
        //    var item = principal.Claims.FirstOrDefault(c => c.Type == "Professional");
        //    return item?.Value;
        //}
        public static bool HasActor(this ClaimsPrincipal principal)
        {
            bool output = false;
            var item = principal.Claims.FirstOrDefault(c => c.Type == "Actor");
            if (item?.Value != Guid.Empty.ToString() && !string.IsNullOrEmpty(item?.Value))
            {
                output = true;
            }
            //return item?.Value;
            return output;
        }
        public static string GetActor(this ClaimsPrincipal principal)
        {
            var item = principal.Claims.FirstOrDefault(c => c.Type == "Actor");
            return item?.Value;
        }
        public static bool IsCurrentUser(this ClaimsPrincipal principal, string id)
        {
            var currentUserId = GetUserId(principal);

            return string.Equals(currentUserId, id, StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsFebrisUser(this ClaimsPrincipal principal)
        {
            // Audit A-09 fix (2026-05-20): added FebrisSales. The role IS a
            // first-class Febris-internal role (granted in AdminPortal alongside
            // the other 5), but the original IsFebrisUser() check omitted it,
            // causing FebrisSales users to authenticate successfully then be
            // Forbidden from every SSO API endpoint that calls IsFebrisUser().
            // Sensitive operations remain restricted by the sibling IsFebrisAdmin()
            // / IsSysAdmin() checks (which intentionally exclude Sales/Support).
            bool output = false;
            if (
                 principal.IsInRole(FebrisUserType.FebrisDeveloper.ToString())
                    || principal.IsInRole(FebrisUserType.FebrisEngineer.ToString())
                    || principal.IsInRole(FebrisUserType.FebrisSales.ToString())
                    || principal.IsInRole(FebrisUserType.FebrisSupport.ToString())
                    || principal.IsInRole(FebrisUserType.SystemAdmin.ToString())
                    || principal.IsInRole(FebrisUserType.SuperAdmin.ToString())
                ) { output = true; }

            return output;//string.Equals(output, id, StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsSysAdmin(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (principal.IsInRole(FebrisUserType.SystemAdmin.ToString())
                || principal.IsInRole(FebrisUserType.SuperAdmin.ToString())) { output = true; }
            return output;
        }
        public static bool IsSuperAdmin(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (principal.IsInRole(FebrisUserType.SuperAdmin.ToString())) { output = true; }
            return output;
        }
        public static bool IsFebrisAdmin(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (
                    principal.IsInRole(FebrisUserType.FebrisEngineer.ToString())
                    || principal.IsInRole(FebrisUserType.SystemAdmin.ToString())
                    || principal.IsInRole(FebrisUserType.SuperAdmin.ToString())
                ) { output = true; }
            return output;
        }
        public static bool HasInstiution(this ClaimsPrincipal principal)
        {
            bool output = false;
            // Fix: the claim is written as "Institution" by SupplementalClaimFactory
            // (central SSO and EndUser). The reader previously looked for the
            // misspelled "Instiution", so HasInstiution/GetInstiution always
            // returned false/null and org-scoping silently broke.
            var item = principal.Claims.FirstOrDefault(c => c.Type == "Institution");
            if (item?.Value != Guid.Empty.ToString() && !string.IsNullOrEmpty(item?.Value))
            {
                output = true;
            }
            //return item?.Value;
            return output;
        }
        public static string GetInstiution(this ClaimsPrincipal principal)
        {
            // Fix: the claim is written as "Institution" by SupplementalClaimFactory
            // (central SSO and EndUser). The reader previously looked for the
            // misspelled "Instiution", so HasInstiution/GetInstiution always
            // returned false/null and org-scoping silently broke.
            var item = principal.Claims.FirstOrDefault(c => c.Type == "Institution");
            return item?.Value;
        }


        //public static bool HasSignedPaperwork(this ClaimsPrincipal principal)
        //{
        //    bool output = false;
        //    try
        //    {
        //        var item = principal.Claims.FirstOrDefault(c => c.Type == "LiabilityWaiver");
        //        if (item != null
        //            && !string.IsNullOrEmpty(item.Value)
        //            && Guid.Parse(item.Value) != Guid.Empty
        //            //&& (
        //            //    principal.IsInRole(UserAccountType.ITAdmin.ToString())
        //            //    || principal.IsInRole(UserAccountType.Admin.ToString())
        //            //    || principal.IsInRole(UserAccountType.User.ToString())
        //            //)
        //            )
        //        {
        //            output = true;
        //        }
        //        var item2 = principal.Claims.FirstOrDefault(c => c.Type == "ServiceAgreement");
        //        if (item2 != null
        //            && !string.IsNullOrEmpty(item.Value)
        //            && Guid.Parse(item.Value) != Guid.Empty
        //            //&& (
        //            //    principal.IsInRole(UserAccountType.ITAdmin.ToString())
        //            //    || principal.IsInRole(UserAccountType.Admin.ToString())
        //            //    || principal.IsInRole(UserAccountType.User.ToString())
        //            //)
        //            )
        //        {
        //            output = true;
        //        }
        //    }
        //    catch { }
        //    return output;
        //}

    }

    public static class DeveloperClaimsPrincipalExtension
    {
        public static string ContentDeveloper(this ClaimsPrincipal principal)
        {
            var item = principal.Claims.FirstOrDefault(c => c.Type == "ContentDeveloper");
            return item?.Value;
        }
        public static string AccreditationBody(this ClaimsPrincipal principal)
        {
            var item = principal.Claims.FirstOrDefault(c => c.Type == "AccreditationBody");
            return item?.Value;
        }
        public static bool IsContentDeveloper(this ClaimsPrincipal principal)
        {
            bool output = false;
            try
            {
                var item = principal.Claims.FirstOrDefault(c => c.Type == "ContentDeveloper");
                if (item != null
                    && !string.IsNullOrEmpty(item.Value)
                    && Guid.Parse(item.Value) != Guid.Empty
                    && (
                        principal.IsInRole(UserAccountType.ITAdmin.ToString())
                        || principal.IsInRole(UserAccountType.Admin.ToString())
                        || principal.IsInRole(UserAccountType.User.ToString())
                    )
                    )
                {
                    output = true;
                }
            }
            // intentional: best-effort claim probe; malformed/absent ContentDeveloper claim degrades to false
            catch { Febris.SharedServices.FebrisLog.Warn("DeveloperClaimsPrincipalExtension.IsContentDeveloper: claim parse failed, treating as not a content developer"); }
            return output;
            //this will not work because it is a guid not a string
            //if (string.IsNullOrEmpty(item.Value))
            //{
            //    output = true;
            //}

            //if (
            //        principal.IsInRole(ContentDeveloperUserType.CCUser.ToString())
            //        || principal.IsInRole(ContentDeveloperUserType.CCAdmin.ToString())
            //        || principal.IsInRole(ContentDeveloperUserType.CCITAdmin.ToString())
            //    ) { output = true; }

        }
        public static bool IsAccreditationBody(this ClaimsPrincipal principal)
        {
            bool output = false;
            try
            {
                var item = principal.Claims.FirstOrDefault(c => c.Type == "AccreditationBody");
                if (item != null
                    && !string.IsNullOrEmpty(item.Value)
                    && Guid.Parse(item.Value) != Guid.Empty
                    && (
                        principal.IsInRole(UserAccountType.ITAdmin.ToString())
                        || principal.IsInRole(UserAccountType.Admin.ToString())
                        || principal.IsInRole(UserAccountType.User.ToString())
                        )
                        )
                {
                    output = true;
                }
            }
            // intentional: best-effort claim probe; malformed/absent AccreditationBody claim degrades to false
            catch //(Exception)
            {
                Febris.SharedServices.FebrisLog.Warn("DeveloperClaimsPrincipalExtension.IsAccreditationBody: claim parse failed, treating as not an accreditation body");
            }
            return output;
            //if (
            //        principal.IsInRole(AccreditationBodyUserType.ABUser.ToString())
            //        || principal.IsInRole(AccreditationBodyUserType.ABAdmin.ToString())
            //        || principal.IsInRole(AccreditationBodyUserType.ABITAdmin.ToString())
            //    ) { output = true; }

        }
        public static bool IsContentDeveloperAndAccreditationBody(this ClaimsPrincipal principal)
        {
            bool output = false;
            bool isDev = IsContentDeveloper(principal);
            bool isAccred = IsAccreditationBody(principal);
            if (isDev && isAccred)
            {
                output = true;
            }
            return output;
        }


    }

    //public static class LicenseClaimsPrincipalExtension
    //{
    //    public static bool HasInstiution(string jwtToken)
    //    {
    //        bool output = false;
    //        var handler = new JwtSecurityTokenHandler();
    //        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
    //        var item = jwtSecurityToken.Claims.First(claim => claim.Type == "Instiution");//.Value;            
    //       // var item = principal.Claims.FirstOrDefault(c => c.Type == "Instiution");
    //        if (item?.Value != Guid.Empty.ToString() && !string.IsNullOrEmpty(item?.Value))
    //        {
    //            output = true;
    //        }
    //        //return item?.Value;
    //        return output;
    //    }
    //    public static string GetInstiution(string jwtToken)
    //    {            
    //        var handler = new JwtSecurityTokenHandler();
    //        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
    //        var item = jwtSecurityToken.Claims.First(claim => claim.Type == "Instiution");//.Value;
    //        return item?.Value;
    //    }

    //    public static bool HasLicense(string jwtToken)
    //    {
    //        bool output = false;
    //        var handler = new JwtSecurityTokenHandler();
    //        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
    //        var item = jwtSecurityToken.Claims.First(claim => claim.Type == "License");//.Value;            
    //                                                                                      // var item = principal.Claims.FirstOrDefault(c => c.Type == "Instiution");
    //        if (item?.Value != Guid.Empty.ToString() && !string.IsNullOrEmpty(item?.Value))
    //        {
    //            output = true;
    //        }
    //        //return item?.Value;
    //        return output;
    //    }

    //    public static string GetLicense(string jwtToken)
    //    {
    //        var handler = new JwtSecurityTokenHandler();
    //        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
    //        var item = jwtSecurityToken.Claims.First(claim => claim.Type == "License");//.Value;
    //        return item?.Value;
    //    }

    //    public static bool IsLockedOut(string jwtToken)
    //    {
    //        bool output = false;
    //        var handler = new JwtSecurityTokenHandler();
    //        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
    //        var item = jwtSecurityToken.Claims.First(claim => claim.Type == "IsLockedOut");            
    //        if (!string.IsNullOrEmpty(item?.Value))
    //        {
    //            output = true;
    //        }
    //        return output;
    //    }

    //}



    /// <summary>
    /// A-02 Stage 2 (2026-05-20): claim accessors for the License/Hardware
    /// data that the JwtLicenseMiddleware / JwtHardwareMiddleware now
    /// pipe into <see cref="System.Security.Claims.ClaimsPrincipal"/>
    /// (in addition to the legacy <c>HttpContext.Items["License"]</c> /
    /// <c>HttpContext.Items["Hardware"]</c> entries which continue to
    /// work for backward compatibility with existing BLL code).
    /// <para>
    /// Migration target: BLL methods that currently cast
    /// <c>HttpContext.Items["License"]</c> to a <c>License</c> object
    /// switch to <c>User.GetTenantId()</c> / <c>GetLicenseRole()</c> /
    /// <c>GetLicenseKey()</c> over time. The two paths coexist during
    /// the migration; either reads the same JWT-claim data.
    /// </para>
    /// </summary>
    public static class DeviceKeyClaimsPrincipalExtension
    {
        /// <summary>
        /// Returns the License-tier tenant id (Institution UUID) carried
        /// by a license-authenticated request. Null when the principal has
        /// no <c>TenantId</c> claim (e.g., user-cookie request, or no
        /// license attached).
        /// </summary>
        public static Guid? GetTenantId(this ClaimsPrincipal principal)
        {
            var claim = principal?.FindFirst("TenantId");
            if (claim == null) return null;
            return Guid.TryParse(claim.Value, out var g) ? g : (Guid?)null;
        }

        /// <summary>
        /// Returns the license key (Guid) carried by a license-authenticated
        /// request. Null when no <c>LicenseKey</c> claim is present.
        /// </summary>
        public static Guid? GetLicenseKey(this ClaimsPrincipal principal)
        {
            var claim = principal?.FindFirst("LicenseKey");
            if (claim == null) return null;
            return Guid.TryParse(claim.Value, out var g) ? g : (Guid?)null;
        }

        /// <summary>
        /// Returns the <see cref="Febris.EnumLibrary.AccountType"/> tier
        /// recorded on the License at JWT issuance time (string form). Null
        /// when no <c>LicenseRole</c> claim is present. Stage 3 of the
        /// A-02 milestone introduces ASP.NET policies that read this claim.
        /// </summary>
        public static string GetLicenseRole(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst("LicenseRole")?.Value;
        }

        /// <summary>
        /// True if the License was marked <c>AccountLocked</c> at JWT
        /// issuance. Stage 2's middleware pipes this through so
        /// <c>OwnershipChecks</c> can reject locked requests early
        /// without re-reading <c>HttpContext.Items["License"]</c>.
        /// Caveat: this is stale-at-issuance per the A-02 design notes;
        /// per-request DB re-fetch is Stage 3 work.
        /// </summary>
        public static bool IsLicenseLockedFromClaim(this ClaimsPrincipal principal)
        {
            var claim = principal?.FindFirst("LicenseLocked");
            return claim != null && bool.TryParse(claim.Value, out var b) && b;
        }
    }

    public static class LicenseClaimsPrincipalExtension
    {
        public async static Task<Febris.ModelLibrary.Models.DataModels.License> GetLicense(IHttpContextAccessor context)
        {
            Febris.ModelLibrary.Models.DataModels.License output = (Febris.ModelLibrary.Models.DataModels.License)context.HttpContext.Items["License"];

            if (output is null)
            {
                return default;
            }

            return output;
        }

        public async static Task<bool> HasLicense(IHttpContextAccessor context)
        {
            bool output = false;
            try
            {
                Febris.ModelLibrary.Models.DataModels.License license = (Febris.ModelLibrary.Models.DataModels.License)context.HttpContext.Items["License"];
                if (license == null || license == default)
                {
                    //output = 
                }
                else
                {
                    output = true;
                }
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
            }
            return output;
        }
    }

    public static class LocalClaimsPrincipalExtension
    {
        public static bool IsLocalUser(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (principal.IsInRole(InstitutionUserAccountType.User.ToString())) { output = true; }
            return output;
        }
        public static bool IsLocalParent(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (principal.IsInRole(InstitutionUserAccountType.UserParent.ToString())) { output = true; }
            return output;
        }
        public static bool IsLocalEducator(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (principal.IsInRole(InstitutionUserAccountType.Educator.ToString())) { output = true; }
            return output;
        }
        public static bool IsLocalAdmin(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (principal.IsInRole(InstitutionUserAccountType.Admin.ToString())
                || principal.IsInRole(InstitutionUserAccountType.ITAdmin.ToString())) { output = true; }
            return output;
        }
        public static bool IsLocalITAdmin(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (principal.IsInRole(InstitutionUserAccountType.ITAdmin.ToString())) { output = true; }
            return output;
        }
        public static bool IsLocalFebrisAdmin(this ClaimsPrincipal principal)
        {
            bool output = false;
            if (principal.IsInRole(FebrisUserType.SuperAdmin.ToString())) { output = true; }
            return output;
        }



        
    }
}
