using Avalonia;
using Avalonia.Headless;
using UnitTest;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
namespace UnitTest;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}