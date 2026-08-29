// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.UserModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    //public class CohortViewModel
    //{
    //    //public string StatusMessage { get; set; }
    //    //public string SearchString { get; set; }
    //    //public string CurrentFilter { get; set; }
    //    public Cohort Cohort { get; set; }
    //   // public List<Professional> ProfessionalList { get; set; }
    //    public List<Curriculum> CurriculumList { get; set; }
    //    public List<Location> LocationList { get; set; }
    //}
    //public class CohortMemberViewModel
    //{
    //    public CohortMember CohortMember { get; set; }        
    //    public List<LocalUserViewModel> UserData{ get; set; }        
    //}
    public class CohortViewModel
    {
        public LocalApplicationUser Instructor { get; set; }
        public Cohort Cohort { get; set; }
        public List<CohortMemberViewModel> MemberList { get; set; }
    }

    public class CohortMemberViewModel
    {
        public LocalUserViewModel UserData { get; set; }
        public CohortMember CohortMember { get; set; }
    }


    public class CohortAccessListViewModel
    {
        public List<CohortAccessEntryViewModel> AccessList { get; set; }
            
    }
    public class CohortAccessEntryViewModel
    {
        /// <summary>How many cohort members this curriculum reaches.</summary>
        public int Seats { get; set; }
        /// <summary>
        /// Was MarketplaceListing. A node's cohort access is curriculum-derived: the cohort is
        /// linked to curricula (CohortLinkedCurriculum) and every member inherits that access.
        /// Nothing outside enduser/ referenced the old property, so this is not a hub-visible break.
        /// </summary>
        public Curriculum Curriculum { get; set; }
    }
}
