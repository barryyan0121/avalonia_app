using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Collections;
using AvaloniaApplication.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;

namespace AvaloniaApplication.ViewModels;

public class MainViewModel : ViewModelBase
{
    private DateTime _today = DateTime.Today;

    public MainViewModel()
    {
        LiveCharts.Configure(config =>
            config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')));
        const string connectionString =
            "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;";
        var databaseManager = new DatabaseManager(connectionString);
        RefreshData(databaseManager);
        // Start a background task to periodically check for data changes
        Task.Run(async () => { await CheckForDataChanges(databaseManager); });
    }

    public AvaloniaList<ProductionData> DailyData { get; private set; }
    public Dictionary<string, List<List<double>>> WeeklyData { get; private set; }

    public DateTime Today
    {
        get => _today;
        set => this.RaiseAndSetIfChanged(ref _today, value);
    }

    public ISeries[] TotalSeriesA { get; set; } = new ISeries[6];
    public ISeries[] TotalSeriesB { get; set; } = new ISeries[6];
    public ISeries[] RateSeriesA { get; set; } = new ISeries[6];
    public ISeries[] RateSeriesB { get; set; } = new ISeries[6];

    public Axis[] XAxes { get; set; } =
    {
        new()
        {
            Name = "生产日期",
            Labels = ChartDataGenerator.GetLastSevenDays(),
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            TextSize = 15,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }
        }
    };

    public Axis[] YProductionAxes { get; set; } =
    [
        new Axis
        {
            Name = "产量",
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            TextSize = 15,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
            {
                StrokeThickness = 2
            }
        }
    ];

    public Axis[] YRateAxes { get; set; } =
    [
        new Axis
        {
            Name = "合格率 (%)",
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            TextSize = 15,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
            {
                StrokeThickness = 2
            }
        }
    ];

    public SolidColorPaint LegendTextPaint { get; set; } = new()
    {
        Color = new SKColor(255, 255, 255)
    };

    private async Task CheckForDataChanges(DatabaseManager databaseManager)
    {
        while (true)
        {
            // Reload data from the database
            RefreshData(databaseManager);
            this.RaisePropertyChanged(nameof(DailyData)); // Notify UI about the data change
            this.RaisePropertyChanged(nameof(WeeklyData));
            await Task.Delay(TimeSpan.FromSeconds(5)); // Wait for 5 seconds before reloading data again
        }
    }

    private void RefreshData(DatabaseManager databaseManager)
    {
        // Reload data from the database
        DailyData = databaseManager.LoadData();
        WeeklyData = databaseManager.LoadWeeklyData();
        ChartDataGenerator.GenerateSeries(TotalSeriesA, WeeklyData["totalA"], ChartDataGenerator.ProductionLinesA);
        ChartDataGenerator.GenerateSeries(TotalSeriesB, WeeklyData["totalB"], ChartDataGenerator.ProductionLinesB);
        ChartDataGenerator.GenerateSeries(RateSeriesA, WeeklyData["rateA"], ChartDataGenerator.ProductionLinesA);
        ChartDataGenerator.GenerateSeries(RateSeriesB, WeeklyData["rateB"], ChartDataGenerator.ProductionLinesB);
    }
}