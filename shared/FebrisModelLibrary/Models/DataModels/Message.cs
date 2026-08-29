// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Interfaces.DataModelInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class Message : BaseModel
    {
        //MessageID
        //public long Id { get; set; }
        //public Guid UUID { get; set; } // lets use this to link? otherwise it is not stated as needed
        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }
        

        //Who is sending the message
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhoneNumber { get; set; }
        public string FurtherUserAssociations { get; set; }


        //Massage
        public string Subject { get; set; }
        public string MessageBody { get; set; }


        //Ticket Status        
        public TicketStatusType TicketStatusType { get; set; }
        //claims reguarding
        [Display(Name = "Regarding")]
        public TicketRegardingType TicketRegardingType { get; set; }


        //corresponding user
        public Guid CorrespondingUserId { get; set; }
        public string CorrespondingUserName { get; set; }
        public string CorrespondingUserEmail { get; set; }

    }
}
