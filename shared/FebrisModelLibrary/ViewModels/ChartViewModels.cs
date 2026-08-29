// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class ChartViewModels
    {
    }
    #region Line chart
    public class LineChart
    {
        public string Title { get; set; }
        public string IdToUse { get; set; }
        public string Description { get; set; }
        public List<LineChartEntry> ChartEntryList { get; set; }
    }
    public class LineChartEntry
    {
        public string Label { get; set; }
        public int Quantity { get; set; }
    }
    #endregion
    #region Radar chart
    public class RadarChart
    {
        public string Title { get; set; }
        public string IdToUse { get; set; }
        public string Description { get; set; }
        public List<RadarChartEntry> ChartEntryList { get; set; }
    }
    public class RadarChartEntry
    {
        public string Label { get; set; }
        public int Quantity { get; set; }
    }
    #endregion
    #region Pie chart
    public class PieChart
    {
        public string Title { get; set; }
        public string IdToUse { get; set; }
        public string Description { get; set; }
        public List<PieChartEntry> ChartEntryList { get; set; }
    }
    public class PieChartEntry
    {
        public string Label { get; set; }
        public int Quantity { get; set; }
    }
    #endregion
    #region Bar chart
    public class BarChart
    {
        public string Title { get; set; }
        public string IdToUse { get; set; }
        public string Description { get; set; }
        public List<BarChartEntry> ChartEntryList { get; set; }
    }
    public class BarChartEntry
    {
        public string Label { get; set; }
        public int Quantity { get; set; }
    }
    #endregion
    #region Bubble chart
    public class BubbleChart
    {
        public string Title { get; set; }
        public string IdToUse { get; set; }
        public string Description { get; set; }
        public List<BubbleChartEntry> ChartEntryList { get; set; }
    }
    public class BubbleChartEntry
    {
        public string Label { get; set; }
        public int VariableA { get; set; }
        public int VariableB { get; set; }
        public int VariableC { get; set; }
    }
    #endregion


    public class WorldMapChart
    {
        public string Title { get; set; }
        public string IdToUse { get; set; }
        public string Description { get; set; }
        public List<WorldMapChartEntry> ChartEntryList { get; set; }
        

    }
    public class WorldMapChartEntry
    {
        public long Reference { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public int Radius { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ToolTip { get; set; }
    }
    public class GeoDataUrls
    {        
        public string TileServerAPIUrl { get; set; }
        public string GeoCoderServerAPIUrl { get; set; }
    }


    #region Generic Chart
    // ChartType enum moved to Febris.EnumLibrary per the "all enums live in FebrisEnumLibrary" rule.
    public class GenericMixedChart
    {
        public GenericMixedChart()
        {
            Title = "Generic Mixed Chart";
            IdToUse = Guid.NewGuid().ToString().Replace("-", string.Empty);
            Description = "An Error Occured When Building This Chart";
            GenericChartList = new List<GenericChart>();
        }
        public string Title { get; set; }
        public string IdToUse { get; set; }
        public string Description { get; set; }
        public List<GenericChart> GenericChartList { get; set; }

    }
    public class GenericChart
    {
        public GenericChart()
        {
            Subtitle = "Qty";
            SubIdToUse = Guid.NewGuid().ToString().Replace("-", string.Empty);
            ChartType = ChartType.none;
            GenericChartEntryList = new List<GenericChartEntry>();
        }
        public string Subtitle { get; set; }
        public string SubIdToUse { get; set; }
        public ChartType ChartType { get; set; }
        public List<GenericChartEntry> GenericChartEntryList { get; set; }
    }
    public class GenericChartEntry
    {
        public GenericChartEntry()
        {
            Label = string.Empty;
            Quantity = 0;
        }
        public string Label { get; set; }
        public int Quantity { get; set; }
    }
    #endregion
}
