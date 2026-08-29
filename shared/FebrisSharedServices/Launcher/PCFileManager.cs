// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;


namespace Febris.SharedServices.Launcher
{
    public class PCFileManager
    {
        private ILogger _log;
        private IConfiguration _config;

        public PCFileManager(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public PCFileManager()
        {
        }

        //#########################################################
        // Get file directory content
        //#########################################################
        public List<string> GetDirectoryContentNames(string path)
        {
            try
            {
                IEnumerable<string> files = Directory.EnumerateFileSystemEntries(path);
                List<string> fileNames = new List<string>();
                foreach (var file in files)
                {
                    string tempName = Path.GetFileName(file).ToString();
                    Febris.SharedServices.FebrisLog.Info(tempName);
                    fileNames.Add(tempName);
                }
                return fileNames;
            }
            catch (Exception ex)
            {
                _log.LogInformation(ex.Message);
                //throw;
                return null;
            }
        }

        #region Get content
        //#########################################################
        // Get file directory content
        //#########################################################
        public string GetFileContent(string path, string name)
        {
            try
            {
                string fileName = Path.Combine(path, name);
                string content = File.ReadAllText(fileName);
                return content;
            }
            catch (Exception ex)
            {
                _log.LogInformation(ex.Message);
                return null;
                //throw;
            }
        }
        //#########################################################
        // Get file directory content
        //#########################################################
        public string GetFileContent(string path)
        {
            try
            {
                string content = File.ReadAllText(path);
                return content;
            }
            catch (Exception ex)
            {
                _log.LogInformation(ex.Message);
                return null;
                //throw;
            }
        }
        #endregion

        #region Set content
        public bool Set(JObject input, string path)
        {
            bool dataWritten = false;
            using (StreamWriter file = new StreamWriter(path))
            {
                try
                {
                    string statementString = SerializeString(input);
                    //file.Write(SerializeString(statementString));
                    file.Write(statementString);
                    //SerializeString(statement);
                    dataWritten = true;
                }
                catch (Exception ex)
                {
                    //_log.Error("WriteToDataFile Error: " + ex.Message);
                    throw;
                }
            }
            return dataWritten;
        }

        public bool Set(string input, string path)
        {
            bool dataWritten = false;
            using (StreamWriter file = new StreamWriter(path))
            {
                try
                {
                    //file.Write(SerializeString(statementString));
                    file.Write(input);
                    //SerializeString(statement);
                    dataWritten = true;
                }
                catch (Exception ex)
                {
                    //_log.Error("WriteToDataFile Error: " + ex.Message);
                    throw;
                }
            }
            return dataWritten;
        }
        #endregion

        #region Move File
        public bool MoveStatementFileToSent(string name)
        {
            try
            {
                string currentFileLocaiton = Path.Combine(PCFileSystem.StatementPath, name);
                string newFileLocation = Path.Combine(PCFileSystem.OldStatementPath, name);
                File.Move(currentFileLocaiton, newFileLocation);                                
                return true;
            }
            catch (Exception ex)
            {
                _log.LogInformation(ex.Message);
                return false;
                //throw;
            }
        }



        #endregion

        #region miss
        public static string SerializeString(JObject jObject)
        {
            string outputString = string.Empty;
            try
            {
                outputString = JsonConvert.SerializeObject(jObject);
            }
            catch (Exception ex)
            {
                //_log.Error("SerializeString Error: " + ex.Message);
            }
            return outputString;
        }
        public static JObject ChangeToObject(string inputString)
        {
            JObject jObject = new JObject();
            try
            {
                jObject = JObject.Parse(inputString);
            }
            catch (Exception ex)
            {
                //_log.Error("ChangeToObject Error: " + ex.Message);
            }
            return jObject;
        }

        public bool ZipVideoFile(string fileName)//, object p)
        {
            bool rslt = false;
            //string filePath = StaticDetails.VideoPath;
            string filePath = PCFileSystem.VideoPath;
            //string zipFolderPath = StaticDetails.zipFolderPath;
            string zipFolderPath = PCFileSystem.zipFolderPath;
            string BaseFileName = Path.GetFileNameWithoutExtension(fileName);
            //var zipPath = Path.Combine(StaticDetails.VideoPath, BaseFileName + ".zip");            
            var zipPath = Path.Combine(PCFileSystem.VideoPath, BaseFileName + ".zip");
            //this creates the zip but puts nothing in it. 
            try
            {
                //create a folder
                string newFolderName = Path.Combine(filePath, BaseFileName);
                Directory.CreateDirectory(newFolderName);

                //move file to that folder
                string moveFileToNewFolder = Path.Combine(Path.Combine(filePath, newFolderName), fileName);
                string currentFileLocation = Path.Combine(PCFileSystem.RecordingsFilePath, fileName);
                bool movedFolder = MoveFolder(currentFileLocation, moveFileToNewFolder);
                if (movedFolder == false)
                {
                    //somthing
                }


                //zip the folder
                ZipFile.CreateFromDirectory(newFolderName, zipPath);
                //move to the zip folder
                bool zipFileMoved = MoveFolder(zipPath, Path.Combine(zipFolderPath, BaseFileName + ".zip"));
                if (zipFileMoved == true)
                {
                    _ = DeleteSplitFiles();
                    _ = DeleteFolders(newFolderName);
                }


                rslt = true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return rslt;
        }

        public bool DeleteFolders(string FullFileName)
        {
            bool rslt = false;

            try
            {
                Directory.Delete(FullFileName, true);
                rslt = true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return rslt;
        }

        public bool DeleteSplitFiles()
        {
            bool rslt = false;

            try
            {
                //IEnumerable<string> splitFiles = Directory.EnumerateFileSystemEntries(StaticDetails.SplitFilePath);
                IEnumerable<string> splitFiles = Directory.EnumerateFileSystemEntries(PCFileSystem.SplitFilePath);
                foreach (var file in splitFiles)
                {
                    File.Delete(file);
                }

                rslt = true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return rslt;
        }
        #endregion

        public bool MoveFolder(string FullCurrentPath, string FullNewPath)
        {
            bool rslt = false;
            try
            {
                File.Move(FullCurrentPath, FullNewPath);
                rslt = true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return rslt;
        }


        #region zipped file system
        //public bool FileUnzipper(string zipFile, Guid module, string linkName)
        //{
        //    bool unzipped = false;
        //    //Process process = FebrisLocalLibrary.Service.ProgressBarService.StartProgressBar(zipFile, FebrisLocalLibrary.Enums.StatusType.Processing);
        //    try
        //    {
        //        string zipPath = zipFile;
        //        // Normalizes the path.
        //        //string extractPath = Path.GetFullPath(StaticDetails.ModulePath);
        //        string extractPath = Path.GetFullPath(PCFileSystem.ModulePath);
        //        string destinationPath = string.Empty;

        //        destinationPath = Path.GetFullPath(Path.Combine(extractPath, Path.GetFileNameWithoutExtension(zipFile)));

        //        ZipFile.ExtractToDirectory(zipPath, destinationPath);
        //        unzipped = true;

        //        //await RemoveOldEditions(StaticDetails.tempLinkName);
        //        Task.Run(() => FindSimulationApplication(destinationPath, module.ToString(), linkName)).Wait();
        //        //await FindSimulationApplication(destinationPath).Wait();

        //    }
        //    catch (Exception e)
        //    {
        //        _log.LogError(e.Message);
        //    }
        //    //FebrisLocalLibrary.Service.ProgressBarService.StopProgressBar(process);
        //    return unzipped;
        //}

        //public async Task<bool> FindSimulationApplication(string rootDirectory, string moduleName, string linkName)
        //{
        //    //Process process = FebrisLocalLibrary.Service.ProgressBarService.StartProgressBar("Simulation", FebrisLocalLibrary.Enums.StatusType.Installing);           

        //    bool simulationFound = false;
        //    //bool rslt = false;
        //    //find enumerable list of directories
        //    //IEnumerable<string> folders = Directory.EnumerateDirectories(StaticDetails.zippedModulePath);
        //    //IEnumerable<string> files = Directory.EnumerateFiles(StaticDetails.zippedModulePath);
        //    try
        //    {
        //        string[] allfiles = Directory.GetFiles(rootDirectory, "*.exe", SearchOption.AllDirectories);

        //        foreach (string i in allfiles)
        //        {
        //            if (i != "UnityCrashHandler64.exe")
        //            {
        //                moduleName = i;
        //                simulationFound = true;
        //                break;
        //            }
        //        }
        //        //await CreateShortCut(StaticDetails.tempLinkName, StaticDetails.tempModuleName);
        //        Task.Run(() => CreateShortCut(linkName, moduleName)).Wait();
        //        //return rslt;
        //        //FebrisLocalLibrary.Service.ProgressBarService.StopProgressBar(process);
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex.Message);
        //    }
        //    return simulationFound;
        //}

        //public bool CreateShortCut(string linkName, string targetFilePath)
        //{
        //    bool shortCutCreated = false;
        //    try
        //    {
        //        //targetFilePath = Path.Combine(StaticDetails.ModulePath, targetFilePath);
        //        targetFilePath = Path.Combine(PCFileSystem.ModulePath, targetFilePath);

        //        //string shortcutLocation = Path.Combine(StaticDetails.ModuleLinkPath, linkName + ".lnk");
        //        string shortcutLocation = Path.Combine(PCFileSystem.ModuleLinkPath, linkName + ".lnk");

        //        WshShell shell = new WshShell();
        //        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

        //        shortcut.TargetPath = targetFilePath;
        //        shortcut.Save();
        //        shortCutCreated = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex.Message);
        //    }
        //    return shortCutCreated;
        //}


        #endregion
    }
}
