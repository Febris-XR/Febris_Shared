// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    public enum UserAccountType
    {
        User = 101,
        //Educator = 201,
        //Supervisor = 301,
        //Executive = 401,
        //Legal =501,
        Admin = 601,
        [Display(Name = "IT Admin")] ITAdmin = 701,
        //[Display(Name = "User Parent")] UserParent = 1000,
        //[Display(Name = "Test Account")] TestAccount = 1010,
    }

    public enum InstitutionUserAccountType
    {
        User = 101,
        Educator = 201,
        //Supervisor = 301,
        //Executive = 401,
        //Legal = 501,
        Admin = 601,
        [Display(Name = "IT Admin")] ITAdmin = 701,
        [Display(Name = "User Parent")] UserParent = 1000,
        //[Display(Name = "Test Account")] TestAccount = 1010,
    }

    //public enum ContentDeveloperUserType
    //{
    //    CCTestUser,
    //    CCUser,
    //    CCAdmin,
    //    CCITAdmin
    //}

    //public enum AccreditationBodyUserType
    //{
    //    ABTestUser,
    //    ABUser,
    //    ABAdmin,
    //    ABITAdmin
    //}


    public enum FebrisUserType
    {
        [Display(Name = "Sales")] FebrisSales = 151,
        [Display(Name = "Support")] FebrisSupport = 251,
        [Display(Name = "Developer")] FebrisDeveloper = 351,
        [Display(Name = "Engineer")] FebrisEngineer = 451,
        [Display(Name = "System Admin")] SystemAdmin = 551,
        [Display(Name = "Super Admin")] SuperAdmin = 651,
    }
}
