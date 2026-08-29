// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.SharedServices.Launcher
{
    public class LauncherSharedDetails
    {
        public const string UrlStart = "https://";

        // Base API URL for the launcher's outbound calls. Deliberately empty and set by the
        // operator at runtime rather than baked in per build configuration: a launcher compiled
        // against one deployment's host is useless to anyone else, and the previous per-config
        // hosts pointed at infrastructure a self-hoster does not own. Same convention the mobile
        // Server head already uses (LocalHardwareStaticDetails.ApiUrl).
        public static string ApiUrl = string.Empty;


        //Url things
        public static string VideoUploaderUrl = ApiUrl + "VideoUploader";
        public static string xAPIStatementUrl = ApiUrl + "xapi";
        public static string ModuleDownloaderUrl = ApiUrl + @"ModuleDownload/ModuleDownloader/";
        public static string ModuleCheckingUrl = ApiUrl + @"ModuleDownload/ModuleChecker";
        public static string LauncherInitializer = ApiUrl + @"Launcher";
        public static string StatementInitializer = ApiUrl + @"Launcher/StatementInitializer";

        //credentials
        public static string getToken = ApiUrl + "token";
                
        //service Names
        public const string uploaderName = "FebrisBackgroundUploader";
        public const string ModuleManagerName = "FebrisModuleManager";

        //video data
        //public static string videoName;
        public static bool SimulationIsRunning = false;

        //argument constants
        public const string StatementPreface = "-febrisData=";
        public const int StatementPrefaceLength = 12;
        public const string VideoDataPreface = "-videoData=";
        public const int VideoDataPrefaceLength = 11;
        public const string SimulationProcessIdPreface = "-simulationProcessId=";
        public const int SimulationProcessIdPrefaceLength = 21;
        public const string SimulationProcessName = "-simulationProcessName=";
        public const int SimulationProcessNameLength = 23;
        
    }
    // ServiceOptions + ProcessOptions enums moved to Febris.EnumLibrary
    // per the "all enums live in FebrisEnumLibrary" rule.
}
