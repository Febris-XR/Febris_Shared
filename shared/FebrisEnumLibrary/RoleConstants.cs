// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
namespace Febris.Constants
{
    /// <summary>
    /// Centralized role-list strings for <c>[Authorize(Roles = ...)]</c> gates (audit MED-7).
    /// Attribute arguments must be compile-time constants, so each entry is a <c>const string</c>
    /// whose value is IDENTICAL to the literal it replaces. The MED-7 sweep is therefore a pure,
    /// behavior-preserving substitution.
    /// <para>
    /// Role names match the <c>FebrisUserType</c> / <c>InstitutionUserAccountType</c> enum names
    /// (see <c>Febris.EnumLibrary.UserAccountType</c>). Authorize treats the list as an OR and is
    /// order-independent, so order-only variants of the same set share a single const.
    /// </para>
    /// <para>
    /// NOTE (latent bug, preserved here, not fixed): a few historical gates embed SPACES after the
    /// commas (for example "SystemAdmin, SuperAdmin"). ASP.NET Core splits <c>Roles</c> on ',' without
    /// trimming, so a leading-space role name may fail to match and the gate can grant only its first
    /// role. Those variants are kept verbatim below to avoid changing behavior during a refactor.
    /// Normalizing them (removing the spaces) is a deliberate behavior change tracked as a follow-up.
    /// </para>
    /// </summary>
    public static class RoleConstants
    {
        /// <summary>All Febris internal staff roles (the dominant gate, ~119 sites).</summary>
        public const string FebrisStaff =
            "FebrisSales,FebrisSupport,FebrisDeveloper,FebrisEngineer,SystemAdmin,SuperAdmin";

        /// <summary>Every end-user / org role including the parent role (broadest read gate).</summary>
        public const string EndUserAll =
            "User,Educator,Admin,ITAdmin,UserParent,SuperAdmin";

        /// <summary>End-user / org roles excluding the parent role.</summary>
        public const string EndUserNoParent =
            "User,Educator,Admin,ITAdmin,SuperAdmin";

        /// <summary>Org member plus org administrators.</summary>
        public const string OrgMemberAndAdmins =
            "User,Admin,ITAdmin";

        /// <summary>Educator plus org administrators.</summary>
        public const string EducatorAndOrgAdmins =
            "Educator,Admin,ITAdmin,SuperAdmin";

        /// <summary>Org administrators.</summary>
        public const string OrgAdmins =
            "Admin,ITAdmin,SuperAdmin";

        /// <summary>Org admin plus IT admin (no super-admin).</summary>
        public const string OrgAdminAndItAdmin =
            "Admin,ITAdmin";

        /// <summary>IT admin plus super-admin.</summary>
        public const string ItAdminAndSuperAdmin =
            "ITAdmin,SuperAdmin";

        /// <summary>Febris system administrators (no-space form).</summary>
        public const string FebrisSystemAdmins =
            "SystemAdmin,SuperAdmin";

        // --- spaced legacy variants, kept verbatim (see the class note on the latent bug) ---

        /// <summary>Febris system administrators, legacy spaced form (~3 sites).</summary>
        public const string FebrisSystemAdminsSpaced =
            "SystemAdmin, SuperAdmin";

        /// <summary>Febris employee plus system admins, legacy spaced form (~9 sites).</summary>
        public const string FebrisEmployeeAndSystemAdmins =
            "FebrisEmployee, SuperAdmin, SystemAdmin";

        /// <summary>Broad org / staff legacy gate, spaced form (~6 sites).</summary>
        public const string OrgStaffLegacy =
            "Educator, Supervisor, Executive, Legal, ITAdmin, FebrisEmployee, SuperAdmin, SystemAdmin";
    }
}
