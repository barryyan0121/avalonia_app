using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

namespace UnitTest;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
