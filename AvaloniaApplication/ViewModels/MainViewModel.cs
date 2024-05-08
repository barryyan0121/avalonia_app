using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using AvaloniaApplication.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;

namespace AvaloniaApplication.ViewModels;

public class MainViewModel : ViewModelBase
{
    private DateTime _today = DateTime.Today;

    public MainViewModel()
    {
        const string connectionString =
            "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;";
        var databaseManager = new DatabaseManager(connectionString);
        DailyData = databaseManager.LoadData();
        WeeklyData = databaseManager.LoadWeeklyData();
        // Start a background task to periodically check for data changes
        Task.Run(async () => { await CheckForDataChanges(databaseManager); });
    }

    public AvaloniaList<ProductionData> DailyData { get; private set; }
    public AvaloniaList<WeeklyProductionData> WeeklyData { get; private set; }

    public ISeries[] Series { get; set; } =
    [
        new LineSeries<double>
        {
            Values = new double[] { 2, 1, 3, 5, 3, 4, 6 },
            Fill = null
        }
    ];

    public DateTime Today
    {
        get => _today;
        set => this.RaiseAndSetIfChanged(ref _today, value);
    }

    private async Task CheckForDataChanges(DatabaseManager databaseManager)
    {
        while (true)
        {
            // Reload data from the database
            DailyData = databaseManager.LoadData();
            WeeklyData = databaseManager.LoadWeeklyData();
            this.RaisePropertyChanged(nameof(DailyData)); // Notify UI about the data change
            this.RaisePropertyChanged(nameof(WeeklyData)); // Notify UI about the data change
            await Task.Delay(TimeSpan.FromSeconds(10)); // Wait for 10 seconds before reloading data again
        }
    }
}