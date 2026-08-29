// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Caching.Distributed;
// NET8 Wave 1: Microsoft.Extensions.Caching.Redis (2.2.0) is deprecated with no net8
// line. Microsoft.Extensions.Caching.StackExchangeRedis carries the same RedisCache /
// RedisCacheOptions types (same names, same IOptions<RedisCacheOptions> ctor shape),
// so the three Distributed*Cache subclasses below are unchanged beyond this using.
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


/// <summary>
/// Generics used for putting data into and taking data out of a Redis cache
/// </summary>
namespace Febris.SharedServices
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetRecord<T>(this IDistributedCache cache,
            string recordId,
            T data,
            TimeSpan? absoluteExpiredTime = null,
            TimeSpan? unusedExpiredTime = null)
        {
            var options = new DistributedCacheEntryOptions() 
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiredTime ?? TimeSpan.FromSeconds(60),
                SlidingExpiration = unusedExpiredTime
            };

            //options.AbsoluteExpirationRelativeToNow = absoluteExpiredTime ?? TimeSpan.FromSeconds(60);
            //options.SlidingExpiration = unusedExpiredTime;

            var jsonData = JsonSerializer.Serialize(data);           
            await cache.SetStringAsync(recordId, jsonData, options);            
        }


        public static async Task<T> GetRecord<T>(this IDistributedCache cache,
            string recordId)
        {
            var jsonData = await cache.GetStringAsync(recordId);

            if (jsonData is null)
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(jsonData);
        }      
    }

    public interface IDistributedLicenseCache : IDistributedCache
    {
    }
    public interface IDistributedUserCache : IDistributedCache
    {
    }
    public interface IDistributedHardwareCache : IDistributedCache
    {
    }
    public class DistributedUserCache : RedisCache, IDistributedUserCache
    {
        //private const string KeyPrefix = "FebrisAuthCookie-";
        public DistributedUserCache(IOptions<RedisCacheOptions> optionsAccessor) : base(optionsAccessor) {}
    }
    public class DistributedLicenseCache : RedisCache, IDistributedLicenseCache
    {
        //private const string KeyPrefix = "FebrisLicenseToken-";
        public DistributedLicenseCache(IOptions<RedisCacheOptions> optionsAccessor) : base(optionsAccessor) { }
    }
    public class DistributedHardwareCache : RedisCache, IDistributedHardwareCache
    {
        //private const string KeyPrefix = "FebrisHardwareToken-";
        public DistributedHardwareCache(IOptions<RedisCacheOptions> optionsAccessor) : base(optionsAccessor) { }
    }
}
