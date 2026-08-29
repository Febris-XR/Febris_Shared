// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
//using Febris.ModelLibrary.LookupModels;
//using Febris.ModelLibrary.Models.DataModels;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Febris.ModelLibrary.ViewModels
//{
//    public class HardwareInitializationModels
//    {
//    }
//    public class HardwareInitializationRequest
//    {
//    }
//    //public class HardwareInitializationResponse
//    //{
//    //    public HardwareInitializationResponse()
//    //    {


//    //        //AdminMessageBoardList = default;
//    //        //MessageBoardList = default;
//    //        //CurriculumList = default;
//    //        //ModuleList = default;
//    //        //CohortList = default;
//    //        //UserList = default;

//    //    }
//    //    //public List<AdminMessageBoard> AdminMessageBoardList { get; set; }
//    //    //public List<MessageBoard> MessageBoardList { get; set; }

//    //    //public List<Curriculum> CurriculumList { get; set; }
//    //    //public List<Module> ModuleList { get; set; }
//    //    //public List<Cohort> CohortList { get; set; }
//    //    //public List<HardwareUserViewModel> UserList { get; set; }

//    //    public HardwareMessageboardViewModels MessageboardViewModels { get; set; }
//    //    public HardwareUserInitaliztionViewModels UserInitaliztionViewModels { get; set; }
//    //    public HardwareModuleInitaliztionViewModels ModuleInitaliztionViewModels { get; set; }
//    //}
//    public class HardwareInitializationResponse
//    {
//        public HardwareInitializationResponse()
//        {
//            MessageboardViewModels = default;
//            UserInitaliztionViewModels = default;
//            ModuleInitaliztionViewModels = default;
//        }
       
//        public HardwareMessageboardViewModels MessageboardViewModels { get; set; }
//        public HardwareUserInitaliztionViewModels UserInitaliztionViewModels { get; set; }
//        public HardwareModuleInitaliztionViewModels ModuleInitaliztionViewModels { get; set; }
//    }

//    public class HardwareUserViewModel
//    {
//        public HardwareUserViewModel()
//        {
//            IsTestUser = false;
//        }
//        public Guid UserId { get; set; }
//        public bool IsTestUser { get; set; }
//        public string FirstName { get; set; }
//        public string LastName { get; set; }
//        //not sure if this is possible
//        public string IdentificationNumber { get; set; }

//        public string EmailAddress { get; set; }
//        public string PhoneNumber { get; set; }
//        public Guid ActorId { get; set; }

//        public string PicturePath { get; set; }

//        //public HardwareUserAccessViewModel AccessViewModel { get; set; }
//    }

//    //public class HardwareUserAccessViewModel
//    //{
//    //    public HardwareUserAccessViewModel()
//    //    {
//    //        ModuleIdAccessList = default;
//    //        CurriculumIdAccessList = default;
//    //        CohortIdAccessList = default;
//    //    }
//    //    public List<long> ModuleIdAccessList { get; set; }
//    //    public List<long> CurriculumIdAccessList { get; set; }
//    //    public List<long> CohortIdAccessList { get; set; }
//    //}

//    public class HardwareUserInitaliztionViewModels 
//    {
//        public HardwareUserInitaliztionViewModels()
//        {
//            //CohortList = default;
//            CohortMemberList = default;
//            UserViewModelList = default;
//        }
//        //public List<Cohort> CohortList { get; set; }
//        public List<CohortMember> CohortMemberList { get; set; }
//        public List<HardwareUserViewModel> UserViewModelList { get; set; }
//    }

//    public class HardwareModuleInitaliztionViewModels
//    {
//        public HardwareModuleInitaliztionViewModels()
//        {
//            //CurriculumList = default;
//            //ModuleList = default;
//            ModuleLinkedCurriculumList = default;
//        }
//        //public List<Curriculum> CurriculumList { get; set; }
//        //public List<Module> ModuleList { get; set; }
//        public List<ModuleLinkedCurriculum> ModuleLinkedCurriculumList { get; set; }
//    }

//    public class HardwareMessageboardViewModels
//    {
//        public HardwareMessageboardViewModels()
//        {
//            AdminMessageBoardList = default;
//            MessageBoardList = default;
//        }
//        public List<AdminMessageBoard> AdminMessageBoardList { get; set; }
//        public List<MessageBoard> MessageBoardList { get; set; }
//    }



//}
