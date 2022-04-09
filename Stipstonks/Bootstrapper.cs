using AutoMapper;
using Caliburn.Micro;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Stip.Stipstonks.Extensions;
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

        private IWindsorContainer _container;

        public Bootstrapper()
        {
            SetCulture();
            Initialize();
        }

        protected override void Configure()
            => _container = ConfigureContainer(ConfigureAutoMapper());

        protected override object GetInstance(Type service, string key)
            => key == null
                ? _container.Resolve(service)
                : _container.Resolve(key, service);

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await InitializeApplicationContextAsync();
            InitializeDisableUIService();

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
            _container.Dispose();
            base.OnExit(sender, e);
        }

        private static IMapper ConfigureAutoMapper()
        {
            var configuration = new MapperConfiguration(config =>
            {
                config.AddMaps(
                    typeof(AutoMapperProfile));
            });

#if DEBUG
            configuration.AssertConfigurationIsValid();
#endif

            configuration.CompileMappings();

            return configuration.CreateMapper();
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

        private static IWindsorContainer ConfigureContainer(IMapper mapper)
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

            container.Install(new DIModule(
                (IApp)Application.Current,
                mapper));

            return container;
        }

        private async Task InitializeApplicationContextAsync()
        {
            using (_container.ResolveComponent<DataPersistenceHelper>(out var dataPersistenceHelper))
            {
                var loadResult = await dataPersistenceHelper.LoadDataAsync();
                if (!loadResult.IsSuccess)
                {
                    using (_container.ResolveComponent<DialogService>(out var dialogService))
                    {
                        dialogService.ShowError(UIStrings.Error_CannotLoadData);
                    }
                    return;
                }
            }

            using (_container.ResolveComponent<PriceCalculator>(out var priceRecalculationHelper))
            using (_container.ResolveComponent<ApplicationContext>(out var applicationContext))
            {
                priceRecalculationHelper.RecalculatePrices(
                    applicationContext.Products,
                    applicationContext.Config.MaxPriceDeviationFactor,
                    applicationContext.Config.PriceResolutionInCents);
            }
        }

        private void InitializeDisableUIService()
        {
            using (_container.ResolveComponent<InputWindowViewModel>(out var inputWindowViewModel))
            using (_container.ResolveComponent<DisableUIService>(out var disableUIService))
            {
                disableUIService.PushViewModel(inputWindowViewModel);
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

            using (_container.ResolveComponent<ChartWindowViewModel>(out var chartWindowViewModel))
            using (_container.ResolveComponent<IWindowManager>(out var windowManager))
            {
                await windowManager.ShowWindowAsync(
                    chartWindowViewModel,
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
                : stackTrace?.Substring(0, maxStackTraceLength));
        }
    }
}
