using System.Linq;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Themes;

namespace AvaloniaApplication.Models;

public class ProgressInfo : ObservableValue
{
    // 颜色数组区分不同工位进度条
    public static readonly SolidColorPaint[] Paints = Enumerable.Range(0, 9)
        .Select(i => new SolidColorPaint(ColorPalletes.MaterialDesign500[i].AsSKColor()))
        .ToArray();

    // 构造函数
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