// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.LookupModels;
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.XApiModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LauncherModels
{

    #region Authentication Models
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class HardwareInitializationRequest
    {
        public string UniqueIdentifier { get; set; }
    }



    #endregion


    #region Full Hardware Initialization 
    public class HardwareInitializationResponse
    {
        public HardwareInitializationResponse()
        {
            MessageboardViewModels = default;
            UserInitaliztionViewModels = default;
            ModuleInitaliztionViewModels = default;
            ModuleList = default;
        }

        public HardwareMessageboardViewModels MessageboardViewModels { get; set; }
        public HardwareUserInitaliztionViewModels UserInitaliztionViewModels { get; set; }
        public HardwareModuleInitaliztionViewModels ModuleInitaliztionViewModels { get; set; }
        public List<Module> ModuleList { get; set; }

    }



    #endregion


    #region Start Simulation 
    public class StatementInitalizationRequestViewModel
    {
        public Guid UserId { get; set; }
        public Guid ActorId { get; set; }
        public Guid ModuleId { get; set; }
        public bool IsTestUser { get; set; }

        /// <summary>
        /// IGNORED BY THE NODE since ROADMAP 22, and never populated by either shipped client
        /// before that: both build this request with ModuleId, UserId, ActorId and IsTestUser
        /// only, so the node's old record branch was dead code. The decision is now derived
        /// server-side from the educator's per-cohort policy
        /// (LauncherLogic.ShouldRecordSession). Retained on the wire contract because the central
        /// and developer tiers still bind this model; do not re-introduce a read of it on the node.
        /// </summary>
        public bool RecordSession { get; set; }
    }
    public class StatementInitalizationResponseViewModel
    {
        public Statement Statement { get; set; }
    }
    #endregion

    #region Statement Upload
    public class StatementUploadRequestViewModel
    {
        public Statement Statement { get; set; }
    }
    public class StatementUploadResponseViewModel
    {
        public bool Success { get; set; }

    }
    #endregion


    #region Direct Response ViewModels
    public class HardwareMessageboardViewModels
    {
        public HardwareMessageboardViewModels()
        {
            AdminMessageBoardList = default;
            MessageBoardList = default;
        }
        public List<AdminMessageBoard> AdminMessageBoardList { get; set; }
        public List<MessageBoard> MessageBoardList { get; set; }
    }

    public class HardwareUserInitaliztionViewModels
    {
        public HardwareUserInitaliztionViewModels()
        {
            CohortList = default;
            CohortMemberList = default;
            UserViewModelList = default;
        }
        public List<Cohort> CohortList { get; set; }
        public List<CohortMember> CohortMemberList { get; set; }
        public List<HardwareUserViewModel> UserViewModelList { get; set; }
        public List<UserAccessList> UserAccessLists { get; set; }
    }

    public class HardwareModuleInitaliztionViewModels
    {
        public HardwareModuleInitaliztionViewModels()
        {
            //CurriculumList = default;
            //ModuleList = default;
            ModuleLinkedCurriculumList = default;
        }
        //public List<Curriculum> CurriculumList { get; set; }
        //public List<Module> ModuleList { get; set; }
        public List<ModuleLinkedCurriculum> ModuleLinkedCurriculumList { get; set; }
    }
    #endregion


    #region Widget request models

    public class VideoFile
    {
        public string FileName { get; set; }
        public string TempFolder { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<string> FileParts { get; set; }
    }
    public class VideoFileUploadResponseViewModel
    {
        public bool Success { get; set; }

    }



    #endregion


    #region View Models 

    public class HardwareUserViewModel
    {
        public HardwareUserViewModel()
        {
            IsTestUser = false;
        }
        public Guid UserId { get; set; }
        public Guid ActorId { get; set; }
        public string IdentificationNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string ProfilePicturePath { get; set; }
        public string PicturePath { get; set; }
        public bool IsTestUser { get; set; }

        //public HardwareUserAccessViewModel AccessViewModel { get; set; }
    }
        
    public class UserAccessList
    {
        public UserAccessList(Guid UserId, Guid ActorId)
        {
            UserId = default;
            ActorId = default;
            ModuleIdList = default;
        }
        public Guid UserId { get; set; }
        public Guid ActorId { get; set; }
        public List<Guid> ModuleIdList { get; set; }
    }

    #endregion






    #region Unused
    //public class LauncherUserViewModel
    //{
    //    public Guid Id { get; set; }
    //    public Guid ActorId { get; set; }
    //    public string UniqueIdentifier { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string UserName { get; set; }
    //    public string EmailAddress { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public string ProfilePicturePath { get; set; }

    //    public bool IsTestUser { get; set; }       
    //}
    //public class HardwareInitializationModels
    //{
    //}

    //public class HardwareInitializationResponse
    //{
    //    public HardwareInitializationResponse()
    //    {


    //        //AdminMessageBoardList = default;
    //        //MessageBoardList = default;
    //        //CurriculumList = default;
    //        //ModuleList = default;
    //        //CohortList = default;
    //        //UserList = default;

    //    }
    //    //public List<AdminMessageBoard> AdminMessageBoardList { get; set; }
    //    //public List<MessageBoard> MessageBoardList { get; set; }

    //    //public List<Curriculum> CurriculumList { get; set; }
    //    //public List<Module> ModuleList { get; set; }
    //    //public List<Cohort> CohortList { get; set; }
    //    //public List<HardwareUserViewModel> UserList { get; set; }

    //    public HardwareMessageboardViewModels MessageboardViewModels { get; set; }
    //    public HardwareUserInitaliztionViewModels UserInitaliztionViewModels { get; set; }
    //    public HardwareModuleInitaliztionViewModels ModuleInitaliztionViewModels { get; set; }
    //}
    //public class HardwareUserAccessViewModel
    //{
    //    public HardwareUserAccessViewModel()
    //    {
    //        ModuleIdAccessList = default;
    //        CurriculumIdAccessList = default;
    //        CohortIdAccessList = default;
    //    }
    //    public List<long> ModuleIdAccessList { get; set; }
    //    public List<long> CurriculumIdAccessList { get; set; }
    //    public List<long> CohortIdAccessList { get; set; }
    //}
    #endregion


}
