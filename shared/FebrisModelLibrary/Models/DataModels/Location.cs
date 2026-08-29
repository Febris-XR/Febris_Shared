// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Interfaces.DataModelInterfaces;
using Febris.ModelLibrary.LookupModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class Location:BaseModel
    {
        //public long Id { get; set; }//can switch to guid instead of int        
        //public Guid UUID { get; set; }//this probably needs to change. I am also not sure if this will auto fill in


        [Display(Name = "Location Name")]
        public string Name { get; set; }
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }
        [Display(Name = "State")]
        public string State { get; set; }
        [Display(Name = "Country")]
        public string Country { get; set; }
       // [Display(Name = "Location type")]
        //public LocationType LocationType { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        //Location lookup list
        //public List<InstitutionLinkedLocation> InstitutionLinkedLocationList { get; set; }

        ////Users with location access
        //public List<LocationLinkedUser> LocationLinkedUserList { get; set; }

        ////Adding professionals to list of provider
        //public List<InstitutionLinkedProfessional> InstitutionLinkedProfessionalList { get; set; }

        ////add hardware list
        //public List<Hardware> HardwareList { get; set; }

    }
}
