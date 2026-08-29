// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class MarketplaceViewModels
    { }

    public class MarketplaceViewModel
    {
        public List<MarketplaceListingViewModel> MarketplaceListingViewModelList { get; set; }
        public List<Category> CategoryList { get; set; }
        public List<Tag> TagList { get; set; }
        public List<AccreditationBody> AccreditationBodyList { get; set; }
    }
    public class MarketplaceListingViewModel
    {
        //public List<Tag> TagList { get; set; }
        public MarketplaceListing MarketplaceListing { get; set; }
        //public Discount Discount { get; set; }
        //public Category Category { get; set; }
        //public List<Module> ModuleList { get; set; }
        //public Curriculum Curriculum { get; set; }
        //public AccreditationBody AccreditationBody { get; set; }

    }
    public class MarketplaceListingCreationViewModel
    {

        public MarketplaceListing MarketplaceListing { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public AccreditationBody AccreditationBody { get; set; }

        [Display(Name = "Use Curriculum Filters")]
        public bool UseCurriculumFilters { get; set; }

        [Display(Name = "Select Discount")]
        public SelectList DiscountList { get; set; }
        public Guid? Discount { get; set; }

        [Display(Name = "Select Curriculum")]
        public SelectList CurriculumList { get; set; }
        public Guid Curriculum { get; set; }

        [Display(Name = "Select Listing Industry")]
        public Guid? Industry { get; set; }
        public SelectList IndustryList { get; set; }

        [Display(Name = "Select Listing Category")]
        public Guid? Category { get; set; }
        public SelectList CategoryList { get; set; }

        [Display(Name = "Select Listing Focus")]
        public Guid? Focus { get; set; }
        public SelectList FocusList { get; set; }

        [Display(Name = "Select Listing Tags")]
        public List<Guid?> SelectedTagList { get; set; }
        public SelectList TagList { get; set; }

        [Display(Name = "Select Compatable Hardware")]
        public List<Guid?> SelectedXRHarwareModelList { get; set; }
        public SelectList xRHarwareModelList { get; set; }

        [Display(Name = "Classification of this Marketplace Listing")]
        public Guid SelectedMarketplaceListingClassification { get; set; }
        public SelectList MarketplaceListingClassificationList { get; set; }

        // File-upload integrity check (passed to FebrisSecurityMethods.CheckSumValidation).
        public Dictionary<string, string> Checksums { get; set; }
    }

    public class MarketplaceViewModelWithDropDowns
    {
        public List<Industry> IndustryList { get; set; }
        public List<MarketplaceListing> MarketplaceListing { get; set; }

        [Display(Name = "Select Module Industry")]
        public Guid? Industry { get; set; }
        public SelectList IndustrySelectList { get; set; }

        [Display(Name = "Select Module Category")]
        public Guid? Category { get; set; }
        public SelectList CategoryList { get; set; }

        [Display(Name = "Select Module Focus")]
        public Guid? Focus { get; set; }
        public SelectList FocusList { get; set; }

        [Display(Name = "Select Relevant Tags")]
        public Guid? SelectedTag { get; set; }
        public SelectList TagList { get; set; }

        [Display(Name = "Select XR hardware types")]
        public Guid? SelectedXRHardwareType { get; set; }
        public SelectList XRHardwareTypeList { get; set; }

       
    }
    public class PrivateMarketplaceListingWhiteListViewModel
    {

        //make select lists
        public SelectList InstitutionSelectList { get; set; }
        public Guid SelectedInstitution { get; set; }


        public PrivateMarketplaceListingWhiteList PrivateMarketplaceListingWhiteList { get; set; }
    }

    public class MultiSelectPrivateMarketplaceListingWhiteListViewModel
    {

        //make select lists
        public SelectList InstitutionSelectList { get; set; }
        public List<Guid> SelectedInstitutionList { get; set; }
        public bool DisableInsitutionSelectList { get; set; }

        public SelectList MarketplaceListingSelectList { get; set; }
        public List<Guid> SelectedListingList { get; set; }
        public bool DisableMarketplaceListingSelectList { get; set; }
    }
    public class GenericSelectListViewModel {

        public Guid UUID { get; set; }
        public string Name { get; set; }
    }

}
