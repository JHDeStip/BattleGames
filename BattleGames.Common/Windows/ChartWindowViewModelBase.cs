using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Stip.BattleGames.Common.Messages;
using System.Threading.Tasks;
using System.Threading;

namespace Stip.BattleGames.Common.Windows;

public abstract class ChartWindowViewModelBase(
    IMessenger _messenger)
    : ViewModelBase,
    IRecipient<ToggleChartWindowStateMessage>
{
    private string _backgroundColor;
    public string BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }

    private WindowState _windowState;
    public WindowState WindowState { get => _windowState; set => SetProperty(ref _windowState, value); }

    private WindowState _previousWindowState;
    private bool canClose;

    public override async ValueTask ActivateAsync(CancellationToken ct)
    {
        await base.ActivateAsync(ct);
        _messenger.RegisterAll(this);
    }

    public override async ValueTask DeactivateAsync(CancellationToken ct)
    {
        await base.DeactivateAsync(ct);
        _messenger.UnregisterAll(this);
    }

    public override async ValueTask<bool> CanDeactivateAsync(CancellationToken ct)
        => canClose
        && await base.CanDeactivateAsync(ct);

    public override ValueTask CloseAsync(CancellationToken ct)
    {
        canClose = true;
        return base.CloseAsync(ct);
    }

    public void Receive(ToggleChartWindowStateMessage _)
        => Dispatcher.UIThread.Invoke(() =>
        {
            if (_windowState == WindowState.FullScreen)
            {
                WindowState = _previousWindowState;
                return;
            }

            _previousWindowState = WindowState;

            // You cannot go form minimized to full screen directly.
            // Transition through Normal first.
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            WindowState = WindowState.FullScreen;
        });
}
