// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Febris.SharedServices
{
    public class Geocoder
    {
        //***********************************************************************************************************************************
        //had to make a specific constructor for linking
        //***********************************************************************************************************************************
        public Geocoder()
        {
        }

        // HIGH-7: one shared HttpClient avoids the per-call socket exhaustion of new HttpClient().
        // The only header is a constant Referer (Nominatim terms of service), set once at
        // construction, so a single shared instance is safe -- there is no per-request header
        // state to race on -- and it is never disposed (process-wide).
        private static readonly HttpClient _httpClient = CreateGeocoderClient();
        private static HttpClient CreateGeocoderClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Referer", "https://febr.is");
            return client;
        }


        //***********************************************************************************************************************************
        //Take in address, save long and lat to db
        //***********************************************************************************************************************************
        public static (double latitude, double longitude) GetGeoCodes(string streetAddress, string city, string zip, string state, string country)
        {
            //double longitude = 39.828300;
            //double latitude = -98.579500;
            double latitude = 39.828300;
            double longitude = -98.579500;
            try
            {
                IConfiguration configSettings = StaticDetails.PassedBackConfig;
                string geoCoderUrl = configSettings.GetSection("GeoDataUrls:GeoCoderServerAPIUrl").Value;

                var client = _httpClient;
                {
#if (DEBUG)

                    //var url = new UriBuilder("https://__TILE_HOST__/search?");
                    var url = new UriBuilder(geoCoderUrl);

#elif (STAGING)

                    //var url = new UriBuilder("https://nominatim.openstreetmap.org/search?");
                    var url = new UriBuilder(geoCoderUrl);

#else

                    //var url = new UriBuilder("https://nominatim.openstreetmap.org/search?");
                    var url = new UriBuilder(geoCoderUrl);
#endif

                    //var url = new UriBuilder("https://nominatim.openstreetmap.org/search?");

                    var query = HttpUtility.ParseQueryString(string.Empty);
                    query["format"] = "json";
                    query["street"] = streetAddress;
                    query["city"] = city;
                    query["postalcode"] = zip;
                    query["state"] = state;
                    query["country"] = country;
                    query["accept-language"] = "en-US";
                    query["limit"] = "1";
                    //Referer header (Nominatim terms of service) is set once on the shared client
                    //make the call
                    url.Query = query.ToString();
                    var response = client.GetAsync(url.ToString());
                    var result = response.Result.ToString();
                    //need to get conent
                    var data = response.Result.Content.ReadAsStringAsync();
                    //parse data
                    dynamic jsonObj = JValue.Parse(data.Result);
                    //var parseData = JsonConvert.DeserializeObject<object>(data.Result);                    
                    longitude = (jsonObj[0]["lon"]);
                    latitude = (jsonObj[0]["lat"]);


                    return (latitude, longitude);
                }
            }
            catch (System.Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "Geocoder.GetGeoCodes: suppressed exception");
                return (latitude, longitude);
            }

        }
    }
}
