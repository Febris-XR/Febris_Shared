// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Febris.ModelLibrary.Models.TicketModels
{
    public class HardwareAuthenticationRequest: BaseAuthenticationRequest
    {
        [Required]
        public string LicenseKey { get; set; }
    }

    public class HardwareAuthenticationResponse: BaseAuthenticateResponse
    {

        //public string JwtToken { get; set; }

        //[JsonIgnore] // refresh token is returned in http only cookie
        //public string RefreshToken { get; set; }

        public HardwareAuthenticationResponse(string jwtToken, string refreshToken)
        {            
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }

    public class RevokeHardwareTokenRequest: BaseRevokeTokenRequest
    {
        //public string Token { get; set; }
    }

    //public class RefreshHardwareToken
    //{
    //    public string? JwtToken { get; set; }
    //    public string? RefreshToken { get; set; }
    //}

    public class RefreshHardwareToken : BaseRefreshLicenseToken
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
