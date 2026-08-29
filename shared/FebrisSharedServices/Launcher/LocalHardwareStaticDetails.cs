// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.LauncherModels;
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.TicketModels;
using Febris.ModelLibrary.Models.XApiModels;
using Febris.ModelLibrary.ViewModels;
using Febris.SharedServices.Launcher;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Febris.SharedServices.Launcher
{
    public class LocalHardwareStaticDetails
    {
        //this static variable is needed to communicate        
        public static string testData = string.Empty;

        // ---------------------------------------------------------------------------------
        // SEVERANCE -- the API URL is pre-configured per deployment:
        // the launcher must point at the OPERATOR'S node, not Febris's central SaaS. ApiUrl is
        // populated at launch by URLSettingUtility.SetURL() from the persisted ConfigModel
        // (Domain/DomainPrefix/DomainPort/DomainPath, or DeveloperUrl when the DeveloperAccount
        // federation flag is opted in). It therefore ships EMPTY across all build configs: an
        // unconfigured launcher fails loudly instead of silently phoning home to Febris.
        // Superseded compiled defaults are preserved in the [Historical] region below.
        // ---------------------------------------------------------------------------------
        public static string ApiUrl = string.Empty;


        //private static string _apiUrl
        //{
        //    get
        //    {
        //        return _apiUrl;
        //    }
        //    set
        //    {
        //        _apiUrl = value;
        //        MainWindow.InitalizationRequest._endpoint = value;
        //        StatementRequest._endpoint = value;
        //        TokenRequest._endpoint = value;
        //    }
        //}


        // DeveloperUrl is the opt-in "developer account" federation endpoint (used only when
        // ConfigModel.DeveloperAccount == true). Externalized to an install-time / environment
        // parameter; the last-resort fallback is EMPTY (never the Febris SaaS host) so a
        // misconfigured install cannot phone home. Set FEBRIS_DEVELOPER_API_URL at install time
        // to point developer-mode clients at a specific node.
        public static string DeveloperUrl =
            System.Environment.GetEnvironmentVariable("FEBRIS_DEVELOPER_API_URL") ?? string.Empty;

        // Retained public symbol: was only ever used to build the pre-severance DeveloperUrl;
        // now referenced only in the historical comments below.
        public static string prefix = string.Empty;



        //#if (DEBUG)
        //        public static string ApiUrl = "http://localhost:5000/api/";

        //       // public static string ApiUrl = "https://localhost:5001/api/";
        //#elif (STAGING)        
        //#else                
        //        public static string prefix = "www";
        //#endif

        //config data
        public static string Prefix = string.Empty;

        //launch data        
        public static Module selectedModule = default;//new Module();
        public static HardwareUserViewModel selectedUser = default;// new HardwareUserViewModel();
        // ROADMAP 22: `recordSession` is deleted. Its only writer was the PC launcher's learner
        // checkbox and its only reader was the launcher's video gate, and that change removed both.
        // The record decision is the node's now, derived from the educator's per-cohort policy, and
        // the video attachment the node returns IS the instruction. A client-side copy of that
        // decision is exactly what must not exist.



        //public static bool testing = false;
        public static StatementInitializerGetViewModel statementInitalizer = new StatementInitializerGetViewModel();
        public static Statement statement = new Statement();
        public static string serializedStatement = string.Empty;

        //API pulls
        public static HardwareAuthenticationResponse _hardwareAuthenticationResponse { get; set; }
        public static HardwareInitializationResponse _hardwareInitializationResponse = new HardwareInitializationResponse();
        //public static HardwareUserInitaliztionViewModels _hardwareUserInitaliztionViewModels = new HardwareUserInitaliztionViewModels();
        //public static LauncherViewModel launcherViewModel = new LauncherViewModel(); //this is the new single pullHardwareUserInitaliztionViewModels


        //public static List<Module> _moduleList = new List<Module>();
        //public static List<MessageBoard> _localMessageBoard = new List<MessageBoard>();        
        //public static List<AdminMessageBoard> _febrisMessageBoard = new List<AdminMessageBoard>();
        //public static List<HardwareUserViewModel> _userList = new List<HardwareUserViewModel>();

        //API domain links - These obviously all need to change        
        public static string professionalImageLink = LauncherSharedDetails.ApiUrl + "HealthCareProfessionals/GetProfessionalImage/"; // this will no longer work
        //public static string DeveloperUrl;

        //search variables
        //public static string ModuleSearch = string.Empty;
        //public static string UserSearch = string.Empty;

        //test variables
        //public static bool isObsolete = false;
        ////opt in for video recording
        //public static bool RecordingOptIn = false;
    }
}
