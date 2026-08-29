// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.MarketingModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.AnalyticsModels
{
    public abstract class AnalyticsBaseModel:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }
        public TimeSpan? TimeSpan { get; set; }


        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string Query { get; set; }
        public string Referer { get; set; }
        public string Path { get; set; }
        public string SourceId { get; set; }

        public int? Visits { get; set; }


        //connection to GeoIp
        public long GeoIPDataId { get; set; }
    }
    public class Analytics : AnalyticsBaseModel
    {
    }
    public class AdminAnalytics : AnalyticsBaseModel
    {
    }
    public class MarketingAnalytics : AnalyticsBaseModel
    {
    }
    public class MarketplaceAnalytics : AnalyticsBaseModel
    {
        public string AccessedThrough { get; set; }
    }
    public class DeveloperAnalytics : AnalyticsBaseModel
    {
    }
    public class UserAnalytics : AnalyticsBaseModel
    {
    }
    public class SharedApiAnalytics : AnalyticsBaseModel
    {
    }
    public class LocalAnalytics : AnalyticsBaseModel
    {
    }

    public class AnalyticsViewModel
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }

        public DateTime TimeStamp { get; set; }
        public DateTime UpdateTimeStamp { get; set; }
        public TimeSpan TimeSpan { get; set; }

        //visitor
        public string IPAddress { get; set; }
        public int Visits { get; set; }

        //pages
        public string Source { get; set; }
        public string LandingPage { get; set; }
        public string ExitPage { get; set; }

        //location
        public string City { get; set; }
        public string Country { get; set; }

        //user device
        public string Browser { get; set; }
        public string OS { get; set; }
        public string Device { get; set; }

        //recording data
        public string RecordingPath { get; set; }
        public bool RecordingWatched { get; set; }

        //userinput recorded
        public bool UserInput { get; set; }

        //add tags
        public string Tag { get; set; }


    }

    
}
