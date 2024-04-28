﻿using CommunityToolkit.Mvvm.Messaging;
using Stip.Stipstonks.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public class CrashManager : StonkMarketEventManagerBase
    {
        public DataPersistenceHelper DataPersistenceHelper { get; set; }
        public DelayHelper DelayHelper { get; set; }

        private Func<Task> _stonkMarketWillCrashAction;
        private Action _stonkMarketCrashEndedAction;

        public virtual void Start(
            Func<Task> stonkMarketWillCrashAction,
            Action stonkMarketCrashEndedAction)
        {
            _stonkMarketWillCrashAction = stonkMarketWillCrashAction;
            _stonkMarketCrashEndedAction = stonkMarketCrashEndedAction;

            Start(ApplicationContext.Config.CrashInterval);
        }

        protected override async Task OnTimerExpiredAsync(CancellationToken ct)
        {
            await _stonkMarketWillCrashAction();

            ApplicationContext.HasCrashed = true;

            PriceRecalculator.Crash(
                ApplicationContext.Products,
                ApplicationContext.Config.MaxPriceDeviationFactor,
                ApplicationContext.Config.PriceResolutionInCents);

            try
            {
                Messenger.Send<PricesUpdatedMessage>();

                await DataPersistenceHelper.SaveDataAsync();

                await DelayHelper.Delay(
                    ApplicationContext.Config.CrashDuration,
                    ct);

                PriceRecalculator.ResetPricesAfterCrash(ApplicationContext.Products);

                ApplicationContext.HasCrashed = false;

                Messenger.Send<PricesUpdatedMessage>();

                _stonkMarketCrashEndedAction();
            }
            finally
            {
                ApplicationContext.HasCrashed = false;
            }
        }
    }
}
