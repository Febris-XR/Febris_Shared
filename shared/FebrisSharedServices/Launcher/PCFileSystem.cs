// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Febris.SharedServices.Launcher
{
    public class PCFileSystem
    {
        //File structure
        //########################################################################
        //Base febrisfile
        //-Media
        //--Video
        //---splitvideo
        //---recordings  
        //---ZippedRecordings
        //-Modules
        //  - links
        //  - module files
        //-Statements
        //Logs
        //  - Uploader
        //  - Launcher
        //########################################################################
        //local folder paths        
        public static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Febris");
        //########################################################################
        //media
        //########################################################################
        public static string MediaPath = Path.Combine(BasePath, "Media");
        #region Video Paths
        //########################################################################
        //inside Media directory
        //########################################################################
        //video path
        public static string VideoPath = Path.Combine(MediaPath, "Videos");
        //########################################################################
        //inside video directory
        //########################################################################
        ////split file path
        public static string SplitFilePath = Path.Combine(VideoPath, "SplitVideos");
        //recording path
        public static string RecordingsFilePath = Path.Combine(VideoPath, "Recordings");
        //recording path
        public static string TempRecordingsFilePath = Path.Combine(VideoPath, "TempRecording");
        //zipped video path        
        public static string zipFolderPath = Path.Combine(VideoPath, "ZippedRecordings");
        #endregion
        #region Module Paths
        //########################################################################
        //Modules
        //########################################################################
        public static string BaseModulePath = Path.Combine(BasePath, "Modules");
        //########################################################################
        //inside module directory
        //########################################################################
        public static string ModuleLinkPath = Path.Combine(BaseModulePath, "ModuleLinks");
        public static string ModulePath = Path.Combine(BaseModulePath, "Modules");
        public static string ZippedModulePath = Path.Combine(BaseModulePath, "ZippedModuleFiles");
        //internal static string simulationName = string.Empty;
        //internal static string ModuleLinkLaunch = ModuleLinkPath+@"\" + simulationName + ".lnk";
        #endregion
        //########################################################################
        //Statements directory
        //########################################################################
        public static string BaseStatementPath = Path.Combine(BasePath, "statements");
        //########################################################################
        //inside statement directory
        //########################################################################
        public static string StatementPath = Path.Combine(BaseStatementPath, "statements");
        public static string WorkingStatementPath = Path.Combine(BaseStatementPath, "workingstatement");
        public static string OldStatementPath = Path.Combine(BaseStatementPath, "oldstatements");
        //########################################################################
        //Logs
        //########################################################################
        public static string BaseLogPath = Path.Combine(BasePath, "Logs");
        //########################################################################
        //inside Log directory
        //########################################################################
        public static string UploaderLogPath = Path.Combine(BaseLogPath, "UploaderLogs");
        public static string LauncherLogPath = Path.Combine(BaseLogPath, "LauncherLogs");
        public static string RecorderLogPath = Path.Combine(BaseLogPath, "RecorderLogs");
        public static string ModuleManagerLogPath = Path.Combine(BaseLogPath, "ModuleManagerLogs");
        public static string SimulationLogBasePath = Path.Combine(BaseLogPath, "SimulationLogs");
        //########################################################################
        //Credentials
        //########################################################################
        public static string sLocation = Path.Combine(BasePath, "cred");
        //########################################################################
        //inside credential directory
        //########################################################################
        public static string userNameLocation = Path.Combine(sLocation, "user.dat");
        public static string passwordLocation = Path.Combine(sLocation, "s.dat");
        public static string ConfigLocation = Path.Combine(sLocation, "config.json");
        // NODE-9. The device credential the node MINTS at registration, stored the same encrypted
        // way as the user credentials beside it. This replaced deriving a licence from WMI: audit
        // T9 made the node generate the credential and store only its hash, so a value the client
        // computes for itself can never match. The operator copies the minted string in once.
        public static string deviceCredentialLocation = Path.Combine(sLocation, "d.dat");

        //########################################################################
        //inside directory?
        //########################################################################

        //Need dll assembly


        //#################################################################

        //.exe paths --- I am not sure this is needed. Can just search for the process.
        //DISABLED 2026-07-25. This walked three levels up from the CURRENT WORKING DIRECTORY, so
        //.Parent returned null and the field initializer threw a NullReferenceException whenever the
        //process ran from fewer than three levels below the drive root. Because it is a static field
        //initializer the failure surfaced as a TypeInitializationException for PCFileSystem and
        //cascaded to every type touching it (observed taking down ScreenRecorderStaticDetails). An
        //install at "C:\Program Files\Febris" running with its own directory as the CWD hits exactly
        //that. The field was never read anywhere in the codebase, and the mobile copy of this same
        //line was already commented out (Febris.SharedMobileLibrary/FileSystem/FileSystem.cs), so
        //this matches that precedent. Resolve paths from AppContext.BaseDirectory if ever needed.
        //public static string localPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        public static string uploaderName = "Febris.PCStatementManagerV3.exe";
        public static string downloaderName = "Febris.PCModuleManagerV3.exe"; 
        public static string UploaderPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), uploaderName);
        public static string DownloaderPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), downloaderName);
        public static string ProgressBarPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"Febris.ConsoleProgressBar.exe");
        public static string screenRecorderPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"Febris.PCScreenRecorderV3.exe");
        //public static string uploaderName = "FebrisBackgroundUploader.exe";// Path.Combine(SoftwareExtensionPaths, @"\Uploader\FebrisBackgroundUploader.exe");
        //public static string downloaderName = "FebrisModuleManager.exe";// Path.Combine(SoftwareExtensionPaths, @"\ModuleDownloader\FebrisModuleManger.exe");//lol I spelled it wrong on the build
        //public static string UploaderPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "FebrisBackgroundUploader.exe");
        //public static string DownloaderPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "FebrisModuleManager.exe");
        //public static string ProgressBarPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"Febris.ConsoleProgressBar.exe");
        //public static string screenRecorderPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"FebrisScreenRecorder.exe");
    }
    public class FileSystemInitalizer
    {

        private readonly IConfiguration _config;
        private ILogger _log;

        public FileSystemInitalizer(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public void FileInitalizer()
        {
            try
            {
                List<string> FileList = new List<string>
                {
                    PCFileSystem.BasePath,
                    PCFileSystem.MediaPath,
                    PCFileSystem.VideoPath,
                    PCFileSystem.SplitFilePath,
                    PCFileSystem.RecordingsFilePath,
                    PCFileSystem.TempRecordingsFilePath,
                    PCFileSystem.zipFolderPath,
                    PCFileSystem.BaseModulePath,
                    PCFileSystem.ModuleLinkPath,
                    PCFileSystem.ModulePath,
                    PCFileSystem.ZippedModulePath,
                    PCFileSystem.BaseStatementPath,
                    PCFileSystem.StatementPath,
                    PCFileSystem.WorkingStatementPath,
                    PCFileSystem.OldStatementPath,
                    PCFileSystem.BaseLogPath,
                    PCFileSystem.UploaderLogPath,
                    PCFileSystem.LauncherLogPath,
                    PCFileSystem.RecorderLogPath,
                    PCFileSystem.SimulationLogBasePath,
                    PCFileSystem.ModuleManagerLogPath,
                    PCFileSystem.sLocation,                    
                    //FileSystem.SharedDataPath
                };

                foreach (var file in FileList)
                {
                    try
                    {
                        CreateFileDirectory(file, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
        }
        //#############################################################################
        // connecting to the file storage retriever
        //#############################################################################
        public void CreateFileDirectory(string path, string name)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(path, name));
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
        }

    }
}
