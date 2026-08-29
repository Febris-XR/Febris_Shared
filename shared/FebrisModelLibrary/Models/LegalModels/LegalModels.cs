// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.LegalModels
{
    public class SignedServiceAgreement : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid? ActorUUID { get; set; }
        public bool Accept { get; set; }
    }
    public class SignedLiabilityWaiver : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid? ActorUUID { get; set; }
        public bool AcceptWaiver { get; set; }
    }
    public class SignedEULA : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid? ActorUUID { get; set; }
        public bool Accepted { get; set; }
    }
    public class ServiceAgreement : BaseModel
    {
        public bool Current { get; set; }
        public string Document { get; set; }
    }
    public class LiabilityWaiver : BaseModel
    {
        public bool Current { get; set; }
        public string Document { get; set; }
    }
    public class EULA : BaseModel
    {
        public bool Current { get; set; }
        public string Document { get; set; }
    }


    //public enum LegalDocumentType
    //{
    //    None =0,
    //    Generic =101,
    //    LicenseAgreement = 453,
    //    ServiceAgreement = 786,
    //    EULA = 981
    //}

    /// <summary>
    /// need a better name
    /// </summary>
    public class LegalDocument:BaseModel
    {
        public LegalDocument()
        {
            Published = false;
            LegalDocumentType = LegalDocumentType.None;
            Document = string.Empty;
        }
        public bool Archived { get; set; }
        public bool Published { get; set; }
        public LegalDocumentType LegalDocumentType { get; set; }       
        public string Document { get; set; }
    }

    public class DeveloperLegalDocument : LegalDocument
    {
        public DeveloperLegalDocument()
        {
            Published = false;
            LegalDocumentType = LegalDocumentType.None;
            Document = string.Empty;
        }
        //public ContentDeveloper ContentDeveloper { get; set; }
        [Required]
        public Guid ContentDeveloperUUID { get; set; }
    }

    public class AccreditationLegalDocument : LegalDocument
    {
        public AccreditationLegalDocument()
        {
            Published = false;
            LegalDocumentType = LegalDocumentType.None;
            Document = string.Empty;
        }
        //public ContentDeveloper ContentDeveloper { get; set; }
        [Required]
        public Guid AccreditationBodyUUID { get; set; }
    }

    public class FebrisLegalDocument : LegalDocument
    {
        public FebrisLegalDocument()
        {
            Published = false;
            LegalDocumentType = LegalDocumentType.None;
            Document = string.Empty;
        }
    }

    public class InternalLegalDocument : LegalDocument
    {
        public InternalLegalDocument()
        {
            Published = false;
            LegalDocumentType = LegalDocumentType.None;
            Document = string.Empty;
        }
    }
}
