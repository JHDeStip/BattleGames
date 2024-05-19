using Avalonia.Threading;
using Stip.Stipstonks.Dialogs;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Services
{
    public class DialogService(
        App _app)
        : IInjectable
    {
        public virtual Task ShowErrorAsync(string title, string message)
            => Dispatcher.UIThread.InvokeAsync(
                () => new BasicDialog(title, message, BasicDialogButton.Close)
                    .ShowDialog(_app.GetLastOpenedWindow()));

        public virtual Task ShowErrorAsync(string message)
            => ShowErrorAsync(UIStrings.Global_Error, message);

        public virtual Task<bool> ShowYesNoDialogAsync(string title, string message)
            => Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new BasicDialog(title, message, BasicDialogButton.Yes, BasicDialogButton.No);
                await dialog.ShowDialog(_app.GetLastOpenedWindow());
                return dialog.Result == BasicDialogButton.Yes;
            });
    }
}
