// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    /// <summary>
    /// 
    /// </summary>
    public static class CronExpression
    {
        public static readonly string EverySecond = "* * * * * *";
        public static readonly string EveryMinute = "0/1 * * * *";
        public static readonly string EveryFiveMinutes = "*/5 * * * *";
        public static readonly string EveryTenMinutes = "*/10 * * * *";
        public static readonly string EveryFifteenMinutes = "*/15 * * * *";
        public static readonly string EveryTwentyMinutes = "*/20 * * * *";
        public static readonly string EveryHour = "0 * * * *";
        public static readonly string EveryDay = "0 0 * * *";
        public static readonly string EveryWeek = "0 0 * * 0";
        public static readonly string EveryMonth = "0 0 1 * *";
    }
    public class CronExpressionEvaluator
    {
        public static async Task<DateTime> GetNextOccurrence(string input)
        {            
            try
            {
                CrontabSchedule schedule = CrontabSchedule.Parse(input);
                DateTime nextOccurrence = schedule.GetNextOccurrence(DateTime.UtcNow);
                return nextOccurrence;
            }
            catch (System.Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "CronExpressionEvaluator.GetNextOccurrence: suppressed exception");
                return DateTime.UtcNow;
            }
        }
        /// <summary>
        /// Takes in the last time stamp and returns the last expected run date was so it can be health checked
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static async Task<DateTime> GetExpectedRunTimeBasedOnLastRun(DateTime givenDate, string exp)
        {
            try
            {
                CrontabSchedule schedule = CrontabSchedule.Parse(exp);
                DateTime lastRunTime = schedule.GetNextOccurrence(givenDate);
                return lastRunTime;
            }
            catch (System.Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "CronExpressionEvaluator.GetExpectedRunTimeBasedOnLastRun: suppressed exception");
                return default;
            }
        }
    }
}
