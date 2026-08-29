// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.TicketModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Febris.SharedServices
{
    public class StaticDetails
    {
        //public static string BaseFileSystemPath = @"\\127.0.0.1\febris\";
        //public static string BaseFileSystemPath = @"\\__STORAGE_HOST__\febris\";
        
        ////public static string BaseFileSystemPath = (Smb.Configuration?.GetValue<string>("SmbClient:Path")) ?? string.Empty;
        ////########################################################################    
        public static string UniqueFileSystemPath = @"TestServer\";
        ////public static string UniqueFileSystemPath = (Smb.Configuration?.GetValue<string>("FileSystem:UniqueFileSystemPath")) ?? string.Empty;
        ////########################################################################
        //public static string SpecificFileSystemPath = BaseFileSystemPath + UniqueFileSystemPath;
#if (DEBUG)
public static string BaseFileSystemPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FebrisPlatformTestFileSystem");
        //public static string BaseFileSystemPath = @"\\127.0.0.1\febris\";        
        ////########################################################################    
        //public static string UniqueFileSystemPath = @"TestServer\";        
        //########################################################################
        public static string SpecificFileSystemPath = BaseFileSystemPath + UniqueFileSystemPath;

        #region Generic Breakdown 
        ///Media generic
        public static string MediaFileSystemPath = SpecificFileSystemPath + @"media\";
        //video
        public static string VideoFileSystemPath = MediaFileSystemPath + @"video\";
        public static string SplitVideoFileSystemPath = VideoFileSystemPath + @"SplitVideos\";
        public static string RecordingsFileSystemPath = VideoFileSystemPath + @"recordings\";
        //Images
        public static string ImageFileSystemPath = MediaFileSystemPath + @"Images\";
        public static string LogoFileSystemPath = ImageFileSystemPath + @"Logos\";
        public static string ProfessionalFileSystemPath = ImageFileSystemPath + @"ProfessionalImages\";
        public static string ContentDeveloperLogoFileSystemPath = ImageFileSystemPath + @"DeveloperLogos\";        
        #endregion
        #region Generic but leaving the specific path for something more generic
        ///??? I think this is for the more generic setup
        public static string ProfessionalFileSystemPathForDb = @"ProfessionalImages\";
        public static string LogoFileSystemPathForDb = @"Logos\";
        public static string ContentDeveloperLogoFileSystemPathForDb = @"ContentDeveloperLogos\";
        #endregion
        #region Local software Package
        ///software packages for download
        public static string LocalSoftwarePackage = BaseFileSystemPath + @"LocalSoftwarePackage\";
        #endregion
        #region Marketplace
        ///Market place media (seperated by listing)
        public static string MarketplaceListingPath = BaseFileSystemPath + @"MarketplaceListings\";
        public static string MarketplaceListingScreenshotPath = @"Screenshot\";
        public static string MarketplaceListingVideoPath = @"Video\";
        #endregion
        #region Module path
        ///Modules
        public static string ModuleFileSystemPath = BaseFileSystemPath + @"modules\";
        #endregion
        #region Statement Files
        ///Test User Statement Files
        public static string StatementFileSystemPath = SpecificFileSystemPath + @"statements\";
        public static string VoidStatementFileSystemPath = StatementFileSystemPath + @"voidstatements\";
        public static string JSONStatementFileSystemPath = StatementFileSystemPath + @"JSONstatements\";
        #endregion
        #region Publication Path
        public static string PublicationPath = BaseFileSystemPath + @"Publications\";
        public static string PublicationImagePath = @"Images\";
        public static string PublicationVideoPath = @"Videos\";
        #endregion
        #region Email Campaign Path
        public static string EmailCampaignPath = BaseFileSystemPath + @"EmailCampaign\";
        public static string EmailCampaignImagePath = @"Images\";        
        #endregion
        #region Logs
        ///Logs, unsure if I should be useing specificFileSystemPath if the deployment is monolithic but I also would not hurt
        public static string LogFileSystemPath = SpecificFileSystemPath + @"logs\";
        public static string APILogFileSystemPath = LogFileSystemPath + @"api\";
        public static string PortalLogFileSystemPath = LogFileSystemPath + @"portal\";
        public static string AdminPortalLogFileSystemPath = LogFileSystemPath + @"adminportal\";
        #endregion


        #region debug
        //media
        //public static string MediaFileSystemPath = SpecificFileSystemPath + @"media\";
        ////video
        //public static string VideoFileSystemPath = MediaFileSystemPath + @"video\";
        ////split video
        //public static string SplitVideoFileSystemPath = VideoFileSystemPath + @"SplitVideos\";
        ////recordings
        //public static string RecordingsFileSystemPath = VideoFileSystemPath + @"recordings\";
        ////Images
        //public static string ImageFileSystemPath = MediaFileSystemPath + @"Images\";
        ////Logos
        //public static string LogoFileSystemPath = ImageFileSystemPath + @"Logos\";
        ////Professional Images
        //public static string ProfessionalFileSystemPath = ImageFileSystemPath + @"ProfessionalImages\";
        ////Edu Org logo
        //public static string ContentDeveloperLogoFileSystemPath = ImageFileSystemPath + @"ContentDeveloperLogos\";
        ////Professional Images for DB
        //public static string ProfessionalFileSystemPathForDb = @"ProfessionalImages\";
        ////provider Images
        //public static string LogoFileSystemPathForDb = @"Logos\";
        ////Edu org Images
        //public static string ContentDeveloperLogoFileSystemPathForDb = @"EduOrgLogos\";
        ////Badges
        ////local software packages
        //public static string LocalSoftwarePackage = @"LocalSoftwarePackage\";

        ////########################################################################
        //public static string PublicationPath = @"Publications\";
        //public static string PublicationImagePath = @"Images\";
        //public static string PublicationVideoPath = @"Videos\";
        ////########################################################################
        //public static string MarketplaceListingPath = @"MarketplaceListings\";
        ////Marketplace listing screenshots
        //public static string MarketplaceListingScreenshotPath = @"Screenshot\";
        ////Marketplace listing videos
        //public static string MarketplaceListingVideoPath = @"Video\";
        ////########################################################################
        ////modules
        //public static string ModuleFileSystemPath = BaseFileSystemPath + @"modules\";
        ////########################################################################
        ////statements
        //public static string StatementFileSystemPath = SpecificFileSystemPath + @"statements\";
        //public static string VoidStatementFileSystemPath = StatementFileSystemPath + @"voidstatements\";
        //public static string JSONStatementFileSystemPath = StatementFileSystemPath + @"JSONstatements\";
        ////########################################################################
        ////logs
        //public static string LogFileSystemPath = SpecificFileSystemPath + @"logs\";
        ////api
        //public static string APILogFileSystemPath = LogFileSystemPath + @"api\";
        ////portal
        //public static string PortalLogFileSystemPath = LogFileSystemPath + @"portal\";
        #endregion
#elif (STAGING)
        public static string BaseFileSystemPath = @"\\__STORAGE_HOST__\febris\";
        //public static string BaseFileSystemPath = (Smb.Configuration?.GetValue<string>("SmbClient:Path")) ?? string.Empty;
        ////########################################################################            
        //public static string UniqueFileSystemPath = (Smb.Configuration?.GetValue<string>("FileSystem:UniqueFileSystemPath")) ?? string.Empty;
        //########################################################################
        public static string SpecificFileSystemPath = BaseFileSystemPath + UniqueFileSystemPath;

        #region Generic Breakdown 
        ///Media generic
        public static string MediaFileSystemPath = SpecificFileSystemPath + @"media/";
        //video
        public static string VideoFileSystemPath = MediaFileSystemPath + @"video/";
        public static string SplitVideoFileSystemPath = VideoFileSystemPath + @"SplitVideos/";
        public static string RecordingsFileSystemPath = VideoFileSystemPath + @"recordings/";
        //Images
        public static string ImageFileSystemPath = MediaFileSystemPath + @"Images/";
        public static string LogoFileSystemPath = ImageFileSystemPath + @"Logos/";
        public static string ProfessionalFileSystemPath = ImageFileSystemPath + @"ProfessionalImages/";
        public static string ContentDeveloperLogoFileSystemPath = ImageFileSystemPath + @"DeveloperLogos/";        
        #endregion
        #region Generic but leaving the specific path for something more generic
        ///??? I think this is for the more generic setup
        public static string ProfessionalFileSystemPathForDb = @"ProfessionalImages/";
        public static string LogoFileSystemPathForDb = @"Logos/";
        public static string ContentDeveloperLogoFileSystemPathForDb = @"ContentDeveloperLogos/";
        #endregion
        #region Local software Package
        ///software packages for download
        public static string LocalSoftwarePackage = BaseFileSystemPath + @"LocalSoftwarePackage/";
        #endregion
        #region Marketplace
        ///Market place media (seperated by listing)
        public static string MarketplaceListingPath = BaseFileSystemPath + @"MarketplaceListings/";
        public static string MarketplaceListingScreenshotPath = @"Screenshot/";
        public static string MarketplaceListingVideoPath = @"Video/";
        #endregion
        #region Module path
        ///Modules
        public static string ModuleFileSystemPath = BaseFileSystemPath + @"modules/";
        #endregion
        #region Statement Files
        ///Test User Statement Files
        public static string StatementFileSystemPath = SpecificFileSystemPath + @"statements/";
        public static string VoidStatementFileSystemPath = StatementFileSystemPath + @"voidstatements/";
        public static string JSONStatementFileSystemPath = StatementFileSystemPath + @"JSONstatements/";
        #endregion
        #region Publication Path
        public static string PublicationPath =BaseFileSystemPath + @"Publications/";
        public static string PublicationImagePath = @"Images/";
        public static string PublicationVideoPath = @"Videos/";
        #endregion
        #region Email Campaign Path
        public static string EmailCampaignPath = BaseFileSystemPath + @"EmailCampaign/";
        public static string EmailCampaignImagePath = @"Images/";        
        #endregion
        #region Logs
        ///Logs, unsure if I should be useing specificFileSystemPath if the deployment is monolithic but I also would not hurt
        public static string LogFileSystemPath = SpecificFileSystemPath + @"logs/";
        public static string APILogFileSystemPath = LogFileSystemPath + @"api/";
        public static string PortalLogFileSystemPath = LogFileSystemPath + @"portal/";
        public static string AdminPortalLogFileSystemPath = LogFileSystemPath + @"adminportal/";
        #endregion


        #region staging
        ////public static string BaseFileSystemPath = (Smb.Configuration?.GetValue<string>("SmbClient:Path")) ?? string.Empty;
        //////########################################################################        
        ////public static string UniqueFileSystemPath = (Smb.Configuration?.GetValue<string>("FileSystem:UniqueFileSystemPath")) ?? string.Empty;        
        //////########################################################################
        ////public static string SpecificFileSystemPath = BaseFileSystemPath + UniqueFileSystemPath;
        ////########################################################################
        ////media
        //public static string MediaFileSystemPath = SpecificFileSystemPath + @"media/";
        ////video
        //public static string VideoFileSystemPath = MediaFileSystemPath + @"video/";
        ////split video
        //public static string SplitVideoFileSystemPath = VideoFileSystemPath + @"SplitVideos/";
        ////recordings
        //public static string RecordingsFileSystemPath = VideoFileSystemPath + @"recordings/";
        ////Images
        //public static string ImageFileSystemPath = MediaFileSystemPath + @"Images/";
        ////Logos
        //public static string LogoFileSystemPath = ImageFileSystemPath + @"Logos/";
        ////Professional Images
        //public static string ProfessionalFileSystemPath = ImageFileSystemPath + @"ProfessionalImages/";
        ////Edu Org logo
        //public static string EduOrgLogoFileSystemPath = ImageFileSystemPath + @"EduOrgLogos/";
        ////Professional Images for DB
        //public static string ProfessionalFileSystemPathForDb = @"ProfessionalImages/";
        ////provider Images
        //public static string LogoFileSystemPathForDb = @"Logos/";
        ////Edu org Images
        //public static string EduOrgLogoFileSystemPathForDb = @"EduOrgLogos/";
        ////########################################################################
        ////modules
        //public static string ModuleFileSystemPath = BaseFileSystemPath + @"modules/";
        ////########################################################################
        ////statements
        //public static string StatementFileSystemPath = SpecificFileSystemPath + @"statements/";
        //public static string VoidStatementFileSystemPath = StatementFileSystemPath + @"voidstatements/";
        //public static string JSONStatementFileSystemPath = StatementFileSystemPath + @"JSONstatements/";
        ////########################################################################
        ////logs
        //public static string LogFileSystemPath = SpecificFileSystemPath + @"logs/";
        ////api
        //public static string APILogFileSystemPath = LogFileSystemPath + @"api/";
        ////portal
        //public static string PortalLogFileSystemPath = LogFileSystemPath + @"portal/";
        #endregion
#else
public static string BaseFileSystemPath = @"\\__STORAGE_HOST__\febris\";
        //public static string BaseFileSystemPath = (Smb.Configuration?.GetValue<string>("SmbClient:Path")) ?? string.Empty;
        ////########################################################################            
        //public static string UniqueFileSystemPath = (Smb.Configuration?.GetValue<string>("FileSystem:UniqueFileSystemPath")) ?? string.Empty;
        //########################################################################
        public static string SpecificFileSystemPath = BaseFileSystemPath + UniqueFileSystemPath;

        #region Generic Breakdown 
        ///Media generic
        public static string MediaFileSystemPath = SpecificFileSystemPath + @"media/";
        //video
        public static string VideoFileSystemPath = MediaFileSystemPath + @"video/";
        public static string SplitVideoFileSystemPath = VideoFileSystemPath + @"SplitVideos/";
        public static string RecordingsFileSystemPath = VideoFileSystemPath + @"recordings/";
        //Images
        public static string ImageFileSystemPath = MediaFileSystemPath + @"Images/";
        public static string LogoFileSystemPath = ImageFileSystemPath + @"Logos/";
        public static string ProfessionalFileSystemPath = ImageFileSystemPath + @"ProfessionalImages/";
        public static string ContentDeveloperLogoFileSystemPath = ImageFileSystemPath + @"DeveloperLogos/";        
        #endregion
        #region Generic but leaving the specific path for something more generic
        ///??? I think this is for the more generic setup
        public static string ProfessionalFileSystemPathForDb = BaseFileSystemPath +@"ProfessionalImages/";
        public static string LogoFileSystemPathForDb = BaseFileSystemPath +@"Logos/";
        public static string ContentDeveloperLogoFileSystemPathForDb =BaseFileSystemPath + @"ContentDeveloperLogos/";
        #endregion
        #region Local software Package
        ///software packages for download
        public static string LocalSoftwarePackage = BaseFileSystemPath + @"LocalSoftwarePackage/";
        #endregion
        #region Marketplace
        ///Market place media (seperated by listing)
        public static string MarketplaceListingPath = BaseFileSystemPath + @"MarketplaceListings/";
        public static string MarketplaceListingScreenshotPath = @"Screenshot/";
        public static string MarketplaceListingVideoPath = @"Video/";
        #endregion
        #region Module path
        ///Modules
        public static string ModuleFileSystemPath = BaseFileSystemPath + @"modules/";
        #endregion
        #region Statement Files
        ///Test User Statement Files
        public static string StatementFileSystemPath = SpecificFileSystemPath + @"statements/";
        public static string VoidStatementFileSystemPath = StatementFileSystemPath + @"voidstatements/";
        public static string JSONStatementFileSystemPath = StatementFileSystemPath + @"JSONstatements/";
        #endregion
        #region Publication Path
        public static string PublicationPath = BaseFileSystemPath +@"Publications/";
        public static string PublicationImagePath = @"Images/";
        public static string PublicationVideoPath = @"Videos/";
        #endregion
        #region Email Campaign Path
        public static string EmailCampaignPath = BaseFileSystemPath + @"EmailCampaign/";
        public static string EmailCampaignImagePath = @"Images/";        
        #endregion
        #region Logs
        ///Logs, unsure if I should be useing specificFileSystemPath if the deployment is monolithic but I also would not hurt
        public static string LogFileSystemPath = SpecificFileSystemPath + @"logs/";
        public static string APILogFileSystemPath = LogFileSystemPath + @"api/";
        public static string PortalLogFileSystemPath = LogFileSystemPath + @"portal/";
        public static string AdminPortalLogFileSystemPath = LogFileSystemPath + @"adminportal/";
        #endregion


        #region production
        ////public static string BaseFileSystemPath = (Smb.Configuration?.GetValue<string>("SmbClient:Path")) ?? string.Empty;
        //////########################################################################        
        ////public static string UniqueFileSystemPath = (Smb.Configuration?.GetValue<string>("FileSystem:UniqueFileSystemPath")) ?? string.Empty;        
        //////########################################################################
        ////public static string SpecificFileSystemPath = BaseFileSystemPath + UniqueFileSystemPath;
        ////########################################################################
        ////media
        //public static string MediaFileSystemPath = SpecificFileSystemPath + @"media/";
        ////video
        //public static string VideoFileSystemPath = MediaFileSystemPath + @"video/";
        ////split video
        //public static string SplitVideoFileSystemPath = VideoFileSystemPath + @"SplitVideos/";
        ////recordings
        //public static string RecordingsFileSystemPath = VideoFileSystemPath + @"recordings/";
        ////Images
        //public static string ImageFileSystemPath = MediaFileSystemPath + @"Images/";
        ////Logos
        //public static string LogoFileSystemPath = ImageFileSystemPath + @"Logos/";
        ////Professional Images
        //public static string ProfessionalFileSystemPath = ImageFileSystemPath + @"ProfessionalImages/";
        ////Edu Org logo
        //public static string EduOrgLogoFileSystemPath = ImageFileSystemPath + @"EduOrgLogos/";
        ////Professional Images for DB
        //public static string ProfessionalFileSystemPathForDb = @"ProfessionalImages/";
        ////provider Images
        //public static string LogoFileSystemPathForDb = @"Logos/";
        ////Edu org Images
        //public static string EduOrgLogoFileSystemPathForDb = @"EduOrgLogos/";
        ////########################################################################
        ////modules
        //public static string ModuleFileSystemPath = BaseFileSystemPath + @"modules/";
        ////########################################################################
        ////statements
        //public static string StatementFileSystemPath = SpecificFileSystemPath + @"statements/";
        //public static string VoidStatementFileSystemPath = StatementFileSystemPath + @"voidstatements/";
        //public static string JSONStatementFileSystemPath = StatementFileSystemPath + @"JSONstatements/";
        ////########################################################################
        ////logs
        //public static string LogFileSystemPath = SpecificFileSystemPath + @"logs/";
        ////api
        //public static string APILogFileSystemPath = LogFileSystemPath + @"api/";
        ////portal
        //public static string PortalLogFileSystemPath = LogFileSystemPath + @"portal/";
        ////########################################################################
        #endregion
#endif
        //local defualts
        public const string DefaultPath = "/images/DefaultMedia/";
        public const string DefaultPicture = DefaultPath + "DefaultPerson.png";
        public const string DefaultLogo = DefaultPath + "default-logo.png";
        public const string DefaultVideo = DefaultPath + "Default_Video.mp4";
        //########################################################################
        //constants for uris
        //########################################################################
        public const string xApiObjectUri = "https://febr.is/Module/";

        /// <summary>
        /// temp cookie storage
        /// </summary>
        public static string SnickerDoodle = string.Empty;

        public static string UserAPIPath = string.Empty;

        public static IConfiguration PassedBackConfig;


        //Authentication Paths
        //public const string TokenAuthenticationPath = "/Authentication";

        public static LicenseAuthenticateResponse LicenseAuthenticateResponse { get; set; }



#if (DEBUG)
        //public static string UserAPIPath = "https://localhost:5001/api/";

#elif (STAGING)
        //public static string UserAPIPath = "https://localhost:5001/api/";

#else
        //public static string UserAPIPath = "https://localhost:5001/api/";

#endif

    }

    public class BackgroundStaticDetails
    {
        public static DateTime LastRestart { get; set; }
        public static int ErrorsLogged { get; set; }
        public static DateTime LastLogCheck { get; set; }


        public static DateTime ByTheMinuteCheck { get; set; }
        private static bool _byTheMinuteUpToDate;
        public static bool ByTheMinuteUpToDate
        {
            get { return _byTheMinuteUpToDate; }
            set { _byTheMinuteUpToDate = value; }
        }

        public static DateTime LastHourlyCheck { get; set; }
        private static bool _hourlyUpToDate;
        public static bool HourlyUpToDate
        {
            get { return _hourlyUpToDate; }
            set { _hourlyUpToDate = value; }
        }
        public static DateTime LastDailyCheck { get; set; }

        private static bool _dailyUpToDate;
        public static bool DailyUpToDate
        {
            get { return _dailyUpToDate; }
            set { _dailyUpToDate = value; }
        }
        public static DateTime LastWeeklyCheck { get; set; }

        private static bool _weeklyUpToDate;
        public static bool WeeklyUpToDate
        {
            get { return _weeklyUpToDate; }
            set { _weeklyUpToDate = value; }
        }

        public static DateTime LastMonthlyCheck { get; set; }

        private static bool _monthlyUpToDate;
        public static bool MonthlyUpToDate
        {
            get { return _monthlyUpToDate; }
            set { _monthlyUpToDate = value; }
        }


    }
}
