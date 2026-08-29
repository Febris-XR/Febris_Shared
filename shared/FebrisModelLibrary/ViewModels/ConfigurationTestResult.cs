// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.UserModels;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Enterprise Tier 1: result of
    /// <c>InstitutionIdentityProviderLogic.TestConfigurationAsync</c>.
    /// Lightweight pre-flight check on a persisted binding -- does NOT
    /// initiate a real login (that requires interacting with the
    /// customer's IdP). Used by the AdminPortal "Test config" button
    /// to catch obvious misconfigurations before going live.
    /// </summary>
    public class ConfigurationTestResult
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
        public InstitutionIdentityProvider Binding { get; set; }
    }
}
