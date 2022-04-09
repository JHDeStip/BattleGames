using Caliburn.Micro;
using System.Windows;

namespace Stip.Stipstonks.Services
{
    public class DialogService : IInjectable
    {
        public virtual void ShowError(string title, string message)
            => Execute.OnUIThread(() => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error));

        public virtual void ShowError(string message)
            => ShowError(UIStrings.Global_Error, message);

        public virtual bool ShowYesNoDialog(string title, string message)
        {
            var result = false;

            Execute.OnUIThread(
                () => result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question)
                == MessageBoxResult.Yes);

            return result;
        }
    }
}
