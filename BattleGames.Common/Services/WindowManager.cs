﻿using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Stip.BattleGames.Common.Windows;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BattleGames.Common.Services;

public class WindowManager(
    AppBase _app)
    : IInjectable
{
    public virtual async Task<Task> ShowWindowAsync<T>(
        WindowBase<T> window,
        CancellationToken ct)
        where T : ViewModelBase
    {
        var viewModel = window.ViewModel;

        var taskCompletionSource = new TaskCompletionSource();

        var canClose = false;

        window.Closing += (_, e) =>
        {
            if (canClose)
            {
                taskCompletionSource.SetResult();
                return;
            }

            e.Cancel = true;
            Dispatcher.UIThread.Post(async () =>
            {
                if (await viewModel.CanDeactivateAsync(CancellationToken.None))
                {
                    await viewModel.DeactivateAsync(CancellationToken.None);
                    canClose = true;
                    window.Close();
                }
            });               
        };

        if (_app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime applicationLifetime
            && applicationLifetime.MainWindow is null)
        {
            applicationLifetime.MainWindow = window;
        }

        window.Show();

        if (!viewModel.IsInitialized)
        {
            await viewModel.InitializeAsync(ct);
        }

        await viewModel.ActivateAsync(ct);

        return taskCompletionSource.Task;
    }
}
