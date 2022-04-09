using AutoMapper;
using Caliburn.Micro;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Models;
using System;

namespace Stip.Stipstonks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<object, PropertyChangedBase>()
                .ForMember(d => d.IsNotifying, o => o.Ignore());

            JsonModelsToModels();
            ModelsToJsonModels();
            ModelsToItems();
        }

        private void ModelsToItems()
        {
            CreateMap<Product, ProductItemBase>()
                .IncludeBase<object, PropertyChangedBase>()
                .ForMember(d => d.Product, o => o.MapFrom(s => s))
                .ForMember(d => d.PriceInCents, o => o.MapFrom(s => s.CurrentPriceInCents))
                .ForMember(d => d.PriceFormatHelper, o => o.Ignore());

            CreateMap<Product, ChartItem>()
                .IncludeBase<Product, ProductItemBase>();

            CreateMap<Product, InputItem>()
                .IncludeBase<Product, ProductItemBase>()
                .ForMember(d => d.Amount, o => o.Ignore())
                .ForMember(d => d.TotalPriceChangedCallback, o => o.Ignore());
        }

        private void JsonModelsToModels()
        {
            CreateMap<JsonModels.Data, Data>()
                .ForMember(d => d.Config, o => o.MapFrom(s => s));

            CreateMap<JsonModels.Data, Config>()
                .ForMember(
                    d => d.PriceUpdateInterval,
                    o => o.MapFrom(s => new TimeSpan(0, 0, s.PriceUpdateIntervalInSeconds)))
                .ForMember(
                    d => d.CrashInterval,
                    o => o.MapFrom(s => new TimeSpan(0, 0, s.CrashIntervalInSeconds)))
                .ForMember(
                    d => d.CrashDuration,
                        o => o.MapFrom(s => new TimeSpan(0, 0, s.CrashDurationInSeconds)));

            CreateMap<JsonModels.Product, Product>()
                .ForMember(d => d.CurrentPriceInCents, o => o.Ignore())
                .ForMember(d => d.Level, o => o.Ignore());
        }

        private void ModelsToJsonModels()
        {
            CreateMap<Data, JsonModels.Data>()
                .ForMember(
                    d => d.PriceUpdateIntervalInSeconds,
                    o => o.MapFrom(s => (int)s.Config.PriceUpdateInterval.TotalSeconds))
                .ForMember(
                    d => d.CrashIntervalInSeconds,
                    o => o.MapFrom(s => (int)s.Config.CrashInterval.TotalSeconds))
                .ForMember(
                    d => d.CrashDurationInSeconds,
                    o => o.MapFrom(s => (int)s.Config.CrashDuration.TotalSeconds))
                .ForMember(d => d.MaxPriceDeviationFactor, o => o.MapFrom(s => s.Config.MaxPriceDeviationFactor))
                .ForMember(d => d.PriceResolutionInCents, o => o.MapFrom(s => s.Config.PriceResolutionInCents))
                .ForMember(d => d.AllowPriceUpdatesDuringOrder, o => o.MapFrom(s => s.Config.AllowPriceUpdatesDuringOrder))
                .ForMember(d => d.WindowBackgroundColor, o => o.MapFrom(s => s.Config.WindowBackgroundColor))
                .ForMember(d => d.CrashChartWindowBackgroundColor, o => o.MapFrom(s => s.Config.CrashChartWindowBackgroundColor))
                .ForMember(d => d.PriceUpdateProgressBarColor, o => o.MapFrom(s => s.Config.PriceUpdateProgressBarColor))
                .ForMember(d => d.CrashProgressBarColor, o => o.MapFrom(s => s.Config.CrashProgressBarColor));

            CreateMap<Product, JsonModels.Product>();
        }
    }
}
