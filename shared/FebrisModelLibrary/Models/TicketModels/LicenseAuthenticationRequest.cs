// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;


/// <summary>
/// These are the models for license use for server to server communication
/// </summary>
namespace Febris.ModelLibrary.Models.TicketModels
{
    public class LicenseAuthenticationRequest: BaseAuthenticationRequest
    {
        [Required]
        public Guid LicenseKey { get; set; }
    }

    public class LicenseAuthenticateResponse: BaseAuthenticateResponse
    {

        //public string JwtToken { get; set; }

        //[JsonIgnore] // refresh token is returned in http only cookie
        //public string RefreshToken { get; set; }

        public LicenseAuthenticateResponse(string jwtToken, string refreshToken)
        {            
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }

    public class RefreshLicenseToken: BaseRefreshLicenseToken
    {
        //[Key]
        //[JsonIgnore]
        //public int Id { get; set; }

        //public string Token { get; set; }
        //public DateTime Expires { get; set; }
        //public bool IsExpired => DateTime.UtcNow >= Expires;
        //public DateTime Created { get; set; }
        //public string CreatedByIp { get; set; }
        //public DateTime? Revoked { get; set; }
        //public string RevokedByIp { get; set; }
        //public string ReplacedByToken { get; set; }
        //public bool IsActive => Revoked == null && !IsExpired;
    }






}
