using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AvaloniaApplication.Models;

public static class ChartDataGenerator
{
    public static void GenerateLineSeries(ISeries[] seriesArray, List<List<double>> data, List<string> labels)
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
                    DataLabelsSize = 10,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsPosition = DataLabelsPosition.Top,
                    Fill = null,
                    GeometrySize = 8,
                    LineSmoothness = 0
                };

                seriesArray[i] = lineSeries;
            }
    }

    public static void GeneratePieCharts(ISeries[][] seriesArray, List<string> names,
        Dictionary<string, KeyValuePair<ObservableValue, ObservableValue>> map)
    {
        for (var i = 0; i < seriesArray.Length; i++)
        {
            var series = seriesArray[i];
            var name = names[i];
            if (!map.TryGetValue(name, out var pair)) continue;
            var rate1 = pair.Key;
            var rate2 = pair.Value;
            series[0] = new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { rate1 },
                Name = "合格",
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 20,
                DataLabelsPosition = PolarLabelsPosition.ChartCenter,
                DataLabelsFormatter = point => (point.Coordinate.PrimaryValue * 100).ToString("0.0") + "% 合格"
            };
            series[1] = new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { rate2 },
                Name = "不合格"
            };
        }
    }

    public static void GenerateGaugeSeries(IEnumerable<ISeries>[] seriesArray, List<string> names,
        Dictionary<string, ObservableValue> map)
    {
        for (var i = 0; i < seriesArray.Length; i++)
        {
            var name = names[i];
            if (!map.TryGetValue(name, out var rate)) continue;

            seriesArray[i] = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(rate, s =>
                {
                    s.MaxRadialColumnWidth = 40;
                    s.DataLabelsSize = 20;
                    s.DataLabelsPaint = new SolidColorPaint(SKColors.White);
                    s.DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("0.0") + "%";
                }));
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