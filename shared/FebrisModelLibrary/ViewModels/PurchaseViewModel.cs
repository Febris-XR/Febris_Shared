// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.LookupModels;
using Febris.ModelLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{

    //public class PurchaseViewModel
    //{
    //    public LocalPurchase LocalPurchase { get; set; }
    //    public Guid LocalPurchaseUUID { get; set; }
    //    public DateTime LocalPurchaseTimestamp { get; set; }
    //    public Guid ActorUUID { get; set; }
    //    public Guid ProfessionalUUID { get; set; }
    //    public Guid CurriculumUUID { get; set; }
    //    public decimal Price { get; set; }
    //    public int Discount { get; set; }
    //    public bool HasBeenProcessed { get; set; }
    //    public Guid CorrespondingUUID { get; set; }
    //}

    public class PurchaseCreationViewModel
    {
        public MarketplaceListing MarketplaceListing { get; set; }
        //public List<Module> ModuleList { get; set; }
        public int SeatsToPurchase { get; set; }
    }

    public class PurchaseViewModel
    {
        public Purchase Purchase { get; set; }
    }

    public class PurchaseTransmissionViewModel
    {
        public Purchaser Purchaser { get; set; }
        public PurchaseCreationViewModel PurchaseCreationViewModel { get; set; }
    }

    public class CohortPurchaseTransmissionViewModel
    {
        public bool UseOpenSeatsFirst { get; set; }

        public Purchaser Purchaser { get; set; }

        public List<SeatAllotmentViewModel> SeatAllotmentList { get; set; }

        public MarketplaceListing MarketplaceListing { get; set; }
        //public PurchaseCreationViewModel PurchaseCreationViewModel { get; set; }
    }





    public class SetAssignSeatViewModel
    {
        public Guid PurchaseUUID { get; set; }
        public SeatAllotmentViewModel SeatAllotmentViewModel { get; set; }
    }

    public class SeatAllotmentViewModel
    {
        public Guid ActorUUID { get; set; }
        public Guid UserUUID { get; set; }
    }


    public class PurchaseDisputeCreationViewModel 
    {
        public Purchaser DisputingUser { get; set; }
        public Guid Purchase { get; set; }
        //public Guid PurchaseOrder { get; set; }
        public IssueCategory IssueCategory { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }


    public class SeatCheckRequestViewModel
    {
        public Guid ActorUUID { get; set; }
        public ModuleLinkedObject ModuleLinkedObject { get; set; }
    }



}
