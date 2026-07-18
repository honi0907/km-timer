using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KmTimer.Helpers;
using KmTimer.Models;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace KmTimer.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly DispatcherQueue _dispatcher;
    private DispatcherQueueTimer? _timerTick;
    private DispatcherQueueTimer? _clockTick;
    private DispatcherQueueTimer? _toastTimer;
    private long _targetEndTimeMs;
    private int _baseSeconds = 5 * 60;
    private int _displayedRemainingSeconds = 5 * 60;

    public MainViewModel(DispatcherQueue dispatcher)
    {
        _dispatcher = dispatcher;
        AppVersionText = $"KM_Timer v{GetAppVersion()}";
        RefreshPresetLabels();
        ResetTimerFromInputs();
        StartClock();
    }

    public string AppVersionText { get; }

    [ObservableProperty] private bool _isTimerRunning;
    [ObservableProperty] private bool _isTimerPaused;

    [ObservableProperty] private double _timerMinutes = 5;
    [ObservableProperty] private double _timerSeconds;
    [ObservableProperty] private bool _countUpAfterZero = true;
    [ObservableProperty] private string _colorNormal = "#10B981";
    [ObservableProperty] private string _colorWarning = "#EAB308";
    [ObservableProperty] private double _warningSeconds = 30;
    [ObservableProperty] private string _colorDanger = "#EF4444";
    [ObservableProperty] private double _timerSize = 20;
    [ObservableProperty] private bool _kanpeVisible;
    [ObservableProperty] private int _activeKanpeSlot;
    [ObservableProperty] private string _draftKanpeText = "";
    [ObservableProperty] private int _selectedKanpeSlot;
    [ObservableProperty] private bool _isKanpeDraftVisible;
    [ObservableProperty] private string _kanpeDraftBadgeText = "";
    [ObservableProperty] private bool _isKanpeDraftBadgeVisible;
    [ObservableProperty] private string _kanpeText1 = "";
    [ObservableProperty] private string _kanpeText2 = "";
    [ObservableProperty] private string _kanpeText3 = "";
    [ObservableProperty] private string _kanpeText4 = "";
    [ObservableProperty] private double _kanpeFontSize = 5;
    [ObservableProperty] private string _kanpeColor = "#FFFFFF";
    [ObservableProperty] private string _bgColor = "#0F172A";
    [ObservableProperty] private bool _clockVisible = true;
    [ObservableProperty] private double _clockSize = 2;
    [ObservableProperty] private ClockPosition _clockPosition = ClockPosition.TopLeft;
    [ObservableProperty] private string _clockColor = "#94A3B8";

    [ObservableProperty] private string _timerText = "05:00";
    [ObservableProperty] private SolidColorBrush _timerBrush = ColorParse.ToBrush("#10B981", Color.FromArgb(255, 16, 185, 129));
    [ObservableProperty] private bool _timerBlink;
    [ObservableProperty] private bool _isKanpeBgBlinking;
    [ObservableProperty] private string _clockText = "00:00:00";
    [ObservableProperty] private string _displayKanpeText = "";
    [ObservableProperty] private string _toastMessage = "";
    [ObservableProperty] private bool _isToastVisible;

    [ObservableProperty] private string _presetLabel1 = "未設定 1";
    [ObservableProperty] private string _presetLabel2 = "未設定 2";
    [ObservableProperty] private string _presetLabel3 = "未設定 3";
    [ObservableProperty] private string _presetLabel4 = "未設定 4";
    [ObservableProperty] private string _presetLabel5 = "未設定 5";

    public IReadOnlyList<ClockPositionOption> ClockPositionOptions { get; } =
    [
        new(ClockPosition.TopLeft, "左上"),
        new(ClockPosition.TopRight, "右上"),
        new(ClockPosition.BottomLeft, "左下"),
        new(ClockPosition.BottomRight, "右下")
    ];

    partial void OnTimerMinutesChanged(double value)
    {
        if (!IsTimerRunning)
            ResetTimerFromInputs();
    }

    partial void OnTimerSecondsChanged(double value)
    {
        if (!IsTimerRunning)
            ResetTimerFromInputs();
    }

    partial void OnColorNormalChanged(string value) => RefreshIdleTimerStyle();
    partial void OnColorWarningChanged(string value) => RefreshIdleTimerStyle();
    partial void OnColorDangerChanged(string value) => RefreshIdleTimerStyle();
    partial void OnWarningSecondsChanged(double value) => RefreshIdleTimerStyle();

    partial void OnKanpeText1Changed(string value) => OnKanpeSlotTextChanged(1, value);
    partial void OnKanpeText2Changed(string value) => OnKanpeSlotTextChanged(2, value);
    partial void OnKanpeText3Changed(string value) => OnKanpeSlotTextChanged(3, value);
    partial void OnKanpeText4Changed(string value) => OnKanpeSlotTextChanged(4, value);

    partial void OnActiveKanpeSlotChanged(int value) => SyncKanpeOutputFromActiveSlot();

    public string KanpeSendLabel1 => ActiveKanpeSlot == 1 ? "解除" : "送信";
    public string KanpeSendLabel2 => ActiveKanpeSlot == 2 ? "解除" : "送信";
    public string KanpeSendLabel3 => ActiveKanpeSlot == 3 ? "解除" : "送信";
    public string KanpeSendLabel4 => ActiveKanpeSlot == 4 ? "解除" : "送信";
    public bool IsKanpeSlot1Sending => ActiveKanpeSlot == 1;
    public bool IsKanpeSlot2Sending => ActiveKanpeSlot == 2;
    public bool IsKanpeSlot3Sending => ActiveKanpeSlot == 3;
    public bool IsKanpeSlot4Sending => ActiveKanpeSlot == 4;

    public bool IsKanpeSlot1Selected => SelectedKanpeSlot == 1;
    public bool IsKanpeSlot2Selected => SelectedKanpeSlot == 2;
    public bool IsKanpeSlot3Selected => SelectedKanpeSlot == 3;
    public bool IsKanpeSlot4Selected => SelectedKanpeSlot == 4;

    public string KanpeBgBlinkLabel => IsKanpeBgBlinking ? "点滅停止" : "背景を点滅";

    partial void OnIsKanpeBgBlinkingChanged(bool value) =>
        OnPropertyChanged(nameof(KanpeBgBlinkLabel));

    partial void OnSelectedKanpeSlotChanged(int value)
    {
        OnPropertyChanged(nameof(IsKanpeSlot1Selected));
        OnPropertyChanged(nameof(IsKanpeSlot2Selected));
        OnPropertyChanged(nameof(IsKanpeSlot3Selected));
        OnPropertyChanged(nameof(IsKanpeSlot4Selected));
    }

    private void OnKanpeSlotTextChanged(int slot, string value)
    {
        if (SelectedKanpeSlot == slot)
        {
            DraftKanpeText = value;
            IsKanpeDraftVisible = !string.IsNullOrWhiteSpace(value);
        }

        if (ActiveKanpeSlot == slot)
            DisplayKanpeText = value;
    }

    private void NotifyKanpeSendUiChanged()
    {
        OnPropertyChanged(nameof(KanpeSendLabel1));
        OnPropertyChanged(nameof(KanpeSendLabel2));
        OnPropertyChanged(nameof(KanpeSendLabel3));
        OnPropertyChanged(nameof(KanpeSendLabel4));
        OnPropertyChanged(nameof(IsKanpeSlot1Sending));
        OnPropertyChanged(nameof(IsKanpeSlot2Sending));
        OnPropertyChanged(nameof(IsKanpeSlot3Sending));
        OnPropertyChanged(nameof(IsKanpeSlot4Sending));
    }

    private static string GetKanpeTextForSlot(int slot, string text1, string text2, string text3, string text4) => slot switch
    {
        1 => text1,
        2 => text2,
        3 => text3,
        4 => text4,
        _ => ""
    };

    private void SyncKanpeOutputFromActiveSlot()
    {
        KanpeVisible = ActiveKanpeSlot != 0;
        DisplayKanpeText = GetKanpeTextForSlot(ActiveKanpeSlot, KanpeText1, KanpeText2, KanpeText3, KanpeText4);
        NotifyKanpeSendUiChanged();
    }

    private void ClearKanpeOutputState()
    {
        ActiveKanpeSlot = 0;
        KanpeVisible = false;
        DisplayKanpeText = "";
        NotifyKanpeSendUiChanged();
    }

    [RelayCommand]
    private void StartTimer()
    {
        if (IsTimerRunning)
            return;

        IsTimerRunning = true;
        IsTimerPaused = false;
        var remaining = _displayedRemainingSeconds;
        _targetEndTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + remaining * 1000L;

        _timerTick ??= _dispatcher.CreateTimer();
        _timerTick.Interval = TimeSpan.FromMilliseconds(100);
        _timerTick.Tick -= OnTimerTick;
        _timerTick.Tick += OnTimerTick;
        _timerTick.Start();
    }

    [RelayCommand]
    private void PauseTimer()
    {
        if (!IsTimerRunning)
            return;

        IsTimerRunning = false;
        IsTimerPaused = true;
        _timerTick?.Stop();
        ApplyTimerStyle(_displayedRemainingSeconds);
    }

    [RelayCommand]
    private void ToggleTimerStartPause()
    {
        if (IsTimerRunning)
            PauseTimer();
        else
            StartTimer();
    }

    [RelayCommand]
    private void ResetTimer()
    {
        IsTimerRunning = false;
        IsTimerPaused = false;
        _timerTick?.Stop();
        ResetTimerFromInputs();
    }

    [RelayCommand]
    private void AdjustTimer(object? parameter)
    {
        var delta = ParseSignedInt(parameter);
        if (delta == 0)
            return;

        AdjustRemainingSeconds(delta);
    }

    private void AdjustRemainingSeconds(int delta)
    {
        var current = IsTimerRunning
            ? (int)Math.Ceiling((_targetEndTimeMs - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000.0)
            : _displayedRemainingSeconds;

        var next = current + delta;
        if (!CountUpAfterZero && next < 0)
            next = 0;

        if (IsTimerRunning)
        {
            _targetEndTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + next * 1000L;
            ApplyTimerStyle(next);

            if (next == 0 && !CountUpAfterZero)
                PauseTimer();
            return;
        }

        ApplyTimerStyle(next);
        if (next >= 0)
        {
            TimerMinutes = next / 60;
            TimerSeconds = next % 60;
            _baseSeconds = next;
        }
    }

    [RelayCommand]
    private void ToggleKanpeSend(object? parameter)
    {
        var slot = ParseIndex(parameter);
        if (slot is < 1 or > 4)
            return;

        if (ActiveKanpeSlot == slot)
        {
            ClearKanpeOutputState();
            BeginKanpePreview(slot);
            return;
        }

        ActiveKanpeSlot = slot;
        KanpeVisible = true;
        DisplayKanpeText = GetKanpeTextForSlot(slot, KanpeText1, KanpeText2, KanpeText3, KanpeText4);
        NotifyKanpeSendUiChanged();
        BeginKanpePreview(slot);
    }

    [RelayCommand]
    private void ToggleKanpeBgBlink() => IsKanpeBgBlinking = !IsKanpeBgBlinking;

    [RelayCommand]
    private void SavePreset(object? parameter)
    {
        var index = ParseIndex(parameter);
        if (index is < 1 or > 5)
            return;

        var existed = SettingsStore.PresetExists(index);
        var settings = CaptureSettings();
        SettingsStore.SavePreset(index, settings);
        SetPresetLabel(index, FormatPresetLabel(settings));
        ShowToast(existed ? "上書きしました" : "保存しました");
    }

    [RelayCommand]
    private void LoadPreset(object? parameter)
    {
        var index = ParseIndex(parameter);
        var settings = SettingsStore.LoadPreset(index);
        if (settings is null)
            return;

        ApplySettings(settings);
        ClearKanpeOutputState();
        ClearKanpeDraftPreview();

        if (!IsTimerRunning)
            ResetTimerFromInputs();
    }

    private static int ParseIndex(object? parameter) => parameter switch
    {
        int i => i,
        string s when int.TryParse(s, out var n) => n,
        _ => 0
    };

    private static int ParseSignedInt(object? parameter) => parameter switch
    {
        int i => i,
        string s when int.TryParse(s, out var n) => n,
        _ => 0
    };

    public void BeginKanpePreview(int slot)
    {
        if (slot is < 1 or > 4)
            return;

        SelectedKanpeSlot = slot;
        RefreshDraftPreview(slot);
    }

    public void UpdateKanpePreview(int slot, string text)
    {
        if (SelectedKanpeSlot != slot)
            return;

        DraftKanpeText = text;
        IsKanpeDraftVisible = !string.IsNullOrWhiteSpace(text);
    }

    private void RefreshDraftPreview(int slot)
    {
        var text = GetKanpeTextForSlot(slot, KanpeText1, KanpeText2, KanpeText3, KanpeText4);
        DraftKanpeText = text;
        IsKanpeDraftVisible = !string.IsNullOrWhiteSpace(text);
        KanpeDraftBadgeText = $"(枠{slot}の改行位置を確認中...)";
        IsKanpeDraftBadgeVisible = true;
    }

    private void ClearKanpeDraftPreview()
    {
        SelectedKanpeSlot = 0;
        DraftKanpeText = "";
        IsKanpeDraftVisible = false;
        KanpeDraftBadgeText = "";
        IsKanpeDraftBadgeVisible = false;
    }

    private void OnTimerTick(DispatcherQueueTimer sender, object args)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var diffSecs = (int)Math.Ceiling((_targetEndTimeMs - now) / 1000.0);

        if (diffSecs <= 0)
        {
            if (CountUpAfterZero)
            {
                ApplyTimerStyle(diffSecs);
            }
            else
            {
                ApplyTimerStyle(0);
                PauseTimer();
            }
        }
        else
        {
            ApplyTimerStyle(diffSecs);
        }
    }

    private void StartClock()
    {
        _clockTick = _dispatcher.CreateTimer();
        _clockTick.Interval = TimeSpan.FromSeconds(1);
        _clockTick.Tick += (_, _) =>
        {
            ClockText = DateTime.Now.ToString("HH:mm:ss");
        };
        _clockTick.Start();
        ClockText = DateTime.Now.ToString("HH:mm:ss");
    }

    private void ResetTimerFromInputs()
    {
        var m = Math.Max(0, (int)TimerMinutes);
        var s = Math.Clamp((int)TimerSeconds, 0, 59);
        _baseSeconds = m * 60 + s;
        ApplyTimerStyle(_baseSeconds);
    }

    private void RefreshIdleTimerStyle()
    {
        if (!IsTimerRunning)
            ApplyTimerStyle(_baseSeconds);
    }

    private void ApplyTimerStyle(int remainingSecs)
    {
        _displayedRemainingSeconds = remainingSecs;
        TimerText = FormatTime(remainingSecs);
        var color = ColorNormal;
        var blink = false;

        if (remainingSecs < 0)
        {
            color = ColorDanger;
        }
        else if (remainingSecs == 0)
        {
            color = ColorDanger;
            blink = CountUpAfterZero;
        }
        else if (remainingSecs <= (int)WarningSeconds)
        {
            color = ColorWarning;
            if (remainingSecs <= 10)
                blink = true;
        }

        TimerBrush = ColorParse.ToBrush(color, Color.FromArgb(255, 16, 185, 129));
        TimerBlink = blink;
    }

    private static string FormatTime(int totalSeconds)
    {
        var abs = Math.Abs(totalSeconds);
        var m = abs / 60;
        var s = abs % 60;
        return $"{m:00}:{s:00}";
    }

    private PresetSettings CaptureSettings() => new()
    {
        TimerMin = (int)TimerMinutes,
        TimerSec = (int)TimerSeconds,
        Countup = CountUpAfterZero,
        ColorNormal = ColorNormal,
        ColorWarning = ColorWarning,
        TimeWarning = (int)WarningSeconds,
        ColorDanger = ColorDanger,
        TimerSize = TimerSize,
        KanpeToggle = ActiveKanpeSlot != 0,
        KanpeText1 = KanpeText1,
        KanpeText2 = KanpeText2,
        KanpeText3 = KanpeText3,
        KanpeText4 = KanpeText4,
        KanpeFontSize = KanpeFontSize,
        KanpeColor = KanpeColor,
        BgColor = BgColor,
        ClockToggle = ClockVisible,
        ClockSize = ClockSize,
        ClockPosition = ClockPosition,
        ClockColor = ClockColor
    };

    private void ApplySettings(PresetSettings s)
    {
        TimerMinutes = s.TimerMin;
        TimerSeconds = s.TimerSec;
        CountUpAfterZero = s.Countup;
        ColorNormal = s.ColorNormal;
        ColorWarning = s.ColorWarning;
        WarningSeconds = s.TimeWarning;
        ColorDanger = s.ColorDanger;
        TimerSize = s.TimerSize;
        KanpeVisible = false;
        ActiveKanpeSlot = 0;
        KanpeText1 = s.KanpeText1;
        KanpeText2 = s.KanpeText2;
        KanpeText3 = s.KanpeText3;
        KanpeText4 = s.KanpeText4;
        KanpeFontSize = s.KanpeFontSize;
        KanpeColor = s.KanpeColor;
        BgColor = s.BgColor;
        ClockVisible = s.ClockToggle;
        ClockSize = s.ClockSize;
        ClockPosition = s.ClockPosition;
        ClockColor = s.ClockColor;
    }

    private void RefreshPresetLabels()
    {
        for (var i = 1; i <= 5; i++)
        {
            var settings = SettingsStore.LoadPreset(i);
            SetPresetLabel(i, settings is null ? $"未設定 {i}" : FormatPresetLabel(settings));
        }
    }

    private void SetPresetLabel(int index, string label)
    {
        switch (index)
        {
            case 1: PresetLabel1 = label; break;
            case 2: PresetLabel2 = label; break;
            case 3: PresetLabel3 = label; break;
            case 4: PresetLabel4 = label; break;
            case 5: PresetLabel5 = label; break;
        }
    }

    private static string FormatPresetLabel(PresetSettings s)
    {
        var zeroAfter = s.Countup ? "あり" : "なし";
        return $"{s.TimerMin:00}:{s.TimerSec:00}\n警告{s.TimeWarning}秒 / 0秒後{zeroAfter}";
    }

    private void ShowToast(string message)
    {
        ToastMessage = message;
        IsToastVisible = true;
        _toastTimer ??= _dispatcher.CreateTimer();
        _toastTimer.Interval = TimeSpan.FromSeconds(2);
        _toastTimer.IsRepeating = false;
        _toastTimer.Tick -= OnToastTick;
        _toastTimer.Tick += OnToastTick;
        _toastTimer.Start();
    }

    private void OnToastTick(DispatcherQueueTimer sender, object args) => IsToastVisible = false;

    private static string GetAppVersion() => Updates.AppVersionReader.GetCurrentVersion();
}

public sealed record ClockPositionOption(ClockPosition Value, string Label);
