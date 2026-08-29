// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Data gathered using mastermind's GeoIplite
/// </summary>
namespace Febris.ModelLibrary.Models.AnalyticsModels
{
    public class GeoIPData:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }


        public GeoIPByCity GeoIPByCity { get; set; }
        public GeoIPByCountry GeoIPByCountry { get; set; }
        public GeoASN GeoASN { get; set; }
    }



    public class GeoIPByCity
    {
        //network,
        //geoname_id,
        //registered_country_geoname_id,
        //represented_country_geoname_id,
        //is_anonymous_proxy,
        //is_satellite_provider,
        //postal_code,
        //latitude,
        //longitude,
        //accuracy_radius

        public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public long GeoName_Id { get; set; }
        public string Network { get; set; }                
        public long Registered_Country_GeoNameId { get; set; }
        public long Represented_Country_GeoName_Id { get; set; }
        public bool Is_Anonymous_Proxy { get; set; }
        public bool Is_Satellite_Provider { get; set; }
        public string Postal_code { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Accuracy_Radius { get; set; }

        public GeoIPByCityData GeoIPByCityData { get; set; }
        
    }
    public class GeoIPByCityData
    {
        //geoname_id,
        //locale_code,
        //continent_code,
        //continent_name,
        //country_iso_code,
        //country_name,
        //subdivision_1_iso_code,
        //subdivision_1_name,
        //subdivision_2_iso_code,
        //subdivision_2_name,
        //city_name,
        //metro_code,
        //time_zone,
        //is_in_european_union

        public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public long GeoName_Id { get; set; }
        public string Locale_Code { get; set; }

        public string Continent_Code { get; set; }
        public string Continent_Name { get; set; }

        public string Country_Iso_Code { get; set; }
        public string Country_Name { get; set; }

        public string Subdivision_1_Iso_Code { get; set; }
        public string Subdivision_1_Name { get; set; }

        public string Subdivision_2_Iso_Code { get; set; }
        public string Subdivision_2_Name { get; set; }
        
        
        public string City_Name { get; set; }
        public string Metro_Code { get; set; }
        public string Time_Zone { get; set; }


        public bool Is_In_European_Union { get; set; }
    }



    public class GeoIPByCountry
    {
        //geoname_id,
        //locale_code,
        //continent_code,
        //continent_name,
        //country_iso_code,
        //country_name,
        //is_in_european_union
 
        public long Id { get; set; }     
        public string Network { get; set; }
        public GeoIPByCountryData GeoIPByCountryData { get; set; }
        public long Registered_Country_GeoNameId { get; set; }
        public long Represented_Country_GeoName_Id { get; set; }
        public bool Is_Anonymous_Proxy { get; set; }
        public bool Is_Satellite_Provider { get; set; }
        //public string Postal_code { get; set; }
        //public double Latitude { get; set; }
        //public double Longitude { get; set; }
        //public int Accuracy_Radius { get; set; }
             
    }
    public class GeoIPByCountryData
    {
        //geoname_id,
        //locale_code,
        //continent_code,
        //continent_name,
        //country_iso_code,
        //country_name,
        //is_in_european_union

        public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public long GeoName_Id { get; set; }

        /// <summary>
        /// These can probably moved to enums
        /// </summary>
        public string Locale_Code { get; set; }
        public string Continent_Code { get; set; }
        public string Continent_Name { get; set; }
        public string Country_Iso_Code { get; set; }
        public string Country_Name { get; set; }
        public bool Is_In_European_Union { get; set; }

    }



    /// <summary>
    /// Autonomous Systems
    /// </summary>
    public class GeoASN
    {
        //network,
        //autonomous_system_number,
        //autonomous_system_organization
        public long Id { get; set; }
        //public Guid UUID { get; set; }        
        public string NetworkIPAddress { get; set; }
        public int Autonomous_System_Number { get; set; }
        public string Autonomous_System_Organization { get; set; }        
    }

}
