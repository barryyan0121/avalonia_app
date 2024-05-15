using System.Linq;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Themes;

namespace AvaloniaApplication.Models;

public class ProgressInfo : ObservableValue
{
    public static SolidColorPaint[] Paints = Enumerable.Range(0, 9)
        .Select(i => new SolidColorPaint(ColorPalletes.MaterialDesign500[i].AsSKColor()))
        .ToArray();

    public ProgressInfo(string name, double value, SolidColorPaint paint)
    {
        Name = name;
        Paint = paint;
        // the ObservableValue.Value property is used by the chart
        Value = value;
    }

    public string Name { get; set; }
    public SolidColorPaint Paint { get; set; }
}