using KmTimer.Models;

namespace KmTimer.Models;

public sealed class PresetSettings
{
    public int TimerMin { get; set; } = 5;
    public int TimerSec { get; set; }
    public bool Countup { get; set; } = true;
    public string ColorNormal { get; set; } = "#10B981";
    public string ColorWarning { get; set; } = "#EAB308";
    public int TimeWarning { get; set; } = 30;
    public string ColorDanger { get; set; } = "#EF4444";
    public double TimerSize { get; set; } = 20;
    public bool KanpeToggle { get; set; } = true;
    public string KanpeText1 { get; set; } = "";
    public string KanpeText2 { get; set; } = "";
    public string KanpeText3 { get; set; } = "";
    public string KanpeText4 { get; set; } = "";
    public double KanpeFontSize { get; set; } = 5;
    public string KanpeColor { get; set; } = "#FFFFFF";
    public string BgColor { get; set; } = "#0F172A";
    public bool ClockToggle { get; set; } = true;
    public double ClockSize { get; set; } = 2;
    public ClockPosition ClockPosition { get; set; } = ClockPosition.TopLeft;
    public string ClockColor { get; set; } = "#94A3B8";
}
