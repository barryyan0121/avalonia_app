using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.ConditionalDraw;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AvaloniaApplication.Models;

// This class is responsible for generating data for the charts.
public static class ChartDataGenerator
{
    // 生成过去七日的产量及合格率折线图
    public static void GenerateLineSeries(ObservableCollection<ISeries> series,
        List<ObservableCollection<ObservableValue>> data, List<string> labels)
    {
        for (var i = 0; i < data.Count; i++)
        {
            series.Add(new LineSeries<ObservableValue>
            {
                Values = data[i],
                Name = labels[i],
                DataLabelsSize = 10,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsPosition = DataLabelsPosition.Top,
                Fill = null,
                GeometrySize = 8,
                LineSmoothness = 0
            });
        }
    }

    // 生成查询当日分时柱状图
    public static void GenerateHourlyColumnSeries(ISeries[] series,
        KeyValuePair<ObservableCollection<ObservableValue>, ObservableCollection<ObservableValue>> keyValuePair)
    {
        // 生成合格柱状图
        series[0] = new ColumnSeries<ObservableValue>
        {
            Values = keyValuePair.Key,
            IsVisible = true,
            Name = "合格",
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsPosition = DataLabelsPosition.Top,
        };
        // 生成不合格柱状图
        series[1] = new ColumnSeries<ObservableValue>
        {
            Values = keyValuePair.Value,
            IsVisible = true,
            Name = "不合格",
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsPosition = DataLabelsPosition.Top
        };
    }

    // 生成当日合格率饼状图
    public static void GeneratePieCharts(ISeries[][] pieSeries, List<string> names,
        Dictionary<string, KeyValuePair<ObservableValue, ObservableValue>> map)
    {
        for (var i = 0; i < pieSeries.Length; i++)
        {
            pieSeries[i] = new ISeries[2];
        }

        for (var i = 0; i < pieSeries.Length; i++)
        {
            var series = pieSeries[i];
            var name = names[i];
            if (!map.TryGetValue(name, out var pair))
            {
                continue;
            }

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

    // 生成当日生产进度仪表盘
    public static void GenerateGaugeSeries(IEnumerable<ISeries>[] series, List<string> names,
        Dictionary<string, ObservableValue> map)
    {
        for (var i = 0; i < series.Length; i++)
        {
            var name = names[i];
            if (!map.TryGetValue(name, out var rate))
            {
                continue;
            }

            var paints = ProgressInfo.Paints;

            var i1 = i;
            series[i] = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(rate, s =>
                {
                    s.MaxRadialColumnWidth = 40;
                    s.DataLabelsSize = 20;
                    s.Fill = paints[i1 % paints.Length];
                    s.DataLabelsPaint = new SolidColorPaint(SKColors.White);
                    s.DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("0.0") + "%";
                }));
        }
    }

    // 生成24小时字符串数组
    public static string[] GetHours()
    {
        // give me an array with values from 0:00 to 23:00, with the format "HH:00"
        return Enumerable.Range(0, 24).Select(x => $"{x:00}:00").ToArray();
    }

    // 生成过去七天的字符串数组
    public static string[] GetLastSevenDays()
    {
        var today = DateTime.Today;
        var dates = new string[7];

        for (var i = 0; i < 7; i++)
        {
            dates[6 - i] = today.AddDays(-i).ToString("M/d");
        }

        return dates;
    }

    // 生成当日实时生产进度条
    public static void GenerateRowSeries(ISeries[] series, ProgressInfo[] data)
    {
        var rowSeries = new RowSeries<ProgressInfo>
        {
            Values = data.OrderBy(x => x.Value).ToArray(),
            Name = "生产进度",
            DataLabelsPaint = new SolidColorPaint(new SKColor(245, 245, 245)),
            DataLabelsPosition = DataLabelsPosition.End,
            DataLabelsTranslate = new LvcPoint(-1, 0),
            DataLabelsFormatter = point => $"{point.Model!.Name} {Math.Round(point.Coordinate.PrimaryValue, 1)}%",
            DataLabelsSize = 15,
            MaxBarWidth = 80,
            Padding = 10,
            IsVisibleAtLegend = false
        }.OnPointMeasured(point =>
        {
            if (point.Visual is null)
            {
                return;
            }

            point.Visual.Fill = point.Model!.Paint;
        });
        series[0] = rowSeries;
    }
}