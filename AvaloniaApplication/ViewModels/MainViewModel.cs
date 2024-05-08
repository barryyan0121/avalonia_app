using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using AvaloniaApplication.Models;
using ReactiveUI;

namespace AvaloniaApplication.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        const string connectionString =
            "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;";
        var databaseManager = new DatabaseManager(connectionString);
        TableData = databaseManager.LoadData();
        // Start a background task to periodically check for data changes
        Task.Run(async () => { await CheckForDataChanges(databaseManager); });
    }

    public AvaloniaList<ProductionData> TableData { get; private set; }

    private async Task CheckForDataChanges(DatabaseManager databaseManager)
    {
        while (true)
        {
            // Reload data from the database
            TableData = databaseManager.LoadData();
            this.RaisePropertyChanged(nameof(TableData)); // Notify UI about the data change
            await Task.Delay(TimeSpan.FromSeconds(10)); // Wait for 10 seconds before reloading data again
        }
    }
}