using Avalonia.Collections;
using AvaloniaApplication.Models;
using ReactiveUI;

namespace AvaloniaApplication.ViewModels;

public class MainViewModel : ViewModelBase
{
    private AvaloniaList<ProductionData> _tableData = [];

    public MainViewModel()
    {
        const string connectionString =
            "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;";
        var databaseManager = new DatabaseManager(connectionString);
        TableData = databaseManager.LoadData();
    }

    public AvaloniaList<ProductionData> TableData
    {
        get => _tableData;
        set => this.RaiseAndSetIfChanged(ref _tableData, value);
    }
}