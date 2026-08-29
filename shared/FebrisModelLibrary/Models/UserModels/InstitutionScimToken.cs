// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.UserModels
{
    /// <summary>
    /// Enterprise Tier 1: bearer token used by the customer's IdP (Okta,
    /// Entra, etc.) to authenticate against Febris's SCIM 2.0 endpoint.
    /// Each <c>Institution</c> can have multiple tokens to support
    /// rotation without downtime; the old token stays valid until
    /// <see cref="RevokedAt"/> is stamped.
    /// <para>
    /// <b>Plaintext is never persisted.</b> Tokens are generated via
    /// <c>PasswordGenerator</c>, returned to the admin ONCE for paste
    /// into the IdP's connector config, then hashed (SHA-256, see
    /// <c>ShaHandler</c>) for storage in <see cref="TokenHash"/>.
    /// Lookups during SCIM auth compare the request's bearer-token
    /// SHA-256 against <see cref="TokenHash"/> in constant time.
    /// </para>
    /// </summary>
    public class InstitutionScimToken : BaseModel
    {
        [Required]
        [Display(Name = "Institution")]
        public Guid InstitutionUUID { get; set; }

        [Required]
        [Display(Name = "Token hash (SHA-256)")]
        public string TokenHash { get; set; }

        [Display(Name = "Token prefix")]
        public string TokenPrefix { get; set; }

        [StringLength(200)]
        [Display(Name = "Label")]
        public string Label { get; set; }

        [Display(Name = "Created by")]
        public Guid? CreatedByUserId { get; set; }

        [Display(Name = "Revoked at")]
        public DateTime? RevokedAt { get; set; }

        [Display(Name = "Revoked by")]
        public Guid? RevokedByUserId { get; set; }
    }
}
