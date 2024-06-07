using Avalonia.Threading;
using Stip.BattleGames.Common.Dialogs;
using System.Threading.Tasks;

namespace Stip.BattleGames.Common.Services;

public class DialogService(
    AppBase _app)
    : IInjectable
{
    public virtual Task ShowErrorAsync(string title, string message)
    {
        var activeWindow = _app.GetActiveWindow();
        return activeWindow is null
            ? Task.CompletedTask
            : Dispatcher.UIThread.InvokeAsync(
                () => new BasicDialog(title, message, BasicDialogButton.Close)
                .ShowDialog(activeWindow));
    }

    public virtual Task ShowErrorAsync(string message)
        => ShowErrorAsync(UIStrings.Global_Error, message);

    public virtual Task<bool> ShowYesNoDialogAsync(string title, string message)
    {
        var activeWindow = _app.GetActiveWindow();
        return activeWindow is null
            ? Task.FromResult(false)
            : Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new BasicDialog(title, message, BasicDialogButton.Yes, BasicDialogButton.No);
                await dialog.ShowDialog(activeWindow);
                return dialog.Result == BasicDialogButton.Yes;
            });
    }
}
