// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// AdminPortal Switchboard widget viewmodels (2026-05-21).
    /// <para>
    /// Phase-2 surfacing for the dashboard: instead of the old
    /// trend-chart-only mix, several widgets render small status
    /// snapshots (pipeline by stage, weighted forecast, moderation
    /// queue counts, the signed-in user's "today" view). These DTOs
    /// keep the BLL/partial contract explicit + testable without
    /// touching the existing <c>GenericMixedChart</c> shape.
    /// </para>
    /// <para>
    /// Each maps 1:1 to a <c>SwitchboardController.Load*Widget</c>
    /// action and a Razor partial in <c>Views/Widget/</c> -- see the
    /// XML doc on each Load action for the wiring details.
    /// </para>
    /// </summary>
    public class SwitchboardPipelineWidget
    {
        public List<PipelineStageDisplayRow> Rows { get; set; } = new List<PipelineStageDisplayRow>();
        public int TotalOpenCount { get; set; }
        public decimal TotalOpenValue { get; set; }
        public decimal TotalWeighted { get; set; }
    }

    public class PipelineStageDisplayRow
    {
        public string StageLabel { get; set; }
        public int Count { get; set; }
        public decimal SumAmount { get; set; }
        public int BarPercent { get; set; }
    }

    public class SwitchboardForecastWidget
    {
        public List<ForecastMonthDisplayRow> Months { get; set; } = new List<ForecastMonthDisplayRow>();
        public decimal TotalGross { get; set; }
        public decimal TotalWeighted { get; set; }
        public decimal TotalCommitted { get; set; }
    }

    public class ForecastMonthDisplayRow
    {
        public string Label { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal WeightedAmount { get; set; }
        public decimal CommittedAmount { get; set; }
        public int BarPercent { get; set; }
    }

    public class SwitchboardModerationWidget
    {
        public int PendingDevelopers { get; set; }
        public int OpenPurchaseDisputes { get; set; }
        public int DuplicateLeadClusters { get; set; }
        public int Total
        {
            get { return PendingDevelopers + OpenPurchaseDisputes + DuplicateLeadClusters; }
        }
    }

    public class SwitchboardMyTodayWidget
    {
        public string DisplayName { get; set; }
        public int OpenTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int TasksDueToday { get; set; }
        public int MyOpenOpportunities { get; set; }
        public decimal MyOpenOpportunityValue { get; set; }
        public decimal MyWeighted30Day { get; set; }
    }
}
