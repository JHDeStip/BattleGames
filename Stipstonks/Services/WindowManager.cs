using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Stip.Stipstonks.Windows;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Services
{
    public class WindowManager(
        App _app)
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

            window.Closing += async (_, e) =>
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
}
