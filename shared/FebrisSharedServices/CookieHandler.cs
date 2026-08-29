// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    public interface ICookieHandler
    {
        string ThinMint { get; }
        //Task CookieJarCrusade(IHttpContextAccessor context);
        //Task CookieHandler(IHttpContextAccessor httpContextAccessor);
    }
    public class CookieHandler : ICookieHandler
    {
        public string ThinMint = string.Empty;
        public const string CookieName = "Febris.AuthCookie";
        private readonly IHttpContextAccessor _httpContextAccessor;

        string ICookieHandler.ThinMint { get => ThinMint; }

        public CookieHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _ = CookieJarCrusade(_httpContextAccessor);
        }

        private async Task CookieJarCrusade(IHttpContextAccessor input)
        {
            try
            {
                ThinMint = input.HttpContext.Request.Cookies["Febris.AuthCookie"];
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
            }
        }

        //public async Task CookieJarCrusade(HttpContext context)
        //{
        //    try
        //    {
        //        ThinMint = context.Request.Cookies["Febris.AuthCookie"];
        //    }
        //    catch (Exception ex)
        //    {
        //        Febris.SharedServices.FebrisLog.Error(ex);
        //    }
        //}

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    try
        //    {
        //        ThinMint = context.Request.Cookies["Febris.AuthCookie"];
        //    }
        //    catch (Exception ex)
        //    {
        //        Febris.SharedServices.FebrisLog.Error(ex);
        //    }
        //    //await next();
        //}


        //public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        //{
        //    try
        //    {
        //        ThinMint = context.Request.Cookies["Febris.AuthCookie"];                
        //    }
        //    catch (Exception ex)
        //    {
        //        Febris.SharedServices.FebrisLog.Error(ex);
        //    }
        //    //await next();
        //}
    }


}
