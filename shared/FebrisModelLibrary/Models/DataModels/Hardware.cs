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
    /// <summary>
    /// This is for the management and distribution of hardware
    /// Ie: the specific laptop, desktop, or tablet
    /// Hardware must be registered to be used in the system
    /// </summary>
    public class Hardware : BaseModel
    {        
        /// <summary>
        /// What type of hardware is this?
        /// </summary>
        //public EnumLibrary.HardwareType HardwareType { get; set; }
        public Guid? HardwareTypeUUID { get; set; }
        [Display(Name = "Hardware Type")]
        public HardwareType HardwareType { get; set; }

        /// <summary>
        /// Generic Informaiton
        /// </summary>           
        [Display(Name = "Descriptive Name")]
        public string DescriptiveName { get; set; }        
        public string Description { get; set; }

        /// <summary>
        /// Hardware set
        /// </summary>    
        /// 
        [Display(Name = "Physical License")]
        public string PhysicalLicense { get; set; }

        /// <summary>
        /// Slight control
        /// </summary>
        /// 
        [Display(Name = "Hardware Condition")]
        public HardwareCondition HardwareCondition { get; set; }
        [Display(Name = "Is Locked Out")]
        public bool IsLockedOut { get; set; }
    }

    public class LocalHardware : BaseModel
    {        
        /// <summary>
        /// What kind of hardware is this? This is the node's SOURCE OF TRUTH.
        ///
        /// <para>
        /// The commented line that stood here named <c>EnumLibrary.HardwareType</c>, an enum that
        /// did not exist anywhere in this repository. <see cref="HardwareKind"/> is that idea,
        /// built.
        /// </para>
        /// </summary>
        [Display(Name = "Hardware Kind")]
        public HardwareKind HardwareKind { get; set; }

        /// <summary>
        /// Inert hub-reconciliation carriers. NOT the node's source of truth, and nothing on the
        /// node dispatches on them. They exist so a device registered while the hub is absent can
        /// still be matched against the hub's own hardware-type catalog if it comes back. The UUID
        /// is frozen per kind in <see cref="HardwareTypeCatalog"/>. The surrogate id is only
        /// meaningful inside one database.
        /// </summary>
        public Guid? HardwareTypeUUID { get; set; }
        [Display(Name = "Hardware Type")]
        public long HardwareTypeId { get; set; }

        /// <summary>
        /// Generic Informaiton
        /// </summary>           
        [Display(Name = "Descriptive Name")]
        public string DescriptiveName { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Hardware set
        /// </summary>    
        /// 
        [Display(Name = "Physical License")]
        public string PhysicalLicense { get; set; }

        /// <summary>
        /// Slight control
        /// </summary>
        /// 
        [Display(Name = "Hardware Condition")]
        public HardwareCondition HardwareCondition { get; set; }
        [Display(Name = "Is Locked Out")]
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// When this device's credential was last regenerated, in UTC, or null if it never was.
        ///
        /// <para>
        /// THE GAP THIS CLOSES. Regenerating a credential rewrites <see cref="PhysicalLicense"/>,
        /// which stops a thief AUTHENTICATING from scratch. It did not stop one who had already
        /// authenticated: refresh re-reads this row but only ever tested <see cref="IsLockedOut"/>,
        /// never the credential, and refresh tokens rotate on every call. So the stolen token chain
        /// renewed itself indefinitely and the documented incident response left the attacker
        /// connected while breaking the honest device.
        /// </para>
        ///
        /// <para>
        /// A TIMESTAMP RATHER THAN A COMPARISON, because the credential is stored as a hash and
        /// deliberately kept out of the token claim, so refresh has nothing to compare against. This
        /// records WHEN the old credential died, and refresh refuses any token minted before that
        /// moment. It is the same shape as <see cref="IsLockedOut"/>: durable state on the row,
        /// re-read on the paths that issue tokens, with the cache revocation list only closing the
        /// access-token window ahead of it.
        /// </para>
        ///
        /// <para>
        /// UTC, NOT LOCAL. It is compared against <c>RefreshHardwareToken.Created</c>, which is
        /// <c>DateTime.UtcNow</c>. Writing local time here would shift every comparison by the host's
        /// offset and would either strand honest devices or leave stolen ones running.
        /// </para>
        /// </summary>
        public DateTime? CredentialRegeneratedAt { get; set; }
    }
}
