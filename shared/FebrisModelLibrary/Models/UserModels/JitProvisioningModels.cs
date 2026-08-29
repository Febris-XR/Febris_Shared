// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;

namespace Febris.ModelLibrary.Models.UserModels
{
    // SSO JIT-provisioning transport shapes. Relocated from
    // Febris.SSO.BLL (JitProvisioningService.cs) per the "models + view models
    // live in FebrisModelLibrary" rule (R1). JitProvisioningRequest is built by
    // the OIDC/SAML controllers and passed into the BLL, so it crosses the
    // controller->BLL boundary and belongs here.

    /// <summary>
    /// Inputs to <c>IJitProvisioningService.EnsureUserAsync</c>.
    /// </summary>
    public class JitProvisioningRequest
    {
        public InstitutionIdentityProvider Binding { get; set; }
        public string ExternalSubject { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public string RemoteIp { get; set; }
        public string UserAgent { get; set; }
    }

    /// <summary>
    /// Outcome of <c>IJitProvisioningService.EnsureUserAsync</c>.
    /// </summary>
    public class JitProvisioningOutcome
    {
        public bool Success { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsNewUser { get; set; }
        public string Error { get; set; }
    }
}
