using KmTimer.Updates;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KmTimer.Services;

public static class OnlineUpdateUiHelper
{
    private static readonly OnlineUpdateCoordinator Coordinator = new();
    private static int _busy;

    public static async Task RunAsync(
        XamlRoot? xamlRoot,
        Action<string> setStatus,
        Func<Task>? beforeExitAsync = null)
    {
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
        {
            setStatus("更新処理が既に実行中です。");
            return;
        }

        var dispatcher = DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException("UI スレッド以外から更新を開始できません。");

        void SetStatusSafe(string message)
        {
            if (dispatcher.HasThreadAccess)
            {
                setStatus(message);
                return;
            }

            dispatcher.TryEnqueue(() => setStatus(message));
        }

        try
        {
            if (xamlRoot is null)
            {
                SetStatusSafe("画面の準備が完了していません。もう一度お試しください。");
                return;
            }

            var progress = new Progress<OnlineUpdateProgress>(p => SetStatusSafe(p.Message));
            var result = await Coordinator.RunAsync(
                message => ConfirmOnUiAsync(dispatcher, xamlRoot, message),
                progress,
                beforeExitAsync,
                () =>
                {
                    if (dispatcher.HasThreadAccess)
                        Application.Current.Exit();
                    else
                        dispatcher.TryEnqueue(() => Application.Current.Exit());
                });

            SetStatusSafe(result.Message);
        }
        catch (Exception ex)
        {
            SetStatusSafe($"更新に失敗しました: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _busy, 0);
        }
    }

    private static Task<bool> ConfirmOnUiAsync(DispatcherQueue dispatcher, XamlRoot xamlRoot, string message)
    {
        if (dispatcher.HasThreadAccess)
            return ConfirmAsync(xamlRoot, message);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var queued = dispatcher.TryEnqueue(() =>
        {
            _ = CompleteConfirmAsync(tcs, xamlRoot, message);
        });

        if (!queued)
            tcs.TrySetResult(false);

        return tcs.Task;
    }

    private static async Task CompleteConfirmAsync(
        TaskCompletionSource<bool> tcs,
        XamlRoot xamlRoot,
        string message)
    {
        try
        {
            tcs.TrySetResult(await ConfirmAsync(xamlRoot, message));
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
    }

    private static async Task<bool> ConfirmAsync(XamlRoot xamlRoot, string message)
    {
        var dialog = new ContentDialog
        {
            Title = "オンライン更新",
            Content = message,
            PrimaryButtonText = "更新する",
            CloseButtonText = "キャンセル",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
}
