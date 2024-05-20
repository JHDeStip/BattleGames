using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BattleGames.Common.Windows;

public abstract class ViewModelBase : ObservableObject
{
    public Window Window { get; set; }
    public bool IsInitialized { get; private set; }

    public virtual ValueTask InitializeAsync(CancellationToken ct)
    {
        IsInitialized = true;
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask ActivateAsync(CancellationToken ct)
        => ValueTask.CompletedTask;

    public virtual ValueTask DeactivateAsync(CancellationToken ct)
        => ValueTask.CompletedTask;

    public virtual ValueTask<bool> CanDeactivateAsync(CancellationToken ct)
        => ValueTask.FromResult(true);

    public virtual ValueTask CloseAsync(CancellationToken ct)
    {
        Window?.Close();
        return ValueTask.CompletedTask;
    }
}
