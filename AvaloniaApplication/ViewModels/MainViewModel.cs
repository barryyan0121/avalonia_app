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
    // 数据库管理对象
    private readonly DatabaseManager _databaseManager;

    // 视图数组
    private readonly List<UserControl> _views =
    [
        new FirstView(),
        new SecondView(),
        new ThirdView(),
        new FourthView()
    ];

    //当日日期
    private string _currentProductionDate = DateTime.Today.ToString("yyyy-MM-dd");

    // 工位名 
    private string _currentProductionLineName = ProductionLineNames[0];

    // 当前视图
    private UserControl _currentView;

    // 当前视图对应的索引
    private int _currentViewIndex;

    // 当日数据
    private AvaloniaList<ProductionData> _dailyData = [];

    // 每小时产量统计 Key代表合格产量 Value代表不合格产量
    private KeyValuePair<ObservableCollection<ObservableValue>, ObservableCollection<ObservableValue>>
        _hourlyProductionCounts;

    // 生产线对应的生产日期数组
    private AvaloniaList<string> _productionLineDates = [];
    private DateTime _today = DateTime.Today;

    // 构造函数
    public MainViewModel()
    {
        // 初始化当前视图
        _currentView = _views[_currentViewIndex];

        // 开启汉字兼容性
        LiveCharts.Configure(config =>
            config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')));

        // 数据库连接字符串
        const string connectionString =
            "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;";
        _databaseManager = new DatabaseManager(connectionString);

        // 第一次获得数据库数据 
        RefreshData();

        // 生成所有图表数据
        GenerateAllSeries();

        // 后台每隔10s轮询数据库数据
        Task.Run(async () => { await CheckForDataChanges(10); });
    }

    // 生产详情字典 => {生产线名 => {生产日期 => 生产详情列表}}
    private AvaloniaDictionary<string, AvaloniaDictionary<string, AvaloniaList<ProductionDetails>>>
        ProductionDetailsDict { get; set; } = [];

    // 当天生产详情列表
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

    // 工区A总产量图表数据
    public ObservableCollection<ISeries> TotalSeriesA { get; set; } = [];

    // 工区B总产量图表数据
    public ObservableCollection<ISeries> TotalSeriesB { get; set; } = [];

    // 工区A合格率图表数据
    public ObservableCollection<ISeries> RateSeriesA { get; set; } = [];

    // 工区B合格率图表数据
    public ObservableCollection<ISeries> RateSeriesB { get; set; } = [];

    //  饼状图
    public ISeries[][] PieSeries { get; set; } = new ISeries[12][];

    // 进度条
    public ISeries[] RaceSeries { get; set; } = new ISeries[1];

    // 柱状图
    public ISeries[] ColumnSeries { get; set; } = new ISeries[2];

    // 仪表盘
    public IEnumerable<ISeries>[] GaugeSeries { get; set; } = new IEnumerable<ISeries>[12];

    // 当前工位名 由界面数据绑定
    public string CurrentProductionLineName
    {
        get => _currentProductionLineName;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentProductionLineName, value);
            // 从数据库重新加载数据
            ProductionDetailsDict = _databaseManager.LoadAllData();
            // 通知界面数据已经改变
            this.RaisePropertyChanged(nameof(CurrentProductionList));
            this.RaisePropertyChanged(nameof(ProductionLineDates));
            // 调用get属性获取每小时产量统计
            _ = GetHourlyProductionCounts();
        }
    }

    // 当前生产日期 由界面数据绑定
    public string CurrentProductionDate
    {
        get => _currentProductionDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentProductionDate, value);
            ProductionDetailsDict = _databaseManager.LoadAllData();
            // 通知界面数据已经改变
            this.RaisePropertyChanged(nameof(CurrentProductionList));
            // 调用get属性获取每小时产量统计
            _ = GetHourlyProductionCounts();
        }
    }

    // 当前生产详情列表
    public AvaloniaList<ProductionDetails> CurrentProductionList =>
        ProductionDetailsDict[CurrentProductionLineName][CurrentProductionDate];

    // 当前生产线生产日期列表
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

    // 每小时产量统计 Key代表合格产量 Value代表不合格产量
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

            // 重置每小时合格产量统计为零
            foreach (var observableValue in _hourlyProductionCounts.Key)
            {
                observableValue.Value = 0;
            }

            // 重置每小时不合格产量统计为零
            foreach (var observableValue in _hourlyProductionCounts.Value)
            {
                observableValue.Value = 0;
            }

            // 遍历当天生产详情列表，统计每小时合格和不合格产量
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

    // X轴日期坐标轴
    public Axis[] XDateAxes { get; set; } =
    [
        new Axis
        {
            Name = "生产日期",
            Labels = ChartDataGenerator.GetLastSevenDays(DateTime.Today),
            NamePaint = new SolidColorPaint(SKColors.White),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            NameTextSize = 15,
            TextSize = 15,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }
        }
    ];

    // X轴小时坐标轴
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

    // X轴生产进度坐标轴
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

    // Y轴生产进度坐标轴
    public Axis[] YProgressAxes { get; set; } =
    [
        new Axis
        {
            IsVisible = false
        }
    ];

    // Y轴产量坐标轴
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

    // Y轴合格率坐标轴
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

    // 工位名称字符串数组
    public static List<string> ProductionLineNames => DatabaseManager.ProductionLinesTotal;

    // 当前视图
    public UserControl CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    // 退出命令
    public static void ExitCommand()
    {
        // Close the application
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    // 切换视图命令，不同视图绑定不同的参数
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

    // 切换显示柱状图命令
    public void ToggleSeries(string parameter)
    {
        switch (parameter)
        {
            case "Total":
                ColumnSeries[0].IsVisible = true;
                ColumnSeries[1].IsVisible = true;
                break;
            case "Qualified":
                ColumnSeries[0].IsVisible = true;
                ColumnSeries[1].IsVisible = false;
                break;
            case "NonQualified":
                ColumnSeries[0].IsVisible = false;
                ColumnSeries[1].IsVisible = true;
                break;
        }
    }

    // 生成所有图表
    private void GenerateAllSeries()
    {
        // 生成仪表盘生产进度条数据
        ChartDataGenerator.GenerateGaugeSeries(GaugeSeries, ProductionLineNames, _databaseManager.ProgressMap);
        // 生成合格率饼状图数据
        ChartDataGenerator.GeneratePieCharts(PieSeries, ProductionLineNames, _databaseManager.RateMap);
        // 生成工区A产量折线图
        ChartDataGenerator.GenerateLineSeries(TotalSeriesA, _databaseManager.WeeklyDataMap["totalA"],
            DatabaseManager.ProductionLinesA);
        // 生成工区B产量折线图
        ChartDataGenerator.GenerateLineSeries(TotalSeriesB, _databaseManager.WeeklyDataMap["totalB"],
            DatabaseManager.ProductionLinesB);
        // 生成工区A合格率折线图
        ChartDataGenerator.GenerateLineSeries(RateSeriesA, _databaseManager.WeeklyDataMap["rateA"],
            DatabaseManager.ProductionLinesA);
        // 生成工区B合格率折线图
        ChartDataGenerator.GenerateLineSeries(RateSeriesB, _databaseManager.WeeklyDataMap["rateB"],
            DatabaseManager.ProductionLinesB);
        // 生成动态生产进度条数据
        ChartDataGenerator.GenerateRowSeries(RaceSeries, _databaseManager.ProgressInfos);
        // 生成当日分时柱状图数据
        ChartDataGenerator.GenerateHourlyColumnSeries(ColumnSeries, HourlyProductionCounts);
    }

    // 获取每小时产量统计 触发get属性
    private KeyValuePair<ObservableCollection<ObservableValue>, ObservableCollection<ObservableValue>>
        GetHourlyProductionCounts()
    {
        return HourlyProductionCounts;
    }

    // 每隔一定时间轮询数据库
    private async Task CheckForDataChanges(int second)
    {
        while (true)
        {
            // Wait for 10 seconds before reloading data again
            await Task.Delay(TimeSpan.FromSeconds(second));
            // Reload data from the database
            RefreshData();
            RaceSeries[0].Values = _databaseManager.ProgressInfos.OrderBy(x => x.Value).ToArray();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    // 加载数据
    private void RefreshData()
    {
        // Reload data from the database
        _dailyData = _databaseManager.LoadData();
        _databaseManager.LoadWeeklyData();
    }
}