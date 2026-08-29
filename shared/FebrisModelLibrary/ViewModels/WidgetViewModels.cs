// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.XApiModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class WidgetViewModels
    {
    }
    public class GeneralWidgetViewModel
    {
        public int? TotalCount { get; set; }
        public int? CurrentCount { get; set; }
        public int? TotalTrainingCount { get; set; }
        public int? CurrentTrainingCount { get; set; }
        public DateTime DateTime { get; set; }
        public double? TotalTimeCount { get; set; }
        public double? CurrentTimeCount { get; set; }
        public double? TotalTrainingTimeCount { get; set; }
        public double? CurrentTrainingTimeCount { get; set; }
    }
    public class PieChartViewModel
    {
        public int TotalCount { get; set; }
        public int PositiveCount { get; set; }
        public int NegativeCount { get; set; }
        public int NeutralCount { get; set; }
        public int NeutralCount2 { get; set; }
        public int NeutralCount3 { get; set; }
        public string PositiveLabel { get; set; }
        public string NegativeLabel { get; set; }
        public string NeutralLabel { get; set; }
        public string NeutralLabel2 { get; set; }
        public string NeutralLabel3 { get; set; }

    }
    public class RadarChartViewModel
    {
        public float ScoreAverage { get; set; }
        public double DurationAverage { get; set; }
        public double RestartCountAverage { get; set; }
        public double SuccessCountAverage { get; set; }
        public double CompletionCountAverage { get; set; }
        public int TimeEstimate { get; set; }
    }
    public class ComparisonRadarChartViewModel
    {
        public Statement Statement { get; set; }
        public float ScoreAverage { get; set; }
        public double DurationAverage { get; set; }
        public double RestartCountAverage { get; set; }
        public double SuccessCountAverage { get; set; }
        public double CompletionCountAverage { get; set; }
        public int TimeEstimate { get; set; }
    }


    public class BackgroundTaskStatusResponseViewModel
    {
        public DateTime LastRestart { get; set; }
        public int ErrorsLogged { get; set; }
        public DateTime LastLogCheck { get; set; }
        public DateTime ByTheMinuteCheck { get; set; }
        public bool ByTheMinuteUpToDate { get; set; }
        public DateTime LastHourlyCheck { get; set; }
        public bool HourlyUpToDate { get; set; }
        public DateTime LastDailyCheck { get; set; }
        public bool DailyUpToDate { get; set; }
        public DateTime LastWeeklyCheck { get; set; }
        public bool WeeklyUpToDate { get; set; }
        public DateTime LastMonthlyCheck { get; set; }
        public bool MonthlyUpToDate { get; set; }        
    }
    public class ManualTriggerResponse
    {        
        public bool Success { get; set; }
        public int Errors { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
    public class SavedBackgroundTaskStatusViewModel
    {
        public SavedBackgroundTaskStatusViewModel()
        {
            TimeStamp = DateTime.UtcNow;
        }
        public DateTime TimeStamp { get; set; }
        public DateTime LastRestart { get; set; }
        public int ErrorsLogged { get; set; }
        public DateTime LastLogCheck { get; set; }
        public DateTime ByTheMinuteCheck { get; set; }
        public bool ByTheMinuteUpToDate { get; set; }
        public DateTime LastHourlyCheck { get; set; }
        public bool HourlyUpToDate { get; set; }
        public DateTime LastDailyCheck { get; set; }
        public bool DailyUpToDate { get; set; }
        public DateTime LastWeeklyCheck { get; set; }
        public bool WeeklyUpToDate { get; set; }
        public DateTime LastMonthlyCheck { get; set; }
        public bool MonthlyUpToDate { get; set; }
    }
}
