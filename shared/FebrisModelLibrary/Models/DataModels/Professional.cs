// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.LookupModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    //public class Professional
    //{
    //    public long Id { get; set; }
    //    public Guid UUID { get; set; }

    //    public DateTime CreationTimeStamp { get; set; }
    //    public DateTime UpdateTimeStamp { get; set; }

    //    /// <summary>
    //    /// Link
    //    /// </summary>
    //    /// 
    //    public string UserName { get; set; }
    //    public ProfessionalSettings ProfessionalSettings { get; set; }
    //    public Guid ProfessionalSettingsUUID { get; set; }


    //    [Display(Name = "Account Type")]
    //    public UserAccountType UserAccountType { get; set; }

    //}

    public class ProfessionalViewModel : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        /// <summary>
        /// Link
        /// </summary>
        /// 
        public string UserName { get; set; }
        //public ProfessionalSettings ProfessionalSettings { get; set; }
        public Guid ProfessionalSettingsUUID { get; set; }


        [Display(Name = "Account Type")]
        public UserAccountType UserAccountType { get; set; }

    }


}
