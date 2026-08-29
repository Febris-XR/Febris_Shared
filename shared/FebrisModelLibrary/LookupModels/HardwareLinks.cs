// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class HardwareLinks
    {
    }
    public class HardwareLinkedContentDeveloper : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }        
        //public DateTime UpdateTimeStamp { get; set; }
        [Required]
        public Hardware Hardware { get; set; }
        [Required]
        public Guid HardwareUUID { get; set; }
        [Required]
        public ContentDeveloper ContentDeveloper { get; set; }
        [Required]
        public Guid ContentDeveloperUUID { get; set; }
    }
    public class HardwareLinkedAccreditationBody : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }        
        //public DateTime UpdateTimeStamp { get; set; }
        [Required]
        public Hardware Hardware { get; set; }
        [Required]
        public Guid HardwareUUID { get; set; }
        [Required]
        public AccreditationBody AccreditationBody { get; set; }
        [Required]
        public Guid AccreditationBodyUUID { get; set; }
    }
    public class HardwareLinkedFebris : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }        
        //public DateTime UpdateTimeStamp { get; set; }
        [Required]
        public Hardware Hardware { get; set; }
        [Required]
        public Guid HardwareUUID { get; set; }
    }
    public class HardwareLinkedCurriculum:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; } // lets use this to link? otherwise it is not stated as needed

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        [Required]
        public Hardware Hardware { get; set; }
        [Required]
        public Guid HardwareUUID { get; set; }
        [Required]
        public Curriculum Curriculum { get; set; }
        [Required]
        public Guid CurriculumUUID { get; set; }
    }
    public class HardwareLinkedModule : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; } 

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        [Required]
        public Hardware Hardware { get; set; }
        [Required]
        public Guid HardwareUUID { get; set; }
        [Required]
        public Module Module { get; set; }
        [Required]
        public Guid ModuleUUID { get; set; }
    }
    public class LocalHardwareLinkedModule : BaseModel
    {  
        [Required]
        public LocalHardware Hardware { get; set; }
        [Required]
        public Guid HardwareUUID { get; set; }
        //public Module Module { get; set; }
        [Required]
        public long ModuleId { get; set; }
        [Required]
        public Guid ModuleUUID { get; set; }
    }

    /// <summary>
    /// The NODE's twin of <see cref="HardwareLinkedCurriculum"/>, navigating
    /// <see cref="LocalHardware"/> instead of the central <see cref="Hardware"/> aggregate.
    ///
    /// <para>
    /// WHY THIS EXISTS. The central-typed original is still correct for the central and developer
    /// tiers and is registered by their context, so it cannot be retyped in place. But the NODE
    /// registered it too, which dragged central <c>Hardware</c> into the node's EF model. Because
    /// <c>DbSet&lt;LocalHardware&gt;</c> had already claimed the table name "Hardware", EF Core 3.1
    /// silently exiled the central entity to a second table called <c>Hardware1</c> and pointed
    /// this link's foreign key at it. <c>Hardware1</c> has no writer and is permanently empty, so
    /// linking a device to a curriculum could only ever fail with <c>23503</c>.
    /// </para>
    ///
    /// <para>
    /// Precedent for a twin rather than a retype is <see cref="LocalHardwareLinkedModule"/> above.
    /// The node DbSet keeps the property name <c>HardwareLinkedCurriculum</c>, so the table name is
    /// unchanged and this is a foreign-key repoint rather than a rename. Unlike
    /// <see cref="LocalHardwareLinkedModule"/>, the <c>Curriculum</c> navigation is KEPT: that one
    /// demoted <c>Module</c> to a bare id because the node has no Module table, which is not the
    /// case here.
    /// </para>
    /// </summary>
    public class LocalHardwareLinkedCurriculum : BaseModel
    {
        [Required]
        public LocalHardware Hardware { get; set; }
        [Required]
        public Guid HardwareUUID { get; set; }
        [Required]
        public Curriculum Curriculum { get; set; }
        [Required]
        public Guid CurriculumUUID { get; set; }
    }

    public class HardwareLinkedCohort : BaseModel
    {
        [Required]
        public LocalHardware Hardware { get; set; }
        [Required]
        public Guid HardwareUUID { get; set; }
        [Required]
        public Cohort Cohort { get; set; }
        [Required]
        public Guid CohortUUID { get; set; }
    }


    public class LocalHardwareLinkedModuleViewModel
    {        
        //public LocalHardware Hardware { get; set; }
        public LocalHardwareLinkedModule LocalHardwareLinkedModule { get; set; }

        public Module Module { get; set; }
    }
}
