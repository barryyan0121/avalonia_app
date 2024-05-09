using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;

namespace AvaloniaApplication.Models;

public static class ChartDataGenerator
{
    public static readonly List<string> ProductionLinesA =
    [
        "胶纸切割",
        "板框焊接",
        "板组件A",
        "板组件B",
        "膜框组件A",
        "膜框组件B"
    ];

    public static readonly List<string> ProductionLinesB =
    [
        "三合一电池A",
        "三合一电池B",
        "三合一电池C",
        "三合一电池检测",
        "总装线",
        "框膜组件检测"
    ];

    public static void GenerateSeries(ISeries[] seriesArray, List<List<double>> data, List<string> labels)
    {
        for (var i = 0; i < data.Count; i++)
            // 如果给定的 seriesArray 已经初始化，则更新对应索引处的 ISeries 对象的 Values 属性
            if (seriesArray[i] is LineSeries<double> line)
            {
                line.Values = new ObservableCollection<double>(data[i]);
            }

            else
            {
                // 如果给定的 seriesArray 还未初始化，或者索引超出了数组长度，则创建新的 ISeries 对象并添加到数组中
                var lineSeries = new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(data[i]),
                    Name = labels[i],
                    DataLabelsPosition = DataLabelsPosition.Top,
                    Fill = null
                };

                seriesArray[i] = lineSeries;
            }
    }

    public static string[] GetLastSevenDays()
    {
        var today = DateTime.Today;
        var dates = new string[7];

        for (var i = 0; i < 7; i++) dates[6 - i] = today.AddDays(-i).ToString("M/d");

        return dates;
    }
}