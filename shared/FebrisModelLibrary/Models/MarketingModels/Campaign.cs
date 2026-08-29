// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    public class Campaign:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public CampaignType CampaignType { get; set; }
        public string Name { get; set; }
        public string Objectives { get; set; }
        public string Sponsor { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Details { get; set; }
    }

    public class CampaignMember : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Lead Lead { get; set; }
        public Guid LeadUUID { get; set; }
        public Campaign Campaign { get; set; }
        public Guid CampaignUUID { get; set; }
    }

    public class CampaignNote : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }
        public Guid User { get; set; }
        public Campaign Campaign { get; set; }
        public Guid CampaignUUID { get; set; }
        
        public string Name { get; set; }
        public string Note { get; set; }        
    }

    public class TeamMemberAssignedToCampaign : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }
        public Campaign Campaign { get; set; }
        public Guid CampaignUUID { get; set; }
        public bool IsTeamLeader { get; set; }
        public Guid User { get; set; }        
    }

    public class EmailCampaignMessage : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }
        public Campaign Campaign { get; set; }
        public Guid CampaignUUID { get; set; }
        
        public bool Complete { get; set; }
        public string Subject { get; set; }

        public string HeaderImage { get; set; }
        public string HeaderImageCaption { get; set; }
        public List<string> SectionList { get; set; }
        //public string SectionList { get; set; }        
    }
   
    public class EmailSectionViewModel 
    {
        public Guid UUID { get; set; }
        //[DataMember]
        public int Index { get; set; }
       
        public string Title { get; set; }
        
        public string Body { get; set; }
        public bool IncludeImage { get; set; }

        public string ImagePath { get; set; }
        public string ImageCaption { get; set; }

        public string Hyperlink { get; set; }
    }
   
    public class FullEmailBuilderViewModel
    {
        
        public List<EmailSectionViewModel> EmailSectionViewModelList { get; set; }
        public EmailCampaignMessage EmailCampaignMessage { get; set; }
        public FullEmailBuilderViewModel()
        {
            EmailSectionViewModelList = new List<EmailSectionViewModel>();
        }
    }

    public class SingleSectionEmailBuilderViewModel
    {
        public EmailSectionViewModel EmailSectionViewModel { get; set; }
        public EmailCampaignMessage EmailCampaignMessage { get; set; }       
    }

    public class FebrisUserCampaignViewModel
    {
        public FebrisUserViewModel FebrisUserViewModel { get; set; }
        public Campaign Campaign { get; set; }
    }

    public class LeadCampaignViewModel
    {
        public Lead Lead { get; set; }
        public Campaign Campaign { get; set; }
    }
}
