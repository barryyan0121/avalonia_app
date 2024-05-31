using System.Collections.ObjectModel;
using AvaloniaApplication.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace UnitTest;

[TestFixture]
public class ChartDataGeneratorTests
{
    [Test]
    public void GenerateLineSeries_ShouldGenerateCorrectSeries()
    {
        // Arrange
        var series = new ObservableCollection<ISeries>();
        var data = new List<ObservableCollection<ObservableValue>>
        {
            new() { new ObservableValue(1), new ObservableValue(2) },
            new() { new ObservableValue(3), new ObservableValue(4) }
        };
        var labels = new List<string> { "Label1", "Label2" };

        // Act
        ChartDataGenerator.GenerateLineSeries(series, data, labels);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(series, Has.Count.EqualTo(2));
            Assert.That(series[0], Is.InstanceOf<LineSeries<ObservableValue>>());
            Assert.That(series[0].Name, Is.EqualTo("Label1"));
            Assert.That(series[0].Values, Is.EqualTo(data[0]));
            Assert.That(series[1].Name, Is.EqualTo("Label2"));
            Assert.That(series[1].Values, Is.EqualTo(data[1]));
        });
    }

    [Test]
    public void GenerateHourlyColumnSeries_ShouldGenerateCorrectSeries()
    {
        // Arrange
        var series = new ISeries[2];
        var keyValuePair =
            new KeyValuePair<ObservableCollection<ObservableValue>, ObservableCollection<ObservableValue>>(
                new ObservableCollection<ObservableValue> { new(5), new(6) },
                new ObservableCollection<ObservableValue> { new(7), new(8) }
            );

        // Act
        ChartDataGenerator.GenerateHourlyColumnSeries(series, keyValuePair);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(series[0], Is.InstanceOf<ColumnSeries<ObservableValue>>());
            Assert.That(series[0].Name, Is.EqualTo("合格"));
            Assert.That(series[0].Values, Is.EqualTo(keyValuePair.Key));
            Assert.That(series[1], Is.InstanceOf<ColumnSeries<ObservableValue>>());
            Assert.That(series[1].Name, Is.EqualTo("不合格"));
            Assert.That(series[1].Values, Is.EqualTo(keyValuePair.Value));
        });
    }

    [Test]
    public void GeneratePieCharts_ShouldGenerateCorrectSeries()
    {
        // Arrange
        var pieSeries = new ISeries[2][];
        var names = new List<string> { "Name1", "Name2" };
        var map = new Dictionary<string, KeyValuePair<ObservableValue, ObservableValue>>
        {
            {
                "Name1",
                new KeyValuePair<ObservableValue, ObservableValue>(new ObservableValue(0.8), new ObservableValue(0.2))
            },
            {
                "Name2",
                new KeyValuePair<ObservableValue, ObservableValue>(new ObservableValue(0.6), new ObservableValue(0.4))
            }
        };

        // Act
        ChartDataGenerator.GeneratePieCharts(pieSeries, names, map);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(pieSeries, Has.Length.EqualTo(2));

            var series0 = pieSeries[0].ToArray();
            Assert.That(series0[0], Is.InstanceOf<PieSeries<ObservableValue>>());
            Assert.That(series0[0].Name, Is.EqualTo("合格"));
            Assert.That(series0[0].Values!.Cast<ObservableValue>().First().Value, Is.EqualTo(0.8));
            Assert.That(series0[1], Is.InstanceOf<PieSeries<ObservableValue>>());
            Assert.That(series0[1].Name, Is.EqualTo("不合格"));
            Assert.That(series0[1].Values!.Cast<ObservableValue>().First().Value, Is.EqualTo(0.2));

            var series1 = pieSeries[1].ToArray();
            Assert.That(series1[0], Is.InstanceOf<PieSeries<ObservableValue>>());
            Assert.That(series1[0].Name, Is.EqualTo("合格"));
            Assert.That(series1[0].Values!.Cast<ObservableValue>().First().Value, Is.EqualTo(0.6));
            Assert.That(series1[1], Is.InstanceOf<PieSeries<ObservableValue>>());
            Assert.That(series1[1].Name, Is.EqualTo("不合格"));
            Assert.That(series1[1].Values!.Cast<ObservableValue>().First().Value, Is.EqualTo(0.4));
        });
    }

    [Test]
    public void GenerateGaugeSeries_ShouldGenerateCorrectSeries()
    {
        // Arrange
        var series = new IEnumerable<ISeries>[2];
        var names = new List<string> { "Gauge1", "Gauge2" };
        var map = new Dictionary<string, ObservableValue>
        {
            { "Gauge1", new ObservableValue(0.5) },
            { "Gauge2", new ObservableValue(0.75) }
        };

        // Act
        ChartDataGenerator.GenerateGaugeSeries(series, names, map);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(series[0].Count(), Is.EqualTo(2));
            Assert.That(series[1].Count(), Is.EqualTo(2));
            var gaugeSeries0 = series[0].First().Values!.Cast<ObservableValue>().ToArray();
            var gaugeSeries1 = series[1].First().Values!.Cast<ObservableValue>().ToArray();
            Assert.That(gaugeSeries0[0].Value, Is.EqualTo(0.5));
            Assert.That(gaugeSeries1[0].Value, Is.EqualTo(0.75));
        });
    }

    [Test]
    public void GenerateRowSeries_ShouldGenerateCorrectSeries()
    {
        // Arrange
        var series = new ISeries[1];
        var data = new[]
        {
            new ProgressInfo { Name = "A", Value = 20, Paint = new SolidColorPaint(SKColors.Red) },
            new ProgressInfo { Name = "B", Value = 50, Paint = new SolidColorPaint(SKColors.Blue) }
        };

        // Act
        ChartDataGenerator.GenerateRowSeries(series, data);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(series[0], Is.InstanceOf<RowSeries<ProgressInfo>>());
            var rowSeries = (RowSeries<ProgressInfo>)series[0];
            var rowSeriesValues = rowSeries.Values!.ToArray();
            Assert.That(rowSeriesValues, Has.Length.EqualTo(data.Length));
            Assert.That(rowSeriesValues[0].Name, Is.EqualTo(data[0].Name));
            Assert.That(rowSeriesValues[0].Value, Is.EqualTo(data[0].Value));
            Assert.That(rowSeriesValues[0].Paint, Is.EqualTo(data[0].Paint));
            Assert.That(rowSeriesValues[1].Name, Is.EqualTo(data[1].Name));
            Assert.That(rowSeriesValues[1].Value, Is.EqualTo(data[1].Value));
            Assert.That(rowSeriesValues[1].Paint, Is.EqualTo(data[1].Paint));
        });
    }

    [Test]
    public void GetLastSevenDays_ShouldReturnCorrectDates()
    {
        // Arrange
        var today = new DateTime(2023, 5, 30); // Use a fixed date for predictable results
        var expectedDates = new[]
        {
            "5/24", "5/25", "5/26", "5/27", "5/28", "5/29", "5/30"
        };

        // Act
        var dates = ChartDataGenerator.GetLastSevenDays(today);

        // Assert
        CollectionAssert.AreEqual(expectedDates, dates);
    }

    [Test]
    public void GetHours_ShouldReturnCorrectHours()
    {
        // Act
        var hours = ChartDataGenerator.GetHours();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(hours, Has.Length.EqualTo(24));
            Assert.That(hours[0], Is.EqualTo("00:00"));
            Assert.That(hours[23], Is.EqualTo("23:00"));
        });
    }
}