// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.MarketingModels;
using Febris.ModelLibrary.Models.UserModels;
using Febris.ModelLibrary.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Febris.SharedServices
{
    #region base
    public interface IFileServerHandler
    {
        Task<byte[]> GetImage(string path);
        Task<FileStream> GetVideo(string path);
        Task<long> GetFileLength(string path);


        Task<JObject> JsonFileRetrieval(string path, string name);
        Task<FileStream> StatementFileRetrieval(string path, string name);
        Task<FileStream> CreationFileStream(string uploads, string input);
        void CreateFileDirectory(string uploadPath, string empty);
        Task<bool> FileExists(string uploadPath, string fileName);
        Task<bool> IsFileInUser(string input);
        string[] GetDirectoryFileList(string path, string searchpattern);
        void FileMover(string currentPath, string newPath);
        void FileDelete(string uploadPath, string fileName);
        void DeleteSplitFiles(string splitVideoFileSystemPath, string fileName);
        Task<FileStream> MergeFileStream(string fileName, FileMode open);
        Task AddFileToMerge(string input);
        //void FileDelete(string uploadPath, string fileName);
    }
    public class SmbSettings
    {
        public string Secret { get; set; }
        public string UserName { get; set; }
    }

    // FileServerHostRole enum moved to Febris.EnumLibrary per the "all enums live in FebrisEnumLibrary" rule.

    public class FileServerHandler : IFileServerHandler
    {
        //#if (!Release)
        public FileServerHandler()
        {
        }
        //#endif
        private SmbSettings smbSettings()
        {
            SmbSettings smbSettings = new SmbSettings()
            {
                Secret = StaticDetails.PassedBackConfig.GetValue<string>("SmbClient:Secret"),
                UserName = StaticDetails.PassedBackConfig.GetValue<string>("SmbClient:UserName")                
                //Secret = Smb.Configuration.GetValue<string>("SmbClient:Secret"),
                //UserName = Smb.Configuration.GetValue<string>("SmbClient:UserName")
            };
            return smbSettings;
            //throw new NotImplementedException();
        }
        private SmbSettings smbSettings(IConfiguration configuration)
        {
            SmbSettings smbSettings = new SmbSettings()
            {
                Secret = configuration.GetValue<string>("SmbClient:Secret"),
                UserName = configuration.GetValue<string>("SmbClient:UserName")
            };
            return smbSettings;
        }

        #region Generic File handling

        #region Initalization
        /// <summary>
        /// Initalizes the File system paths
        /// </summary>
        /// <param name="config"></param>        
        public async void FileInitalizer(IConfiguration config, FileServerHostRole role = FileServerHostRole.Central)
        {
            StaticDetails.BaseFileSystemPath = config.GetValue<string>("SmbClient:Path");
            StaticDetails.UniqueFileSystemPath = config.GetValue<string>("FileSystem:UniqueFileSystemPath");

            StaticDetails.SpecificFileSystemPath = StaticDetails.BaseFileSystemPath + StaticDetails.UniqueFileSystemPath;
#if (DEBUG)
            ///Media generic
            StaticDetails.MediaFileSystemPath = StaticDetails.SpecificFileSystemPath + @"media\";
            //video
            StaticDetails.VideoFileSystemPath = StaticDetails.MediaFileSystemPath + @"video\";
            StaticDetails.SplitVideoFileSystemPath = StaticDetails.VideoFileSystemPath + @"SplitVideos\";
            StaticDetails.RecordingsFileSystemPath = StaticDetails.VideoFileSystemPath + @"recordings\";
            //Images
            StaticDetails.ImageFileSystemPath = StaticDetails.MediaFileSystemPath + @"Images\";
            StaticDetails.LogoFileSystemPath = StaticDetails.ImageFileSystemPath + @"Logos\";
            StaticDetails.ProfessionalFileSystemPath = StaticDetails.ImageFileSystemPath + @"ProfessionalImages\";
            StaticDetails.ContentDeveloperLogoFileSystemPath = StaticDetails.ImageFileSystemPath + @"DeveloperLogos\";

            ///??? I think this is for the more generic setup
            StaticDetails.ProfessionalFileSystemPathForDb = @"ProfessionalImages\";
            StaticDetails.LogoFileSystemPathForDb = @"Logos\";
            StaticDetails.ContentDeveloperLogoFileSystemPathForDb = @"DeveloperLogos\";

            ///software packages for download
            StaticDetails.LocalSoftwarePackage = StaticDetails.BaseFileSystemPath + @"LocalSoftwarePackage\";

            ///Market place media (seperated by listing)
            StaticDetails.MarketplaceListingPath = StaticDetails.BaseFileSystemPath + @"MarketplaceListings\";
            StaticDetails.MarketplaceListingScreenshotPath = @"Screenshot\";
            StaticDetails.MarketplaceListingVideoPath = @"Video\";

            ///Modules
            StaticDetails.ModuleFileSystemPath = StaticDetails.BaseFileSystemPath + @"modules\";

            ///Test User Statement Files
            StaticDetails.StatementFileSystemPath = StaticDetails.SpecificFileSystemPath + @"statements\";
            StaticDetails.VoidStatementFileSystemPath = StaticDetails.StatementFileSystemPath + @"voidstatements\";
            StaticDetails.JSONStatementFileSystemPath = StaticDetails.StatementFileSystemPath + @"JSONstatements\";

            ///Publication Path
            StaticDetails.PublicationPath = StaticDetails.BaseFileSystemPath + @"Publications\";
            StaticDetails.PublicationImagePath = @"Images\";
            StaticDetails.PublicationVideoPath = @"Videos\";

            ///Email Campaign Path
            StaticDetails.EmailCampaignPath = StaticDetails.BaseFileSystemPath + @"EmailCampaign\";
            StaticDetails.EmailCampaignImagePath = @"Images\";

            ///Logs, unsure if I should be useing specificFileSystemPath if the deployment is monolithic but I also would not hurt
            StaticDetails.LogFileSystemPath = StaticDetails.SpecificFileSystemPath + @"logs\";
            StaticDetails.APILogFileSystemPath = StaticDetails.LogFileSystemPath + @"api\";
            StaticDetails.PortalLogFileSystemPath = StaticDetails.LogFileSystemPath + @"portal\";
            StaticDetails.AdminPortalLogFileSystemPath = StaticDetails.LogFileSystemPath + @"adminportal\";
#elif (STAGING)
            ///Media generic
            StaticDetails.MediaFileSystemPath = StaticDetails.SpecificFileSystemPath + @"media/";
            //video
            StaticDetails.VideoFileSystemPath = StaticDetails.MediaFileSystemPath + @"video/";
            StaticDetails.SplitVideoFileSystemPath = StaticDetails.VideoFileSystemPath + @"SplitVideos/";
            StaticDetails.RecordingsFileSystemPath = StaticDetails.VideoFileSystemPath + @"recordings/";
            //Images
            StaticDetails.ImageFileSystemPath = StaticDetails.MediaFileSystemPath + @"Images/";
            StaticDetails.LogoFileSystemPath = StaticDetails.ImageFileSystemPath + @"Logos/";
            StaticDetails.ProfessionalFileSystemPath = StaticDetails.ImageFileSystemPath + @"ProfessionalImages/";
            StaticDetails.ContentDeveloperLogoFileSystemPath = StaticDetails.ImageFileSystemPath + @"EduOrgLogos/";

            ///??? I think this is for the more generic setup
            StaticDetails.ProfessionalFileSystemPathForDb = @"ProfessionalImages/";
            StaticDetails.LogoFileSystemPathForDb = @"Logos/";
            StaticDetails.ContentDeveloperLogoFileSystemPathForDb = @"ContentDeveloperLogos/";

            ///software packages for download
            StaticDetails.LocalSoftwarePackage = StaticDetails.BaseFileSystemPath + @"LocalSoftwarePackage/";

            ///Market place media (seperated by listing)
            StaticDetails.MarketplaceListingPath = StaticDetails.BaseFileSystemPath + @"MarketplaceListings/";
            StaticDetails.MarketplaceListingScreenshotPath = @"Screenshot/";
            StaticDetails.MarketplaceListingVideoPath = @"Video/";

            ///Modules
            StaticDetails.ModuleFileSystemPath = StaticDetails.BaseFileSystemPath + @"modules/";

            ///Test User Statement Files
            StaticDetails.StatementFileSystemPath = StaticDetails.SpecificFileSystemPath + @"statements/";
            StaticDetails.VoidStatementFileSystemPath = StaticDetails.StatementFileSystemPath + @"voidstatements/";
            StaticDetails.JSONStatementFileSystemPath = StaticDetails.StatementFileSystemPath + @"JSONstatements/";

            ///Publication Path
            StaticDetails.PublicationPath = StaticDetails.BaseFileSystemPath + @"Publications/";
            StaticDetails.PublicationImagePath = @"Images/";
            StaticDetails.PublicationVideoPath = @"Videos/";

            ///Email Campaign Path
            StaticDetails.EmailCampaignPath = StaticDetails.BaseFileSystemPath + @"EmailCampaign/";
            StaticDetails.EmailCampaignImagePath = @"Images/";

            ///Logs, unsure if I should be useing specificFileSystemPath if the deployment is monolithic but I also would not hurt
            StaticDetails.LogFileSystemPath = StaticDetails.SpecificFileSystemPath + @"logs/";
            StaticDetails.APILogFileSystemPath = StaticDetails.LogFileSystemPath + @"api/";
            StaticDetails.PortalLogFileSystemPath = StaticDetails.LogFileSystemPath + @"portal/";



            //StaticDetails.MediaFileSystemPath = StaticDetails.SpecificFileSystemPath + @"media/";
            //StaticDetails.VideoFileSystemPath = StaticDetails.MediaFileSystemPath + @"video/";
            //StaticDetails.SplitVideoFileSystemPath = StaticDetails.VideoFileSystemPath + @"SplitVideos/";
            //StaticDetails.RecordingsFileSystemPath = StaticDetails.VideoFileSystemPath + @"recordings/";
            //StaticDetails.ImageFileSystemPath = StaticDetails.MediaFileSystemPath + @"Images/";
            //StaticDetails.LogoFileSystemPath = StaticDetails.ImageFileSystemPath + @"Logos/";
            //StaticDetails.ProfessionalFileSystemPath = StaticDetails.ImageFileSystemPath + @"ProfessionalImages/";
            //StaticDetails.EduOrgLogoFileSystemPath = StaticDetails.ImageFileSystemPath + @"EduOrgLogos/";
            //StaticDetails.ProfessionalFileSystemPathForDb = @"ProfessionalImages/";
            //StaticDetails.LogoFileSystemPathForDb = @"Logos/";
            //StaticDetails.EduOrgLogoFileSystemPathForDb = @"EduOrgLogos/";

            //StaticDetails.ModuleFileSystemPath = StaticDetails.BaseFileSystemPath + @"modules/";

            //StaticDetails.StatementFileSystemPath = StaticDetails.SpecificFileSystemPath + @"statements/";
            //StaticDetails.VoidStatementFileSystemPath = StaticDetails.StatementFileSystemPath + @"voidstatements/";
            //StaticDetails.JSONStatementFileSystemPath = StaticDetails.StatementFileSystemPath + @"JSONstatements/";


            //StaticDetails.LogFileSystemPath = StaticDetails.SpecificFileSystemPath + @"logs/";
            //StaticDetails.APILogFileSystemPath = StaticDetails.LogFileSystemPath + @"api/";
            //StaticDetails.PortalLogFileSystemPath = StaticDetails.LogFileSystemPath + @"portal/";
            //StaticDetails.AdminPortalLogFileSystemPath = StaticDetails.LogFileSystemPath + @"adminportal/";
#else
            ///Media generic
            StaticDetails.MediaFileSystemPath = StaticDetails.SpecificFileSystemPath + @"media/";
            //video
            StaticDetails.VideoFileSystemPath = StaticDetails.MediaFileSystemPath + @"video/";
            StaticDetails.SplitVideoFileSystemPath = StaticDetails.VideoFileSystemPath + @"SplitVideos/";
            StaticDetails.RecordingsFileSystemPath = StaticDetails.VideoFileSystemPath + @"recordings/";
            //Images
            StaticDetails.ImageFileSystemPath = StaticDetails.MediaFileSystemPath + @"Images/";
            StaticDetails.LogoFileSystemPath = StaticDetails.ImageFileSystemPath + @"Logos/";
            StaticDetails.ProfessionalFileSystemPath = StaticDetails.ImageFileSystemPath + @"ProfessionalImages/";
            StaticDetails.ContentDeveloperLogoFileSystemPath = StaticDetails.ImageFileSystemPath + @"EduOrgLogos/";

            ///??? I think this is for the more generic setup
            StaticDetails.ProfessionalFileSystemPathForDb = @"ProfessionalImages/";
            StaticDetails.LogoFileSystemPathForDb = @"Logos/";
            StaticDetails.ContentDeveloperLogoFileSystemPathForDb = @"ContentDeveloperLogos/";

            ///software packages for download
            StaticDetails.LocalSoftwarePackage = StaticDetails.BaseFileSystemPath + @"LocalSoftwarePackage/";

            ///Market place media (seperated by listing)
            StaticDetails.MarketplaceListingPath = StaticDetails.BaseFileSystemPath + @"MarketplaceListings/";
            StaticDetails.MarketplaceListingScreenshotPath = @"Screenshot/";
            StaticDetails.MarketplaceListingVideoPath = @"Video/";

            ///Modules
            StaticDetails.ModuleFileSystemPath = StaticDetails.BaseFileSystemPath + @"modules/";

            ///Test User Statement Files
            StaticDetails.StatementFileSystemPath = StaticDetails.SpecificFileSystemPath + @"statements/";
            StaticDetails.VoidStatementFileSystemPath = StaticDetails.StatementFileSystemPath + @"voidstatements/";
            StaticDetails.JSONStatementFileSystemPath = StaticDetails.StatementFileSystemPath + @"JSONstatements/";

            ///Publication Path
            StaticDetails.PublicationPath = StaticDetails.BaseFileSystemPath + @"Publications/";
            StaticDetails.PublicationImagePath = @"Images/";
            StaticDetails.PublicationVideoPath = @"Videos/";

            ///Email Campaign Path
            StaticDetails.EmailCampaignPath = StaticDetails.BaseFileSystemPath + @"EmailCampaign/";
            StaticDetails.EmailCampaignImagePath = @"Images/";

            ///Logs, unsure if I should be useing specificFileSystemPath if the deployment is monolithic but I also would not hurt
            StaticDetails.LogFileSystemPath = StaticDetails.SpecificFileSystemPath + @"logs/";
            StaticDetails.APILogFileSystemPath = StaticDetails.LogFileSystemPath + @"api/";
            StaticDetails.PortalLogFileSystemPath = StaticDetails.LogFileSystemPath + @"portal/";
            StaticDetails.AdminPortalLogFileSystemPath = StaticDetails.LogFileSystemPath + @"adminportal/";

#endif

            try
            {
                // Common areas every host that stores files needs (media, modules, statements, and the
                // host's own api/portal logs). Directory.CreateDirectory is mkdir -p, so the order here
                // does not matter for correctness.
                List<string> FileList = new List<string>
                {
                    StaticDetails.BaseFileSystemPath,
                    StaticDetails.SpecificFileSystemPath,
                    StaticDetails.MediaFileSystemPath,
                    StaticDetails.VideoFileSystemPath,
                    StaticDetails.SplitVideoFileSystemPath,
                    StaticDetails.RecordingsFileSystemPath,
                    StaticDetails.ImageFileSystemPath,
                    StaticDetails.LogoFileSystemPath,
                    StaticDetails.ProfessionalFileSystemPath,
                    StaticDetails.ModuleFileSystemPath,
                    StaticDetails.StatementFileSystemPath,
                    StaticDetails.VoidStatementFileSystemPath,
                    StaticDetails.JSONStatementFileSystemPath,
                    StaticDetails.LogFileSystemPath,
                    StaticDetails.PortalLogFileSystemPath,
                    StaticDetails.APILogFileSystemPath,
                };

                // Central-only areas the EndUser platform does not use locally: content-developer logos,
                // the marketplace listing tree (the EndUser serves marketplace images via the remote core
                // API, not a local path -- see WidgetController.MarketplaceListingImageLoader), downloadable
                // software packages, the admin-portal log dir, and -- since the ROADMAP 17 reachability
                // sweep deleted the node's BadgeLoader / PublicationImageLoader /
                // CampaignEmailMessageImageLoader, the only node code that ever served them -- badges,
                // publications, and email-campaign assets. A host-scoped EndUser deployment must NOT
                // create these -- it only ever owns its own tenant storage (auth-island / uncontrolled-
                // deployment boundary). This mirrors StorageManifests.EndUser, the canonical area
                // declaration.
                if (role == FileServerHostRole.Central)
                {
                    FileList.AddRange(new[]
                    {
                        StaticDetails.ContentDeveloperLogoFileSystemPath,
                        StaticDetails.MarketplaceListingPath,
                        StaticDetails.MarketplaceListingScreenshotPath,
                        StaticDetails.MarketplaceListingVideoPath,
                        StaticDetails.LocalSoftwarePackage,
                        StaticDetails.AdminPortalLogFileSystemPath,
                        StaticDetails.PublicationPath,
                        StaticDetails.PublicationImagePath,
                        StaticDetails.PublicationVideoPath,
                        StaticDetails.EmailCampaignPath,
                        StaticDetails.EmailCampaignImagePath,
                    });
                }

                foreach (var file in FileList)
                {
                    try
                    {
                        CreateFileDirectory(config, file, string.Empty);
                        //CreateFileDirectory(file, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        Febris.SharedServices.FebrisLog.Error(ex);
                    }
                }

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                //throw;
            }
        }
        #endregion

        #region Create
        /// <summary>
        /// Create File
        /// </summary>
        /// <param name="path"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<FileStream> CreationFileStream(string path, string input)
        {
            try
            {
                //SmbSettings settings = smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path + input), "Basic", credentials);
                FileStream stream = File.Create(path + input);

                return stream;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                throw;
            }
        }

        /// <summary>
        /// For creating Statement Files
        /// </summary>
        /// <param name="path"></param>
        /// <param name="StatementUUID"></param>
        /// <param name="input"></param>
        public async Task CreationFileStream(string path, string StatementUUID, object input)
        {
            try
            {
                SmbSettings settings = smbSettings();

                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                ////NetworkCredential credentials = new NetworkCredential(smbSettings.UserName, smbSettings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path + input), "Basic", credentials);
                //string fullPath = Path.Combine(path, StatementUUID + ".json");
                string fullPath = path + "/" + StatementUUID + ".json";
                using (FileStream stream = File.Create(fullPath))
                {
                    byte[] statment = new UTF8Encoding(true).GetBytes(input.ToString());
                    stream.Write(statment, 0, statment.Length);
                }
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                // throw;
            }
        }

        /// <summary>
        /// Create File directory
        /// </summary>
        /// <param name="config"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public async void CreateFileDirectory(IConfiguration config, string path, string name)
        {
            try
            {
                //SmbSettings settings = smbSettings(config);
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
                Directory.CreateDirectory(path + name);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                //throw;
            }
        }

        /// <summary>
        /// Create File Directory
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public async void CreateFileDirectory(string path, string name)
        {
            try
            {
                //SmbSettings settings = smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
                Directory.CreateDirectory(path + name);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                //throw;
            }
        }

        #endregion

        #region Get
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileStream OutgoingFileStream(string path)
        {
            try
            {
                //SmbSettings settings = smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
                FileStream stream = new FileStream(path, FileMode.Open);//(path + input);

                return stream;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Get File
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<object> FileRetrieval(string path, string name)
        {
            try
            {
                bool success = false;
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                File.GetAttributes(path + name);

                return success;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                throw;
            }
            return false;
        }

        /// <summary>
        /// Get File List
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public string[] GetDirectoryFileList(string path, string searchPattern)
        {
            try
            {
                string[] fileList = { };
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                var fullFileList = Directory.GetFiles(path, searchPattern)
                    .Select(x => new FileInfo(x))
                    .OrderByDescending(x => x.LastWriteTime)
                    .ToArray();

                fileList = fullFileList.Select(o => o.Name).ToArray();

                return fileList;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                throw;
            }
        }

        /// <summary>
        /// Get Full list of file names
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFullDirectoryFileList(string path)
        {
            try
            {
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                //fileList = Directory.GetFiles(path, searchPattern);
                IEnumerable<string> fileList = Directory.EnumerateFiles(path);

                return fileList;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Moves Files
        /// </summary>
        /// <param name="currentPath"></param>
        /// <param name="newPath"></param>
        public async void FileMover(string currentPath, string newPath)
        {
            try
            {
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(currentPath), "Basic", credentials);
                credentialCache.Add(new Uri(newPath), "Basic", credentials);
                File.Move(currentPath, newPath);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //cannot move file to an area with a file that already has that name. 
                string name = Path.GetFileName(currentPath);
                string path = Path.GetDirectoryName(currentPath);
                FileDelete(path, name);
                //throw;
            }
        }

        #endregion

        #region Delete
        /// <summary>
        /// Generic File Deletion
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        // FIX (SCBA-B8): body is fully synchronous (only File.Delete, no await), so async void let exceptions escape to the SynchronizationContext. Make it plain void.
        // public async void FileDelete(string path, string name)
        public void FileDelete(string path, string name)
        {
            try
            {
                //SmbSettings settings = smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
#if (DEBUG)
                File.Delete(path + name);
#elif (STAGING)
                File.Delete(path + name);
#else
                File.Delete(path + name);
#endif
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                throw;
            }
        }





        /// <summary>
        /// Deletes split Files from chucked uploads
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        // FIX (SCBA-B8): body is fully synchronous (only File.Delete, no await), so async void let exceptions escape to the SynchronizationContext. Make it plain void.
        // public async void DeleteSplitFiles(string path, string name)
        public void DeleteSplitFiles(string path, string name)
        {
            try
            {
                //SmbSettings settings = smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
                File.Delete(path + name);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        /// <summary>
        ///connecting to the file server to store - Does not seem used ********* 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<bool> FileStorage(string path, object input)
        {
            try
            {
                bool success = false;

                SmbSettings settings = smbSettings();

                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                File.Create(path + input);

                return success;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                throw;
            }
            return false;
        }


        /// <summary>
        /// Check if this file exists
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> FileExists(string path, string name)
        {
            try
            {
                bool success = false;
                //SmbSettings settings = smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
                success = File.Exists(path + name);

                return success;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);

                throw;
            }
            return false;
        }

        #endregion

        #region Media

        #region Images
        /// <summary>
        /// Get images
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<byte[]> GetImage(string path)
        {
            try
            {
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                byte[] image = { };
                try
                {
                    image = File.ReadAllBytes(path);
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                return image;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                return null;
            }
        }


        /// <summary>
        /// Add images
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<bool> AddImage(string path)
        {
            bool output = false;
            try
            {
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                byte[] image = { };
                try
                {
                    image = File.ReadAllBytes(path);
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                //return image;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return output;
        }

        #endregion

        #region Video
        /// <summary>
        /// Get video file stream
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<FileStream> GetVideo(string path)
        {
            try
            {
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                FileStream stream = new FileStream(path, FileMode.Open);

                return stream;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                return null;
            }
        }


        /// <summary>
        /// Add video file stream
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<bool> AddVideo(string path)
        {
            bool output = false;
            try
            {
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                FileStream stream = new FileStream(path, FileMode.Open);

                //return stream;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return output;
        }

        #endregion

        /// <summary>
        /// detection of file sizes
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<long> GetFileLength(string path)
        {
            try
            {
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                FileInfo info = new FileInfo(path);
                long fileSize = 0;
                fileSize = info.Length;
                return fileSize;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                return 0;
            }
        }


        #endregion

        #region json statement  

        /// <summary>
        /// Gathering JOnject for voiding and what not
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<JObject> JsonFileRetrieval(string path, string name)
        {
            try
            {
                JObject jobj = new JObject();
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(path), "Basic", credentials);
                //string file = File.ReadAllText(Path.Combine(path,name));
#if (DEBUG)
                string file = File.ReadAllText(Path.Combine(path, name));
#elif (STAGING)
                string file = File.ReadAllText(path + name);
#else
                string file = File.ReadAllText(path + name);
#endif

                jobj = JsonConvert.DeserializeObject<JObject>(file);

                return jobj;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// connecting to the file storage retriever
        /// I think this is used for file downloading
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<FileStream> StatementFileRetrieval(string path, string name)
        {
            try
            {
#if (DEBUG)
                string fullPath = Path.Combine(path, name);
#elif (STAGING)
                string fullPath = path + "/" + name;
#else
                string fullPath = path + "/" + name;
#endif
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(fullPath), "Basic", credentials);
                FileStream stream = new FileStream(fullPath, FileMode.Open);

                return stream;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task<bool> IsFileInUser(string input)
        {
            try
            {
                bool isInUse = true;
                SmbSettings settings = smbSettings();
                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                CredentialCache credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(input), "Basic", credentials);
                isInUse = MergeFileManager.Instance.InUse(input);
                return isInUse;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task<FileStream> MergeFileStream(string path, FileMode mode)
        {
            try
            {
                //SmbSettings settings = smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
                FileStream stream = File.Open(path, mode);//new FileStream(path, mode);

                return stream;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task AddFileToMerge(string path)
        {
            try
            {
                //SmbSettings settings = smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
                MergeFileManager.Instance.AddFile(path);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        #endregion

        #region unused
        //#############################################################################
        // FileStream
        //#############################################################################
        //public FileStream OutgoingFileStream(string path)
        //{
        //    try
        //    {
        //        NetworkCredential credentials = new NetworkCredential(smbSettings.UserName, smbSettings.Secret);
        //        CredentialCache credentialCache = new CredentialCache();
        //        credentialCache.Add(new Uri(path), "Basic", credentials);
        //        FileStream stream = new FileStream(path, FileMode.Open);//(path + input);

        //        return stream;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //#############################################################################
        // FileStream
        //#############################################################################
        //public FileStream MergeFileStream(string path, FileMode mode)
        //{
        //    try
        //    {
        //        //SmbSettings smbSettings = new SmbSettings()
        //        //{
        //        //    Secret = _config["SmbClient:Secret"],
        //        //    UserName = _config["SmbClient:UserName"]
        //        //};
        //        //var smbInfo = _config.Value;
        //        //var smbSettings = smbInfo;/*.Get<SmbSettings>();*/
        //        //string newPath = path + input;

        //        NetworkCredential credentials = new NetworkCredential(smbSettings.UserName, smbSettings.Secret);
        //        CredentialCache credentialCache = new CredentialCache();
        //        credentialCache.Add(new Uri(path), "Basic", credentials);
        //        FileStream stream = File.Open(path, mode);//new FileStream(path, mode);

        //        return stream;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //#############################################################################
        // FileStream
        //#############################################################################
        #endregion
    }
    #endregion

    #region image
    public interface IImageFileHandler
    {
        Task<(bool output, MarketplaceListing input)> AddImage(IFormFile formFile, MarketplaceListing marketplaceListing);
        Task<(bool uploaded, UserSettingsViewModel output)> AddImage(IFormFile formFile, UserSettingsViewModel input);
        Task<(bool output, Publication input)> AddImage(IFormFile file, Publication input);

        Task<(bool output, string newImageName)> AddImage(IFormFile formFile, EmailCampaignMessage input);
        Task<(bool uploaded, LocalUserSettingsViewModel output)> AddImage(IFormFile file, LocalUserSettingsViewModel input);
        Task<(bool uploaded, LocalApplicationUser output)> AddImage(IFormFile file, LocalApplicationUser input);

        /// <summary>
        /// Deletes a user's profile photograph from disk. Returns true when a file was removed.
        /// Safe to call when there is no photograph.
        /// </summary>
        Task<bool> DeleteProfileImage(Guid userId, string profilePicturePath);
    }
    public class ImageFileHandler : IImageFileHandler
    {
        FileServerHandler _fileServerHandler;
        public ImageFileHandler()
        {
            _fileServerHandler = new FileServerHandler();
        }

        #region Images
        /// <summary>
        /// Get images
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<byte[]> GetImage(string path)
        {
            try
            {
                //SmbSettings settings = _fileServerHandler.smbSettings();
                //NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(path), "Basic", credentials);
                byte[] image = { };
                try
                {
                    image = File.ReadAllBytes(path);
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                return image;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                return null;
            }
        }


        /// <summary>
        /// Add images
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<(bool output, MarketplaceListing input)> AddImage(IFormFile file, MarketplaceListing input)
        {
            bool output = false;
            try
            {
                //create File 
                string basePath = Path.Combine(StaticDetails.MarketplaceListingPath, input.UUID.ToString());
                string imagePath = Path.Combine(basePath, StaticDetails.MarketplaceListingScreenshotPath);
                string cutdownPath = Path.Combine(input.UUID.ToString(), StaticDetails.MarketplaceListingScreenshotPath);
                try
                {
                    if (!await _fileServerHandler.FileExists(StaticDetails.MarketplaceListingPath, input.UUID.ToString()))
                    { _fileServerHandler.CreateFileDirectory(StaticDetails.MarketplaceListingPath, input.UUID.ToString()); }
                    if (!await _fileServerHandler.FileExists(imagePath, StaticDetails.MarketplaceListingScreenshotPath))
                    { _fileServerHandler.CreateFileDirectory(imagePath, StaticDetails.MarketplaceListingScreenshotPath); }
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                var something = file.Name;
                //var something2 = file.FileName;

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".jpg", ".png" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, input);
                }
                var contentCheck = FileUploadValidator.ValidateImage(file);
                if (!contentCheck.IsValid) { Febris.SharedServices.FebrisLog.Warn("Rejected image upload: " + contentCheck.Reason); return (output, input); }
                using (var scanStream = file.OpenReadStream())
                {
                    var scan = await Febris.SharedServices.FileScanService.ScanAsync(scanStream, file.FileName);
                    if (scan.Scanned && !scan.IsClean) { Febris.SharedServices.FebrisLog.Warn("Rejected upload flagged by malware scan: " + (scan.Threat ?? "unknown")); return (output, input); }
                }


                string imageRename = Guid.NewGuid().ToString() + extension;

                switch (something)
                {
                    case "ScreenShot1":
                        input.ScreenShot1 = Path.Combine(cutdownPath, imageRename);
                        break;
                    case "ScreenShot2":
                        input.ScreenShot2 = Path.Combine(cutdownPath, imageRename);
                        break;
                    case "ScreenShot3":
                        input.ScreenShot3 = Path.Combine(cutdownPath, imageRename);
                        break;
                    case "ScreenShot4":
                        input.ScreenShot4 = Path.Combine(cutdownPath, imageRename);
                        break;
                    case "ScreenShot5":
                        input.ScreenShot5 = Path.Combine(cutdownPath, imageRename);
                        break;
                    default:
                        return (output, input);
                        //case "ScreenShot1":
                        //    input.ScreenShot1 = Path.Combine(imagePath, imageRename);
                        //    break;
                        //case "ScreenShot2":
                        //    input.ScreenShot2 = Path.Combine(imagePath, imageRename);
                        //    break;
                        //case "ScreenShot3":
                        //    input.ScreenShot3 = Path.Combine(imagePath, imageRename);
                        //    break;
                        //case "ScreenShot4":
                        //    input.ScreenShot4 = Path.Combine(imagePath, imageRename);
                        //    break;
                        //case "ScreenShot5":
                        //    input.ScreenShot5 = Path.Combine(imagePath, imageRename);
                        //    break;
                        //default:
                        //    return (output, input);
                }

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(imagePath, imageRename))
                {
                    file.CopyTo(filestream);
                }
                output = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, input);
        }

        public async Task<(bool uploaded, UserSettingsViewModel output)> AddImage(IFormFile file, UserSettingsViewModel input)
        {
            bool output = false;
            try
            {
                //create File 
                //string basePath = Path.Combine(StaticDetails.ProfessionalFileSystemPath, input.Id.ToString());
                //string imagePath = Path.Combine(basePath, StaticDetails.MarketplaceListingScreenshotPath);
                //string cutdownPath = Path.Combine(input.UUID.ToString(), StaticDetails.MarketplaceListingScreenshotPath);
                try
                {
                    //if (!await _fileServerHandler.FileExists(StaticDetails.MarketplaceListingPath, input.Id.ToString()))
                    //{ _fileServerHandler.CreateFileDirectory(StaticDetails.MarketplaceListingPath, input.UUID.ToString()); }
                    //if (!await _fileServerHandler.FileExists(imagePath, StaticDetails.MarketplaceListingScreenshotPath))
                    //{ _fileServerHandler.CreateFileDirectory(imagePath, StaticDetails.MarketplaceListingScreenshotPath); }
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                string something = file.Name;
                if (something != "ProfilePicture")
                {
                    return (output, input);
                }

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".jpg", ".png" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, input);
                }
                var contentCheck = FileUploadValidator.ValidateImage(file);
                if (!contentCheck.IsValid) { Febris.SharedServices.FebrisLog.Warn("Rejected image upload: " + contentCheck.Reason); return (output, input); }
                using (var scanStream = file.OpenReadStream())
                {
                    var scan = await Febris.SharedServices.FileScanService.ScanAsync(scanStream, file.FileName);
                    if (scan.Scanned && !scan.IsClean) { Febris.SharedServices.FebrisLog.Warn("Rejected upload flagged by malware scan: " + (scan.Threat ?? "unknown")); return (output, input); }
                }



                //string imageRename = Guid.NewGuid().ToString() + extension;
                string imageRename = input.Id.ToString() + extension;
                //switch (something)
                //{
                //    case "ScreenShot1":
                //        input.ScreenShot1 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot2":
                //        input.ScreenShot2 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot3":
                //        input.ScreenShot3 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot4":
                //        input.ScreenShot4 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot5":
                //        input.ScreenShot5 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    default:
                //        return (output, input);

                //}

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(StaticDetails.ProfessionalFileSystemPath, imageRename))
                {
                    file.CopyTo(filestream);
                }
                input.ProfilePicturePath = imageRename;
                output = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, input);
        }

        public async Task<(bool uploaded, LocalUserSettingsViewModel output)> AddImage(IFormFile file, LocalUserSettingsViewModel input)
        {
            bool output = false;
            try
            {
                //create File 
                //string basePath = Path.Combine(StaticDetails.ProfessionalFileSystemPath, input.Id.ToString());
                //string imagePath = Path.Combine(basePath, StaticDetails.MarketplaceListingScreenshotPath);
                //string cutdownPath = Path.Combine(input.UUID.ToString(), StaticDetails.MarketplaceListingScreenshotPath);
                try
                {
                    //if (!await _fileServerHandler.FileExists(StaticDetails.MarketplaceListingPath, input.Id.ToString()))
                    //{ _fileServerHandler.CreateFileDirectory(StaticDetails.MarketplaceListingPath, input.UUID.ToString()); }
                    //if (!await _fileServerHandler.FileExists(imagePath, StaticDetails.MarketplaceListingScreenshotPath))
                    //{ _fileServerHandler.CreateFileDirectory(imagePath, StaticDetails.MarketplaceListingScreenshotPath); }
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                string something = file.Name;
                if (something != "ProfilePicture")
                {
                    return (output, input);
                }

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".jpg", ".png" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, input);
                }
                var contentCheck = FileUploadValidator.ValidateImage(file);
                if (!contentCheck.IsValid) { Febris.SharedServices.FebrisLog.Warn("Rejected image upload: " + contentCheck.Reason); return (output, input); }
                using (var scanStream = file.OpenReadStream())
                {
                    var scan = await Febris.SharedServices.FileScanService.ScanAsync(scanStream, file.FileName);
                    if (scan.Scanned && !scan.IsClean) { Febris.SharedServices.FebrisLog.Warn("Rejected upload flagged by malware scan: " + (scan.Threat ?? "unknown")); return (output, input); }
                }



                //string imageRename = Guid.NewGuid().ToString() + extension;
                string imageRename = input.Id.ToString() + extension;
                //switch (something)
                //{
                //    case "ScreenShot1":
                //        input.ScreenShot1 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot2":
                //        input.ScreenShot2 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot3":
                //        input.ScreenShot3 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot4":
                //        input.ScreenShot4 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot5":
                //        input.ScreenShot5 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    default:
                //        return (output, input);

                //}

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(StaticDetails.ProfessionalFileSystemPath, imageRename))
                {
                    file.CopyTo(filestream);
                }
                input.ProfilePicturePath = imageRename;
                output = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, input);
        }

        /// <summary>
        /// Removes a user's profile photograph.
        ///
        /// <para>
        /// Deleting an account left the photograph behind in BOTH branches, soft and hard, and no
        /// image delete existed anywhere in the repository. Any learner can upload one through
        /// <c>UserController.SelfEdit</c>, it is stored under their own user id, and it is served to
        /// browsers, so an account that had been "deleted" still had a photograph of its owner on
        /// disk indefinitely. That is an erasure gap rather than a retention one, which is why it is
        /// fixed here rather than given a timer.
        /// </para>
        ///
        /// <para>
        /// <b>It deletes through the same path constant the write uses, deliberately.</b> The newer
        /// <c>IStorageProvider</c> seam has a <c>DeleteAsync</c> and it would have been the tidier
        /// call, but its Professional area resolves to <c>{Storage:BasePath}/media/images/professional</c>
        /// while <see cref="AddImage(IFormFile, LocalApplicationUser)"/> writes to
        /// <c>StaticDetails.ProfessionalFileSystemPath</c>, which on a deployed node is
        /// <c>{SmbClient:Path}{UniqueFileSystemPath}media/Images/ProfessionalImages</c>. Those are
        /// different directories, differing by a path segment AND by casing, so deleting through the
        /// provider would have removed nothing at all while reporting success.
        /// </para>
        ///
        /// <para>
        /// <c>ProfilePicturePath</c> is the authoritative name, but a stale or empty value must not
        /// leave the photograph behind, so anything matching the user's id is swept as well. The
        /// file is named after the user id, so that pattern cannot match another person's photo.
        /// </para>
        /// </summary>
        public async Task<bool> DeleteProfileImage(Guid userId, string profilePicturePath)
        {
            await Task.CompletedTask;
            bool deletedAny = false;
            try
            {
                string dir = StaticDetails.ProfessionalFileSystemPath;
                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(profilePicturePath))
                {
                    // Name only. A stored value carrying separators is not trusted to build a path.
                    string named = Path.Combine(dir, Path.GetFileName(profilePicturePath));
                    if (File.Exists(named))
                    {
                        File.Delete(named);
                        deletedAny = true;
                    }
                }

                if (userId != Guid.Empty)
                {
                    foreach (string path in Directory.GetFiles(dir, userId.ToString() + ".*"))
                    {
                        try
                        {
                            File.Delete(path);
                            deletedAny = true;
                        }
                        catch (Exception inner)
                        {
                            Febris.SharedServices.FebrisLog.Error(inner,
                                "ImageFileHandler.DeleteProfileImage: could not delete '" + path + "'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Never fail an account deletion because its photograph could not be removed. The
                // caller logs and continues, matching how cohort membership cleanup behaves.
                Febris.SharedServices.FebrisLog.Error(ex, "ImageFileHandler.DeleteProfileImage");
            }
            return deletedAny;
        }

        public async Task<(bool uploaded, LocalApplicationUser output)> AddImage(IFormFile file, LocalApplicationUser input)
        {
            bool output = false;
            try
            {
                //create File 
                //string basePath = Path.Combine(StaticDetails.ProfessionalFileSystemPath, input.Id.ToString());
                //string imagePath = Path.Combine(basePath, StaticDetails.MarketplaceListingScreenshotPath);
                //string cutdownPath = Path.Combine(input.UUID.ToString(), StaticDetails.MarketplaceListingScreenshotPath);
                try
                {
                    //if (!await _fileServerHandler.FileExists(StaticDetails.MarketplaceListingPath, input.Id.ToString()))
                    //{ _fileServerHandler.CreateFileDirectory(StaticDetails.MarketplaceListingPath, input.UUID.ToString()); }
                    //if (!await _fileServerHandler.FileExists(imagePath, StaticDetails.MarketplaceListingScreenshotPath))
                    //{ _fileServerHandler.CreateFileDirectory(imagePath, StaticDetails.MarketplaceListingScreenshotPath); }
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                string something = file.Name;
                if (something != "ProfilePicture")
                {
                    return (output, input);
                }

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".jpg", ".png" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, input);
                }
                var contentCheck = FileUploadValidator.ValidateImage(file);
                if (!contentCheck.IsValid) { Febris.SharedServices.FebrisLog.Warn("Rejected image upload: " + contentCheck.Reason); return (output, input); }
                using (var scanStream = file.OpenReadStream())
                {
                    var scan = await Febris.SharedServices.FileScanService.ScanAsync(scanStream, file.FileName);
                    if (scan.Scanned && !scan.IsClean) { Febris.SharedServices.FebrisLog.Warn("Rejected upload flagged by malware scan: " + (scan.Threat ?? "unknown")); return (output, input); }
                }



                //string imageRename = Guid.NewGuid().ToString() + extension;
                string imageRename = input.Id.ToString() + extension;
                //switch (something)
                //{
                //    case "ScreenShot1":
                //        input.ScreenShot1 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot2":
                //        input.ScreenShot2 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot3":
                //        input.ScreenShot3 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot4":
                //        input.ScreenShot4 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot5":
                //        input.ScreenShot5 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    default:
                //        return (output, input);

                //}

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(StaticDetails.ProfessionalFileSystemPath, imageRename))
                {
                    file.CopyTo(filestream);
                }
                input.ProfilePicturePath = imageRename;
                output = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, input);
        }


        public async Task<(bool output, Publication input)> AddImage(IFormFile file, Publication input)
        {
            bool output = false;
            try
            {
                //create File 
                string basePath = Path.Combine(StaticDetails.PublicationPath, input.UUID.ToString());
                string imagePath = Path.Combine(basePath, StaticDetails.PublicationImagePath);
                string cutdownPath = Path.Combine(input.UUID.ToString(), StaticDetails.PublicationImagePath);
                try
                {
                    if (!await _fileServerHandler.FileExists(StaticDetails.PublicationPath, input.UUID.ToString()))
                    { _fileServerHandler.CreateFileDirectory(StaticDetails.PublicationPath, input.UUID.ToString()); }
                    if (!await _fileServerHandler.FileExists(imagePath, StaticDetails.PublicationImagePath))
                    { _fileServerHandler.CreateFileDirectory(imagePath, StaticDetails.PublicationImagePath); }
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                var something = file.Name;

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".jpg", ".png" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, input);
                }
                var contentCheck = FileUploadValidator.ValidateImage(file);
                if (!contentCheck.IsValid) { Febris.SharedServices.FebrisLog.Warn("Rejected image upload: " + contentCheck.Reason); return (output, input); }
                using (var scanStream = file.OpenReadStream())
                {
                    var scan = await Febris.SharedServices.FileScanService.ScanAsync(scanStream, file.FileName);
                    if (scan.Scanned && !scan.IsClean) { Febris.SharedServices.FebrisLog.Warn("Rejected upload flagged by malware scan: " + (scan.Threat ?? "unknown")); return (output, input); }
                }


                string imageRename = Guid.NewGuid().ToString() + extension;

                switch (something)
                {
                    case "Image0":
                        input.Image0 = Path.Combine(cutdownPath, imageRename);
                        break;
                    case "Image1":
                        input.Image1 = Path.Combine(cutdownPath, imageRename);
                        break;
                    case "Image2":
                        input.Image2 = Path.Combine(cutdownPath, imageRename);
                        break;
                    case "Image3":
                        input.Image3 = Path.Combine(cutdownPath, imageRename);
                        break;
                    case "Image4":
                        input.Image4 = Path.Combine(cutdownPath, imageRename);
                        break;
                    default:
                        return (output, input);
                }

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(imagePath, imageRename))
                {
                    file.CopyTo(filestream);
                }
                output = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, input);
        }

        public async Task<(bool output, string newImageName)> AddImage(IFormFile file, EmailCampaignMessage input)
        {
            bool output = false;
            string imageRename = string.Empty;
            try
            {
                //create File 
                string basePath = Path.Combine(StaticDetails.EmailCampaignPath, input.UUID.ToString());
                string imagePath = Path.Combine(basePath, StaticDetails.EmailCampaignImagePath);
                string cutdownPath = Path.Combine(input.UUID.ToString(), StaticDetails.EmailCampaignImagePath);
                try
                {
                    if (!await _fileServerHandler.FileExists(StaticDetails.EmailCampaignPath, input.UUID.ToString()))
                    { _fileServerHandler.CreateFileDirectory(StaticDetails.EmailCampaignPath, input.UUID.ToString()); }
                    if (!await _fileServerHandler.FileExists(imagePath, StaticDetails.EmailCampaignImagePath))
                    { _fileServerHandler.CreateFileDirectory(imagePath, StaticDetails.EmailCampaignImagePath); }
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                //get the file name
                var something = file.Name;


                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".jpg", ".png" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, imageRename);
                }
                var contentCheck = FileUploadValidator.ValidateImage(file);
                if (!contentCheck.IsValid) { Febris.SharedServices.FebrisLog.Warn("Rejected image upload: " + contentCheck.Reason); return (output, imageRename); }
                using (var scanStream = file.OpenReadStream())
                {
                    var scan = await Febris.SharedServices.FileScanService.ScanAsync(scanStream, file.FileName);
                    if (scan.Scanned && !scan.IsClean) { Febris.SharedServices.FebrisLog.Warn("Rejected upload flagged by malware scan: " + (scan.Threat ?? "unknown")); return (output, imageRename); }
                }


                imageRename = Guid.NewGuid().ToString() + extension;


                //switch (something)
                //{
                //    case "ScreenShot1":
                //        input.ScreenShot1 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot2":
                //        input.ScreenShot2 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot3":
                //        input.ScreenShot3 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot4":
                //        input.ScreenShot4 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    case "ScreenShot5":
                //        input.ScreenShot5 = Path.Combine(cutdownPath, imageRename);
                //        break;
                //    default:
                //        return (output, input);
                //        //case "ScreenShot1":
                //        //    input.ScreenShot1 = Path.Combine(imagePath, imageRename);
                //        //    break;
                //        //case "ScreenShot2":
                //        //    input.ScreenShot2 = Path.Combine(imagePath, imageRename);
                //        //    break;
                //        //case "ScreenShot3":
                //        //    input.ScreenShot3 = Path.Combine(imagePath, imageRename);
                //        //    break;
                //        //case "ScreenShot4":
                //        //    input.ScreenShot4 = Path.Combine(imagePath, imageRename);
                //        //    break;
                //        //case "ScreenShot5":
                //        //    input.ScreenShot5 = Path.Combine(imagePath, imageRename);
                //        //    break;
                //        //default:
                //        //    return (output, input);
                //}

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(imagePath, imageRename))
                {
                    file.CopyTo(filestream);
                }
                imageRename = Path.Combine(cutdownPath, imageRename);
                output = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, imageRename);
        }
        #endregion

    }
    #endregion

    #region Video
    public interface IVideoFileHandler
    {
        Task AddFileToMerge(string v);
        Task<(bool output, MarketplaceListing input)> AddVideo(IFormFile formFile, MarketplaceListing marketplaceListing);
        Task<(bool output, Publication input)> AddVideo(IFormFile formFile, Publication input);
        Task CreateFileDirectory(string uploadPath, string empty);
        Task<FileStream> CreationFileStream(string uploadPath, string fileName);
        Task DeleteSplitFiles(string splitVideoFileSystemPath, string fileName);
        Task FileDelete(string uploadPath, string fileName);
        Task<bool> FileExists(string uploadPath, string fileName);
        Task FileMover(string currentPath, string newPath);
        Task<string[]> GetDirectoryFileList(string path, string searchpattern);
        Task<bool> IsFileInUse(string v);
        Task<FileStream> MergeFileStream(string fileName, FileMode open);
    }
    public class VideoFileHandler : IVideoFileHandler
    {
        IFileServerHandler _fileServerHandler;
        public VideoFileHandler()
        {
            _fileServerHandler = new FileServerHandler();
        }


        #region Video
        /// <summary>
        /// Get video file stream
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        //public async Task<FileStream> GetVideo(string path)
        //{
        //    try
        //    {
        //        SmbSettings settings = smbSettings();
        //        NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
        //        CredentialCache credentialCache = new CredentialCache();
        //        credentialCache.Add(new Uri(path), "Basic", credentials);
        //        FileStream stream = new FileStream(path, FileMode.Open);

        //        return stream;
        //    }
        //    catch (Exception ex)
        //    {
        //        Febris.SharedServices.FebrisLog.Error(ex);
        //        return null;
        //    }
        //}


        /// <summary>
        /// Add video file stream
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<(bool output, MarketplaceListing input)> AddVideo(IFormFile file, MarketplaceListing input)
        {
            bool output = false;
            try
            {
                //create File 
                string basePath = Path.Combine(StaticDetails.MarketplaceListingPath, input.UUID.ToString());
                string videoPath = Path.Combine(basePath, StaticDetails.MarketplaceListingVideoPath);
                try
                {
                    if (!await _fileServerHandler.FileExists(StaticDetails.MarketplaceListingPath, input.UUID.ToString()))
                    { _fileServerHandler.CreateFileDirectory(StaticDetails.MarketplaceListingPath, input.UUID.ToString()); }
                    if (!await _fileServerHandler.FileExists(videoPath, StaticDetails.MarketplaceListingVideoPath))
                    { _fileServerHandler.CreateFileDirectory(videoPath, StaticDetails.MarketplaceListingVideoPath); }
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }

                var something = file.Name;
                var something2 = file.FileName;

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".mp4" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, input);
                }
                string rename = Guid.NewGuid().ToString();
                input.VideoName = Path.Combine(videoPath, rename);

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(videoPath, rename + extension))
                {
                    file.CopyTo(filestream);
                }
                output = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, input);
        }

        public async Task<(bool output, Publication input)> AddVideo(IFormFile file, Publication input)
        {
            bool output = false;
            try
            {
                //create File 
                string basePath = Path.Combine(StaticDetails.PublicationPath, input.UUID.ToString());
                string videoPath = Path.Combine(basePath, StaticDetails.PublicationVideoPath);
                string cutdownPath = Path.Combine(input.UUID.ToString(), StaticDetails.PublicationVideoPath);
                try
                {
                    if (!await _fileServerHandler.FileExists(StaticDetails.PublicationPath, input.UUID.ToString()))
                    { _fileServerHandler.CreateFileDirectory(StaticDetails.PublicationPath, input.UUID.ToString()); }
                    if (!await _fileServerHandler.FileExists(videoPath, StaticDetails.PublicationVideoPath))
                    { _fileServerHandler.CreateFileDirectory(videoPath, StaticDetails.PublicationVideoPath); }
                }
                catch (Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex);
                }


                var something = file.Name;

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".mp4" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, input);
                }

                string rename = Guid.NewGuid().ToString() + extension;

                switch (something)
                {
                    case "Video0":
                        input.Video0 = Path.Combine(cutdownPath, rename);
                        break;
                    case "Video1":
                        input.Video1 = Path.Combine(cutdownPath, rename);
                        break;
                    case "Video2":
                        input.Video2 = Path.Combine(cutdownPath, rename);
                        break;
                    default:
                        return (output, input);
                }



                using (FileStream filestream = await _fileServerHandler.CreationFileStream(videoPath, rename))
                {
                    file.CopyTo(filestream);
                }
                output = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, input);
        }

        #endregion

        #region Video upload logic
        public async Task AddFileToMerge(string input)
        {
            try
            {
                await _fileServerHandler.AddFileToMerge(input);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }

            //throw new NotImplementedException();
        }
        public async Task CreateFileDirectory(string uploadPath, string empty)
        {
            try
            {
                _fileServerHandler.CreateFileDirectory(uploadPath, empty);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task<FileStream> CreationFileStream(string uploadPath, string fileName)
        {
            try
            {
                return await _fileServerHandler.CreationFileStream(uploadPath, fileName);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task DeleteSplitFiles(string splitVideoFileSystemPath, string fileName)
        {
            try
            {
                _fileServerHandler.DeleteSplitFiles(splitVideoFileSystemPath, fileName);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
            // throw new NotImplementedException();
        }

        public async Task FileDelete(string uploadPath, string fileName)
        {
            try
            {
                _fileServerHandler.FileDelete(uploadPath, fileName);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task<bool> FileExists(string uploadPath, string fileName)
        {
            bool output = false;
            try
            {
                output = await _fileServerHandler.FileExists(uploadPath, fileName);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
            return output;
        }

        public async Task FileMover(string currentPath, string newPath)
        {
            try
            {
                _fileServerHandler.FileMover(currentPath, newPath);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task<string[]> GetDirectoryFileList(string path, string searchpattern)
        {
            string[] output = { };
            try
            {
                output = _fileServerHandler.GetDirectoryFileList(path, searchpattern);
                return output;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }

        }

        public async Task<bool> IsFileInUse(string input)
        {
            try
            {
                bool isInUse = true;
                isInUse = await _fileServerHandler.IsFileInUser(input);
                return isInUse;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        public async Task<FileStream> MergeFileStream(string fileName, FileMode open)
        {
            try
            {
                return await _fileServerHandler.MergeFileStream(fileName, open);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

        #endregion

    }
    #endregion

    #region Module
    public interface IModuleFileHandler
    {
        Task<FileStream> Download(Module module);
    }
    public class ModuleFileHandler : IModuleFileHandler
    {
        FileServerHandler _fileServerHandler;
        public ModuleFileHandler()
        {
            _fileServerHandler = new FileServerHandler();
        }


        public async Task<FileStream> Download(Module input)
        {
            try
            {
                string fileName = string.Concat(StaticDetails.ModuleFileSystemPath, input.UUID.ToString(), ".zip");
                FileStream output = _fileServerHandler.OutgoingFileStream(fileName);
                return output;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }
    }
    #endregion

    #region statement
    public interface IStatementFileHandler
    {
        Task<bool> UploadPackage(string uuid, string stringifiedStatement);

        /// <summary>
        /// Reads a stored statement back as bytes, or null when nothing is stored under that uuid.
        ///
        /// <para>
        /// This interface was WRITE-ONLY, which is the reason the statement JSON download could not
        /// be restored by wiring alone. Two read methods sat a hundred lines below, entirely
        /// commented out, one of them annotated "I think this is used for file downloading".
        /// </para>
        /// </summary>
        Task<byte[]> DownloadPackage(string uuid);
    }
    public class StatementFileHandler : IStatementFileHandler
    {
        FileServerHandler _fileServerHandler;
        public StatementFileHandler()
        {
            _fileServerHandler = new FileServerHandler();
        }

        /// <summary>
        /// Prefers the VERBATIM raw-body copy and falls back to the serialized one.
        ///
        /// <para>
        /// T4 gave the raw copy its own <c>.raw.json</c> suffix, so two artifacts can exist per
        /// statement. The raw one is the bytes the producer actually sent. The serialized one is
        /// built by the BLL and is lossy by construction: <c>JSONCharacterRemoval</c> strips every
        /// backslash from it, which corrupts any escaped content. A download labelled "Download
        /// JSON data" should hand back what was received, so the raw copy wins whenever it exists.
        /// </para>
        ///
        /// <para>
        /// The fallback matters because the raw copy only exists for statements ingested after T4
        /// and only on routes that persist it. Anything older has the serialized copy alone.
        /// </para>
        /// </summary>
        public async Task<byte[]> DownloadPackage(string uuid)
        {
            try
            {
                string directory = StaticDetails.JSONStatementFileSystemPath;
                string[] candidates =
                {
                    uuid + XApiStatementBinding.RawBodyFileSuffix,
                    uuid + ".json",
                };

                foreach (string name in candidates)
                {
                    if (!await _fileServerHandler.FileExists(directory, name))
                    {
                        continue;
                    }

                    using (FileStream stream = await _fileServerHandler.StatementFileRetrieval(directory, name))
                    {
                        if (stream == null)
                        {
                            continue;
                        }
                        using (MemoryStream memory = new MemoryStream())
                        {
                            await stream.CopyToAsync(memory);
                            return memory.ToArray();
                        }
                    }
                }

                // Not an error. A statement can predate the file writing, and the caller reports
                // "nothing stored" rather than failing the request.
                return null;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                return null;
            }
        }

        public async Task<bool> UploadPackage(string uuid, string stringifiedStatement)
        {
            bool output = false;
            try
            {
                //string fullPath = Path.Combine(StaticDetails.JSONStatementFileSystemPath, uuid + ".json");                
                //using (FileStream stream = File.Create(fullPath))
                //{
                //    byte[] statment = new UTF8Encoding(true).GetBytes(stringifiedStatement);
                //    stream.Write(statment, 0, statment.Length);
                //}

                using (FileStream stream = await _fileServerHandler.CreationFileStream(StaticDetails.JSONStatementFileSystemPath, uuid + ".json"))
                {
                    byte[] statment = new UTF8Encoding(true).GetBytes(stringifiedStatement);
                    stream.Write(statment, 0, statment.Length);                           
                }
                output = true;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //throw;
            }
            return output;
            //throw new NotImplementedException();
        }
        public async Task<bool> UploadPackage(IFormFile file, LocalSoftwarePackage input)
        {
            bool output = false;
            try
            {
                string something = file.Name;
                if (something != "files")
                {
                    // return (output, input);
                    return (output);
                }

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".zip" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    //return (output, input);
                    return (output);
                }

                string fileRename = input.UUID.ToString() + extension;

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(StaticDetails.LocalSoftwarePackage, fileRename))
                {
                    file.CopyTo(filestream);
                }
                output = true;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            //return (output, input);
            return (output);
        }

        public async Task<FileStream> DownloadPackage(Guid input)
        {
            try
            {
                string fileName = string.Concat(StaticDetails.JSONStatementFileSystemPath, input.ToString(), ".zip");
                FileStream output = _fileServerHandler.OutgoingFileStream(fileName);
                return output;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }



        #region json statement  

        /// <summary>
        /// Gathering JOnject for voiding and what not
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //        public async Task<JObject> JsonFileRetrieval(string path, string name)
        //        {
        //            try
        //            {
        //                JObject jobj = new JObject();
        //                SmbSettings settings = smbSettings();
        //                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
        //                CredentialCache credentialCache = new CredentialCache();
        //                credentialCache.Add(new Uri(path), "Basic", credentials);
        //                //string file = File.ReadAllText(Path.Combine(path,name));
        //#if (DEBUG)
        //                string file = File.ReadAllText(Path.Combine(path, name));
        //#elif (STAGING)
        //                string file = File.ReadAllText(path + name);
        //#else
        //                string file = File.ReadAllText(path + name);
        //#endif

        //                jobj = JsonConvert.DeserializeObject<JObject>(file);

        //                return jobj;
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }

        /// <summary>
        /// connecting to the file storage retriever
        /// I think this is used for file downloading
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //        public async Task<FileStream> StatementFileRetrieval(string path, string name)
        //        {
        //            try
        //            {
        //#if (DEBUG)
        //                string fullPath = Path.Combine(path, name);
        //#elif (STAGING)
        //                string fullPath = path + "/" + name;
        //#else
        //                string fullPath = path + "/" + name;
        //#endif
        //                SmbSettings settings = smbSettings();
        //                NetworkCredential credentials = new NetworkCredential(settings.UserName, settings.Secret);
        //                CredentialCache credentialCache = new CredentialCache();
        //                credentialCache.Add(new Uri(fullPath), "Basic", credentials);
        //                FileStream stream = new FileStream(fullPath, FileMode.Open);

        //                return stream;
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }

        #endregion

    }
    #endregion

    #region local software packages
    public interface ILocalSoftwarePackageFileHandler
    {
        Task<FileStream> DownloadPackage(LocalSoftwarePackage package);
        Task<(bool uploaded, LocalSoftwarePackage output)> UploadPackage(IFormFile file, LocalSoftwarePackage output);
    }
    public class LocalSoftwarePackageFileHandler : ILocalSoftwarePackageFileHandler
    {
        FileServerHandler _fileServerHandler;
        public LocalSoftwarePackageFileHandler()
        {
            _fileServerHandler = new FileServerHandler();
        }

        public async Task<(bool uploaded, LocalSoftwarePackage output)> UploadPackage(IFormFile file, LocalSoftwarePackage input)
        {
            bool output = false;
            try
            {
                string something = file.Name;
                if (something != "files")
                {
                    return (output, input);
                }

                //test if in correct format
                var extension = Path.GetExtension(file.FileName);
                string[] acceptableFileTypes = new string[] { ".zip" };
                if (!acceptableFileTypes.Any(extension.ToLower().Equals))
                {
                    return (output, input);
                }

                string fileRename = input.UUID.ToString() + extension;

                using (FileStream filestream = await _fileServerHandler.CreationFileStream(StaticDetails.LocalSoftwarePackage, fileRename))
                {
                    file.CopyTo(filestream);
                }
                output = true;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                //return null;
            }
            return (output, input);
        }

        public async Task<FileStream> DownloadPackage(LocalSoftwarePackage input)
        {
            try
            {
                string fileName = string.Concat(StaticDetails.LocalSoftwarePackage, input.UUID.ToString(), ".zip");
                FileStream output = _fileServerHandler.OutgoingFileStream(fileName);
                return output;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
        }

    }
    #endregion
}

