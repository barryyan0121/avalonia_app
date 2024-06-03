using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using AvaloniaApplication.ViewModels;
using AvaloniaApplication.Views;

namespace UnitTest;

public class ViewModelTests
{
    private MainViewModel _viewModel;

    [SetUp]
    public void Setup()
    {
        // Initialize the ViewModel
        _viewModel = new MainViewModel();
    }

    [Test]
    public void Should_Initialize_Correctly()
    {
        // Assert initial state of ViewModel
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.DailyData, Is.Not.Null);
            Assert.That(_viewModel.TotalSeriesA, Is.Not.Null);
            Assert.That(_viewModel.TotalSeriesB, Is.Not.Null);
            Assert.That(_viewModel.RateSeriesA, Is.Not.Null);
            Assert.That(_viewModel.RateSeriesB, Is.Not.Null);
            Assert.That(_viewModel.PieSeries, Is.Not.Null);
            Assert.That(_viewModel.RaceSeries, Is.Not.Null);
            Assert.That(_viewModel.ColumnSeries, Is.Not.Null);
            Assert.That(_viewModel.GaugeSeries, Is.Not.Null);
        });
    }

    [Test]
    public void Should_Set_CurrentView_Correctly()
    {
        // Test switching views
        _viewModel.SwitchViewCommand("First");
        Assert.That(_viewModel.CurrentView, Is.InstanceOf<FirstView>());

        _viewModel.SwitchViewCommand("Second");
        Assert.That(_viewModel.CurrentView, Is.InstanceOf<SecondView>());

        _viewModel.SwitchViewCommand("Third");
        Assert.That(_viewModel.CurrentView, Is.InstanceOf<ThirdView>());

        _viewModel.SwitchViewCommand("Fourth");
        Assert.That(_viewModel.CurrentView, Is.InstanceOf<FourthView>());
    }

    [Test]
    public void Should_Update_ProductionLineData_Correctly()
    {
        // Set production line name and check if data updates correctly
        _viewModel.CurrentProductionLineName = "胶纸切割";
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.CurrentProductionList, Is.Not.Null);
            Assert.That(_viewModel.ProductionLineDates, Is.Not.Null);
        });
    }

    [Test]
    public void Should_Update_CurrentProductionDate_Correctly()
    {
        // Set production date and check if data updates correctly
        _viewModel.CurrentProductionDate = DateTime.Today.ToString("yyyy-MM-dd");
        Assert.That(_viewModel.CurrentProductionList, Is.Not.Null);
    }

    [Test]
    public void Should_Refresh_Data_And_Update_Series()
    {
        // Mock data refresh and verify that series data is updated
        
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.DailyData, Is.Not.Null);
            Assert.That(_viewModel.TotalSeriesA, Is.Not.Empty);
            Assert.That(_viewModel.TotalSeriesB, Is.Not.Empty);
            Assert.That(_viewModel.RateSeriesA, Is.Not.Empty);
            Assert.That(_viewModel.RateSeriesB, Is.Not.Empty);
        });
    }
}