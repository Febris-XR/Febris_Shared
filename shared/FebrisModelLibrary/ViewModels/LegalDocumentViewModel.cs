// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.LegalModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    public class LegalDocumentViewModel
    {
        public LegalDocumentViewModel()
        {
            DocumentTitle = string.Empty;
            SectionList = new List<Section>();
        }
        public string DocumentTitle { get; set; }
        public List<Section> SectionList { get; set; }

    }
    public class Section
    {
        public Section()
        {
            Index = 0;
            Title = string.Empty;
            SubsectionList = new List<Subsection>();
        }
        public int Index { get; set; }
        public string Title { get; set; }
        public List<Subsection> SubsectionList { get; set; }
    }

    public class Subsection
    {
        public Subsection()
        {
            Index = 0;
            Title = string.Empty;
            Body = string.Empty;
        }

        public int Index { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

    public class EULALegalDocumentViewModel
    {
        public EULALegalDocumentViewModel()
        {
            SectionList = new List<Section>();
            EULA = new EULA();
        }
        public List<Section> SectionList { get; set; }
        public EULA EULA { get; set; }
    }

    public class LiabilityWaiverLegalDocumentViewModel
    {
        public List<Section> SectionList { get; set; }
        public LiabilityWaiver LiabilityWaiver { get; set; }
    }

    public class ServiceAgreementLegalDocumentViewModel
    {
        
        public List<Section> SectionList { get; set; }
        public ServiceAgreement ServiceAgreement { get; set; }
    }


    public class FebrisLegalDocumentViewModel
    {

        public LegalDocumentViewModel LegalDocumentViewModel { get; set; }
        public FebrisLegalDocument FebrisLegalDocument { get; set; }
    }
    public class AccreditationLegalDocumentViewModel
    {

        public LegalDocumentViewModel LegalDocumentViewModel { get; set; }
        public AccreditationLegalDocument AccreditationLegalDocument { get; set; }
    }
    public class DeveloperLegalDocumentViewModel
    {

        public LegalDocumentViewModel LegalDocumentViewModel { get; set; }
        public DeveloperLegalDocument DeveloperLegalDocument { get; set; }
    }
    public class InternalLegalDocumentViewModel
    {

        public LegalDocumentViewModel LegalDocumentViewModel { get; set; }
        public InternalLegalDocument InternalLegalDocument { get; set; }
    }
}
