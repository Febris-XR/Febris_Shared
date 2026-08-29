// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
// NET8 Wave 1: deprecated Caching.Redis -> StackExchangeRedis. Same RedisCacheOptions
// type name; it still implements IOptions<RedisCacheOptions> (self-options pattern),
// so the RedisCacheOptions ctor below keeps compiling unchanged. This file is the
// heart of the platform's auth model (cookies-as-tickets) -- behavior verified by the
// Wave-4 auth smoke gate + the SharedServicesTests flip in Wave 5.
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    public class RedisCacheTicketStore : ITicketStore
    {
        /// <summary>
        /// https://mikerussellnz.github.io/.NET-Core-Auth-Ticket-Redis/
        /// There are still a few more changes needed ie. storage
        /// </summary>
        #region Found solution

        private const string KeyPrefix = "FebrisAuthCookie-";
        // private readonly IDistributedCache _cache;
        private readonly IDistributedUserCache _cache;
        public RedisCacheTicketStore(RedisCacheOptions cache)
        {
            //_cache = new RedisCache(cache);
            _cache = new DistributedUserCache(cache);
        }
        //public RedisCacheUserTicketStore(IDistributedCache cache)
        public RedisCacheTicketStore(IDistributedUserCache cache)
        {
            _cache = cache;
        }

        #region Get
        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            AuthenticationTicket output;
            try
            {
                byte[] bytes = null;
                bytes = _cache.Get(key);
                output = DeserializeFromBytes(bytes);
                return Task.FromResult(output);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                // FIX (SSO-B5): return a completed Task wrapping null, not a null Task, so the cookie-auth handler degrades to anonymous instead of throwing NRE.
                // return null;
                return Task.FromResult<AuthenticationTicket>(null);
            }
        }
        #endregion

        #region Create
        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            string key = string.Empty;
            try
            {
                key = KeyPrefix + Guid.NewGuid().ToString();
                await RenewAsync(key, ticket);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
            }
            return key;
        }
        #endregion

        #region Update
        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            try
            {
                DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
                var expiresUtc = ticket.Properties.ExpiresUtc;
                if (expiresUtc.HasValue)
                {
                    options.SetAbsoluteExpiration(expiresUtc.Value);
                }
                byte[] val = SerializeToBytes(ticket);
                _cache.Set(key, val, options);
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                // FIX (SSO-B5): return a completed Task, not a null Task, so awaiting RenewAsync on a Redis fault does not throw NRE inside auth.
                // return null;
                return Task.CompletedTask;
            }
        }
        #endregion

        #region Delete
        public Task RemoveAsync(string key)
        {
            try
            {
                _cache.Remove(key);
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
            }
            // FIX (SSO-B5): return a completed Task, not a null Task, so awaiting RemoveAsync after a swallowed Redis fault does not throw NRE inside auth.
            // return null;
            return Task.CompletedTask;
        }
        #endregion

        #region Helpers
        private static byte[] SerializeToBytes(AuthenticationTicket source)
        {
            return TicketSerializer.Default.Serialize(source);
        }

        private static AuthenticationTicket DeserializeFromBytes(byte[] source)
        {
            return source == null ? null : TicketSerializer.Default.Deserialize(source);
        }
        #endregion

        #endregion                
    }

    //public interface IRedisCacheLicenseTicketStore
    //{

    //}

    //public class RedisCacheLicenseTicketStore : IRedisCacheLicenseTicketStore //: ITicketStore
    //{
    //    /// <summary>
    //    /// https://mikerussellnz.github.io/.NET-Core-Auth-Ticket-Redis/
    //    /// There are still a few more changes needed ie. storage
    //    /// </summary>
    //    #region Found solution

    //    private const string KeyPrefix = "FebrisLicenseCookie-";
    //    private readonly IDistributedCache _cache;
    //    public RedisCacheLicenseTicketStore(RedisCacheOptions cache)
    //    {
    //        _cache = new RedisCache(cache);
    //    }
    //    public RedisCacheLicenseTicketStore(IDistributedCache cache)
    //    {
    //        _cache = cache;
    //    }

    //    #region Get
    //    public Task<AuthenticationTicket> RetrieveAsync(string key)
    //    {
    //        AuthenticationTicket output;
    //        try
    //        {
    //            byte[] bytes = null;
    //            bytes = _cache.Get(key);
    //            output = DeserializeFromBytes(bytes);
    //            return Task.FromResult(output);
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }
    //    #endregion

    //    #region Create
    //    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    //    {
    //        string key = string.Empty;
    //        try
    //        {
    //            key = KeyPrefix + Guid.NewGuid().ToString();
    //            await RenewAsync(key, ticket);
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //        return key;
    //    }
    //    #endregion

    //    #region Update
    //    public Task RenewAsync(string key, AuthenticationTicket ticket)
    //    {
    //        try
    //        {
    //            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
    //            var expiresUtc = ticket.Properties.ExpiresUtc;
    //            if (expiresUtc.HasValue)
    //            {
    //                options.SetAbsoluteExpiration(expiresUtc.Value);
    //            }
    //            byte[] val = SerializeToBytes(ticket);
    //            _cache.Set(key, val, options);
    //            return Task.FromResult(0);
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }
    //    #endregion

    //    #region Delete
    //    public Task RemoveAsync(string key)
    //    {
    //        try
    //        {
    //            _cache.Remove(key);
    //            return Task.FromResult(0);
    //        }
    //        catch (Exception ex)
    //        {
    //            Febris.SharedServices.FebrisLog.Error(ex);
    //        }
    //        return null;
    //    }
    //    #endregion

    //    #region Helpers
    //    private static byte[] SerializeToBytes(AuthenticationTicket source)
    //    {
    //        return TicketSerializer.Default.Serialize(source);
    //    }

    //    private static AuthenticationTicket DeserializeFromBytes(byte[] source)
    //    {
    //        return source == null ? null : TicketSerializer.Default.Deserialize(source);
    //    }
    //    #endregion

    //    #endregion                
    //}
}

