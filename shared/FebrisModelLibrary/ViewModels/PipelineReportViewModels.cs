// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// CRM Phase 2 Tier 2.1 (2026-05-21): DTOs returned by the pipeline
    /// report BLL. Plain POCOs (not EF entities) so they can be
    /// serialized, cached, or unit-tested without DAL.
    /// <para>
    /// All currency values are in <c>USD</c> (matches the per-deal
    /// <c>Opportunity.Currency</c> default). Multi-currency rollups will
    /// require either a conversion-rate step or per-currency
    /// sub-reports -- both deferred to a future iteration.
    /// </para>
    /// </summary>
    public class PipelineByStageReport
    {
        public List<PipelineStageRow> Rows { get; set; } = new List<PipelineStageRow>();
        public decimal TotalOpenValue { get; set; }
        public int TotalOpenCount { get; set; }
    }

    public class PipelineStageRow
    {
        public DealStage Stage { get; set; }
        public int Count { get; set; }
        public decimal SumAmount { get; set; }
        /// <summary>Sum of Amount * StageProbability / 100 (weighted contribution).</summary>
        public decimal WeightedAmount { get; set; }
    }

    public class ForecastByMonthReport
    {
        public List<ForecastMonthRow> Months { get; set; } = new List<ForecastMonthRow>();
        public decimal TotalGross { get; set; }
        public decimal TotalWeighted { get; set; }
        /// <summary>
        /// Sum of Amount across opportunities with StageProbability &gt;= 80.
        /// "Committed" is the high-confidence band the sales team would
        /// stake the quarter on.
        /// </summary>
        public decimal TotalCommitted { get; set; }
    }

    public class ForecastMonthRow
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal WeightedAmount { get; set; }
        public decimal CommittedAmount { get; set; }
        /// <summary>"2026-06" -- convenience for axis labels.</summary>
        public string Label { get { return Year.ToString("0000") + "-" + Month.ToString("00"); } }
    }

    public class WinRateReport
    {
        /// <summary>Overall win rate across the date range (0..1).</summary>
        public decimal OverallWinRate { get; set; }
        public int TotalWon { get; set; }
        public int TotalLost { get; set; }
        public decimal TotalWonValue { get; set; }
        public decimal TotalLostValue { get; set; }
        public List<WinRateBreakdownRow> ByRep { get; set; } = new List<WinRateBreakdownRow>();
        public List<WinRateBreakdownRow> ByLossReason { get; set; } = new List<WinRateBreakdownRow>();
    }

    public class WinRateBreakdownRow
    {
        public string Key { get; set; }     // rep id / reason name / ...
        public int Won { get; set; }
        public int Lost { get; set; }
        public decimal WonValue { get; set; }
        public decimal LostValue { get; set; }
        /// <summary>Won / (Won + Lost). 0 if no closed deals in the bucket.</summary>
        public decimal WinRate { get; set; }
    }

    public class SalesCycleReport
    {
        /// <summary>Mean days from Opportunity creation to ActualCloseDate, won deals only.</summary>
        public double OverallMeanDays { get; set; }
        public int SampleSize { get; set; }
        public List<SalesCycleBreakdownRow> ByRep { get; set; } = new List<SalesCycleBreakdownRow>();
        public List<SalesCycleBreakdownRow> ByDealSizeBucket { get; set; } = new List<SalesCycleBreakdownRow>();
    }

    public class SalesCycleBreakdownRow
    {
        public string Key { get; set; }     // rep id / "$0-10K" / "$10-50K" / ...
        public int SampleSize { get; set; }
        public double MeanDays { get; set; }
        public double MedianDays { get; set; }
    }

    public class LossReasonReport
    {
        public List<LossReasonRow> Rows { get; set; } = new List<LossReasonRow>();
        public int TotalLost { get; set; }
        public decimal TotalLostValue { get; set; }
    }

    public class LossReasonRow
    {
        public OpportunityLossReason Reason { get; set; }
        public int Count { get; set; }
        public decimal SumLostAmount { get; set; }
        /// <summary>Count / TotalLost (0..1).</summary>
        public decimal Share { get; set; }
    }
}
