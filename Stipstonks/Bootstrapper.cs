using Caliburn.Micro;
using Microsoft.Extensions.DependencyInjection;
using Stip.Stipstonks.Common;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Services;
using Stip.Stipstonks.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Stip.Stipstonks
{
    public class Bootstrapper : BootstrapperBase
    {
        private const double InitialWindowWidth = 1200;
        private const double InitialWindowHeight = 600;

        private ServiceProvider _serviceProvider;
        private ServiceScopeFactory _serviceScopeFactory;

        public Bootstrapper()
        {
            SetCulture();
            Initialize();
        }

        protected override void Configure()
        {
            _serviceProvider = ConfigurServiceProvider();
            _serviceScopeFactory = _serviceProvider.GetRequiredService<ServiceScopeFactory>();
        }

        protected override object GetInstance(Type service, string key)
            => key == null
                ? _serviceProvider.GetRequiredService(service)
                : _serviceProvider.GetRequiredKeyedService(service, key);

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            if (!(await InitializeApplicationContextAsync()).IsSuccess)
            {
                Application.Current.Shutdown(1);
                return;
            }

            await InitializeDisableUIServiceAsync();

            var settings = new Dictionary<string, object>
            {
                { "SizeToContent", SizeToContent.Manual },
                { "Width", InitialWindowWidth },
                { "Height", InitialWindowHeight }
            };

            await DisplayRootViewForAsync<InputWindowViewModel>(settings);

            await ShowChartWindowAsync();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            _serviceProvider.Dispose();
            base.OnExit(sender, e);
        }

        private static void SetCulture()
        {
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.CurrencySymbol = UIStrings.Global_CurrencySymbol;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.CurrentCulture = culture;

            var uiCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            uiCulture.NumberFormat.CurrencySymbol = UIStrings.Global_CurrencySymbol;

            CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
            CultureInfo.CurrentUICulture = uiCulture;
        }

        private static ServiceProvider ConfigurServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            DIModule.RegisterServices(serviceCollection, (IApp)Application.Current);

            var serviceProviderOptions = new ServiceProviderOptions();
#if DEBUG
            serviceProviderOptions.ValidateScopes = true;
            serviceProviderOptions.ValidateOnBuild = true;
#endif
            return serviceCollection.BuildServiceProvider(serviceProviderOptions);
        }

        private async Task<ActionResult> InitializeApplicationContextAsync()
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var loadResult = await scope
                    .GetRequiredService<DataPersistenceHelper>()
                    .LoadDataAsync();
                if (!loadResult.IsSuccess)
                {
                    scope
                        .GetRequiredService<DialogService>()
                        .ShowError(UIStrings.Error_CannotLoadData);
                    return ActionResult.Failure;
                }

                var applicationContext = scope.GetRequiredService<ApplicationContext>();
                scope
                    .GetRequiredService<PriceCalculator>()
                    .RecalculatePrices(
                        applicationContext.Products,
                        applicationContext.Config.MaxPriceDeviationFactor,
                        applicationContext.Config.PriceResolutionInCents);
            }

            return ActionResult.Success;
        }

        private async Task InitializeDisableUIServiceAsync()
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                scope
                    .GetRequiredService<DisableUIService>()
                    .PushViewModel(
                        scope.GetRequiredService<InputWindowViewModel>());
            }
        }

        private async Task ShowChartWindowAsync()
        {
            var settings = new Dictionary<string, object>
            {
                { "SizeToContent", SizeToContent.Manual },
                { "Width", InitialWindowWidth },
                { "Height", InitialWindowHeight }
            };

            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                await scope
                    .GetRequiredService<IWindowManager>()
                    .ShowWindowAsync(
                        scope.GetRequiredService<ChartWindowViewModel>(),
                        null,
                        settings);
            }
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);

            const int maxStackTraceLength = 2000;
            var stackTrace = e.Exception.StackTrace;

            MessageBox.Show(stackTrace?.Length <= maxStackTraceLength
                ? stackTrace
                : stackTrace?[..maxStackTraceLength]);
        }
    }
}
