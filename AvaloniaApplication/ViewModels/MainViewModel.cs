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
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;

namespace AvaloniaApplication.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly DatabaseManager _databaseManager;

    // Do not forget to add the new views here
    private readonly List<UserControl> _views = [new FirstView(), new SecondView(), new ThirdView(), new FourthView()];
    private string _currentProductionDate = DateTime.Today.ToString("yyyy-MM-dd");
    private string _currentProductionLineName = ProductionLineNames[0];
    private UserControl _currentView;
    private int _currentViewIndex;
    private AvaloniaList<ProductionData> _dailyData = [];

    private KeyValuePair<ObservableCollection<ObservableValue>, ObservableCollection<ObservableValue>>
        _hourlyProductionCounts;


    private AvaloniaList<string> _productionLineDates = [];
    private DateTime _today = DateTime.Today;

    public MainViewModel()
    {
        for (var i = 0; i < PieSeries.Length; i++)
        {
            PieSeries[i] = new ISeries[2];
        }

        _currentView = _views[_currentViewIndex];

        LiveCharts.Configure(config =>
            config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')));
        const string connectionString =
            "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;";
        _databaseManager = new DatabaseManager(connectionString);
        RefreshData();
        GenerateAllSeries();
        // Start a background task to periodically check for data changes
        Task.Run(async () => { await CheckForDataChanges(); });
    }

    private AvaloniaDictionary<string, AvaloniaDictionary<string, AvaloniaList<ProductionDetails>>>
        ProductionDetailsDict { get; set; } = [];

    public AvaloniaList<ProductionData> DailyData
    {
        get => _dailyData;
        set => this.RaiseAndSetIfChanged(ref _dailyData, value);
    }

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

    public ISeries[] ColumnSeries { get; set; } = new ISeries[2];

    public IEnumerable<ISeries>[] GaugeSeries { get; set; } = new IEnumerable<ISeries>[12];

    public string CurrentProductionLineName
    {
        get => _currentProductionLineName;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentProductionLineName, value);
            ProductionDetailsDict = _databaseManager.LoadAllData();
            this.RaisePropertyChanged(nameof(CurrentProductionList));
            this.RaisePropertyChanged(nameof(ProductionLineDates));
            _ = GetHourlyProductionCounts();
        }
    }

    public string CurrentProductionDate
    {
        get => _currentProductionDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentProductionDate, value);
            ProductionDetailsDict = _databaseManager.LoadAllData();
            this.RaisePropertyChanged(nameof(CurrentProductionList));
            _ = GetHourlyProductionCounts();
        }
    }

    public AvaloniaList<ProductionDetails> CurrentProductionList =>
        ProductionDetailsDict[CurrentProductionLineName][CurrentProductionDate];

    public AvaloniaList<string> ProductionLineDates
    {
        get
        {
            _productionLineDates = ProductionDetailsDict.TryGetValue(CurrentProductionLineName, out var value)
                ? new AvaloniaList<string>(value.Keys)
                : [];
            return _productionLineDates;
        }
        set => this.RaiseAndSetIfChanged(ref _productionLineDates, value);
    }

    public KeyValuePair<ObservableCollection<ObservableValue>, ObservableCollection<ObservableValue>>
        HourlyProductionCounts
    {
        get
        {
            if (!ProductionDetailsDict.TryGetValue(CurrentProductionLineName, out var value))
            {
                var qualifiedObservableCollection = new ObservableCollection<ObservableValue>();
                var nonQualifiedObservableCollection = new ObservableCollection<ObservableValue>();
                for (var i = 0; i < 24; i++)
                {
                    qualifiedObservableCollection.Add(new ObservableValue(0));
                    nonQualifiedObservableCollection.Add(new ObservableValue(0));
                }

                _hourlyProductionCounts =
                    new KeyValuePair<ObservableCollection<ObservableValue>, ObservableCollection<ObservableValue>>(
                        qualifiedObservableCollection, nonQualifiedObservableCollection);
                return _hourlyProductionCounts;
            }

            // Reset the counts to zero
            foreach (var observableValue in _hourlyProductionCounts.Key)
            {
                observableValue.Value = 0;
            }

            foreach (var observableValue in _hourlyProductionCounts.Value)
            {
                observableValue.Value = 0;
            }

            // TODO access the current selected date, current implementation is wrong
            var detailsList = value[CurrentProductionDate];
            foreach (var detail in detailsList)
            {
                var hour = detail.ProductionTime.Hour;

                switch (detail.IsQualified)
                {
                    case "OK":
                        _hourlyProductionCounts.Key[hour].Value++;
                        break;
                    case "NG":
                        _hourlyProductionCounts.Value[hour].Value++;
                        break;
                }
            }


            return _hourlyProductionCounts;
        }
        set => this.RaiseAndSetIfChanged(ref _hourlyProductionCounts, value);
    }


    public Axis[] XDateAxes { get; set; } =
    [
        new Axis
        {
            Name = "生产日期",
            Labels = ChartDataGenerator.GetLastSevenDays(),
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            NameTextSize = 15,
            TextSize = 15,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }
        }
    ];

    public Axis[] XHourAxes { get; set; } =
    [
        new Axis
        {
            Name = "生产时间",
            Labels = ChartDataGenerator.GetHours(),
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            NameTextSize = 15,
            TextSize = 15,
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
            NameTextSize = 20,
            TextSize = 20,
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
            NameTextSize = 15,
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
            NameTextSize = 15,
            TextSize = 10,
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

    public static List<string> ProductionLineNames => DatabaseManager.ProductionLinesTotal;

    public UserControl CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    private void GenerateAllSeries()
    {
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
        ChartDataGenerator.GenerateHourlyColumnSeries(ColumnSeries, HourlyProductionCounts);
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
            "First" => 0,
            "Second" => 1,
            "Third" => 2,
            "Fourth" => 3,
            _ => _currentViewIndex
        };

        CurrentView = _views[_currentViewIndex];
    }

    private KeyValuePair<ObservableCollection<ObservableValue>, ObservableCollection<ObservableValue>>
        GetHourlyProductionCounts()
    {
        return HourlyProductionCounts;
    }

    private async Task CheckForDataChanges()
    {
        while (true)
        {
            // Wait for 10 seconds before reloading data again
            await Task.Delay(TimeSpan.FromSeconds(10));
            // Reload data from the database
            RefreshData();
            RaceSeries[0].Values = _databaseManager.ProgressInfos.OrderBy(x => x.Value).ToArray();
        }
    }

    private void RefreshData()
    {
        // Reload data from the database
        _dailyData = _databaseManager.LoadData();
        _databaseManager.LoadWeeklyData();
    }
}