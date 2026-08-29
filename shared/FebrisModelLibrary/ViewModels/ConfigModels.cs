// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class ConfigModels
    {
    }

    public class JWTSettingsModel
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }
        public string ExpiryTimeInSeconds { get; set; }
    }

    public class SmbSettings
    {
        public string Secret { get; set; }
        public string UserName { get; set; }
        public string Path { get; set; }
    }

    public class UserAPIConfig
    {
        public string UserManagementAPI { get; set; }
        public string UserLoginAPI { get; set; }
        public string UserLogoutAPI { get; set; }
        //public string UserAuthAPI { get; set; }
        //public string CompanionAPI { get; set; }
        //public string API { get; set; }
        public string UserAPI { get; set; }
        public string IdentityAPI { get; set; }
    }

    //public class LocalUserAPIConfig
    //{
    //    //public string UserManagementAPI { get; set; }
    //    //public string UserLoginAPI { get; set; }
    //    //public string UserLogoutAPI { get; set; }
    //    //public string UserAuthAPI { get; set; }
    //    //public string CompanionAPI { get; set; }
    //    //public string API { get; set; }
    //    public string UserAPI { get; set; }
    //    public string IdentityAPI { get; set; }
    //}


    // CertificationSettingsViewModel DELETED (ROADMAP 18, 2026-08-23). It was bound on the node
    // API host and injected by nothing in any tier; the Portal never bound it. Its six credential
    // keys sat in both node templates as placeholders that no code could ever read.

}
