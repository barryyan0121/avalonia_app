using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaApplication.Models;
using AvaloniaApplication.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;

namespace AvaloniaApplication.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly DatabaseManager _databaseManager;
    private UserControl _currentView;
    private DateTime _today = DateTime.Today;
    private readonly List<UserControl> _views = [new PrimaryView(), new SecondaryView()];
    private int _currentViewIndex;

    public MainViewModel()
    {
        for (var i = 0; i < PieSeries.Length; i++)
        {
            PieSeries[i] = new ISeries[2];
        }
        
        
        _currentViewIndex = 0;
        _currentView = _views[_currentViewIndex];

        LiveCharts.Configure(config =>
            config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')));
        const string connectionString =
            "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;";
        _databaseManager = new DatabaseManager(connectionString);
        RefreshData();
        ChartDataGenerator.GenerateGaugeSeries(GaugeSeries, ProductionLineNames, _databaseManager.ProgressMap);
        ChartDataGenerator.GeneratePieCharts(PieSeries, ProductionLineNames, _databaseManager.RateMap);
        ChartDataGenerator.GenerateLineSeries(TotalSeriesA, _databaseManager.WeeklyDataMap["totalA"],
            DatabaseManager.ProductionLinesA);
        ChartDataGenerator.GenerateLineSeries(TotalSeriesB, _databaseManager.WeeklyDataMap["totalB"],
            DatabaseManager.ProductionLinesB);
        ChartDataGenerator.GenerateLineSeries(RateSeriesA, _databaseManager.WeeklyDataMap["rateA"],
            DatabaseManager.ProductionLinesA);
        ChartDataGenerator.GenerateLineSeries(RateSeriesB, _databaseManager.WeeklyDataMap["rateB"],
            DatabaseManager.ProductionLinesB);
        ChartDataGenerator.GenerateRowSeries(RaceSeries, _databaseManager.ProgressInfos);
        // Start a background task to periodically check for data changes
        Task.Run(async () => { await CheckForDataChanges(); });
    }

    public AvaloniaList<ProductionData> DailyData { get; set; } = [];


    public DateTime Today
    {
        get => _today;
        set => this.RaiseAndSetIfChanged(ref _today, value);
    }

    public ObservableCollection<ISeries> TotalSeriesA { get; set; } = [];
    public ObservableCollection<ISeries> TotalSeriesB { get; set; } = [];
    public ObservableCollection<ISeries> RateSeriesA { get; set; } = [];
    public ObservableCollection<ISeries> RateSeriesB { get; set; } = [];
    public ISeries[][] PieSeries { get; set; } = new ISeries[12][];

    public ISeries[] RaceSeries { get; set; } = new ISeries[1];

    public IEnumerable<ISeries>[] GaugeSeries { get; set; } = new IEnumerable<ISeries>[12];


    public Axis[] XAxes { get; set; } =
    [
        new Axis
        {
            Name = "生产日期",
            Labels = ChartDataGenerator.GetLastSevenDays(),
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            NameTextSize = 10,
            TextSize = 10,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }
        }
    ];

    public Axis[] XProgressAxes { get; set; } =
    [
        new Axis
        {
            Name = "生产进度 (%)",
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            NameTextSize = 10,
            TextSize = 10,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 },
            MinLimit = 0,
            MaxLimit = 100
        }
    ];

    public Axis[] YProgressAxes { get; set; } =
    [
        new Axis
        {
            IsVisible = false
        }
    ];

    public Axis[] YProductionAxes { get; set; } =
    [
        new Axis
        {
            Name = "产量",
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            NameTextSize = 10,
            TextSize = 10,
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
            NameTextSize = 10,
            TextSize = 10,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
            {
                StrokeThickness = 2
            },
            MaxLimit = 1.1
        }
    ];

    public SolidColorPaint LegendTextPaint { get; set; } = new()
    {
        Color = new SKColor(255, 255, 255)
    };

    public static List<string> ProductionLineNames => DatabaseManager.ProductionLinesTotal;

    public UserControl CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    public static void ExitCommand()
    {
        // Close the application
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    public void SwitchViewCommand(string parameter)
    {
        _currentViewIndex = parameter switch
        {
            "Primary" => 0,
            "Secondary" => 1,
            _ => _currentViewIndex
        };

        CurrentView = _views[_currentViewIndex];
    }

    private async Task CheckForDataChanges()
    {
        while (true)
        {
            // Reload data from the database
            RefreshData();
            RaceSeries[0].Values = _databaseManager.ProgressInfos.OrderBy(x => x.Value).ToArray();
            await Task.Delay(TimeSpan.FromSeconds(3)); // Wait for 3 seconds before reloading data again
        }
    }

    private void RefreshData()
    {
        // Reload data from the database
        DailyData = _databaseManager.LoadData();
        _databaseManager.LoadWeeklyData();
        this.RaisePropertyChanged(nameof(DailyData)); // Notify UI about the data change
    }
}