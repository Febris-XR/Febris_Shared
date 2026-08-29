// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    //class LocationLinks
    //{
    //}

    /// <summary>
    /// This one may not be needed
    /// </summary>
    public class LocationLinkedProfessional:BaseModel
    {       
        public Location Location { get; set; }
        public Guid LocationUUID { get; set; }
        //public Professional Professional { get; set; }
        //public Guid ProfessionalUUID { get; set; }
    }
    public class LocationLinkedUser:BaseModel
    {       
        public Location Location { get; set; }
        public Guid LocationUUID { get; set; }
        
        public Guid UserId { get; set; }
        public AttachmentStatus AttachmentStatus { get; set; }
    }
    public class LocationLinkedHardware:BaseModel
    {

        public Hardware Hardware { get; set; }
        public Guid HardwareUUID { get; set; }
        public Location Location { get; set; }
        public Guid LocationUUID { get; set; }
    }

    /// <summary>
    /// The NODE's twin of <see cref="LocationLinkedHardware"/>, navigating
    /// <see cref="LocalHardware"/> instead of the central <see cref="Hardware"/> aggregate.
    ///
    /// <para>
    /// Same cause and same fix as <c>LocalHardwareLinkedCurriculum</c>: the node registered the
    /// central-typed original, which pulled central <c>Hardware</c> into the node's EF model, where
    /// EF exiled it to the empty <c>Hardware1</c> table and pointed this link's foreign key there.
    /// Linking a device to a location could only ever fail with <c>23503</c>. The original above
    /// stays exactly as it is for the central and developer tiers, which register it against real
    /// central Hardware rows.
    /// </para>
    ///
    /// <para>
    /// The node DbSet keeps the property name <c>LocationLinkedHardware</c>, so the table name is
    /// unchanged and this is a foreign-key repoint rather than a rename.
    /// </para>
    /// </summary>
    public class LocalLocationLinkedHardware : BaseModel
    {
        public LocalHardware Hardware { get; set; }
        public Guid HardwareUUID { get; set; }
        public Location Location { get; set; }
        public Guid LocationUUID { get; set; }
    }




}
