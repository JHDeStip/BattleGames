using Stip.BattleGames.Common;
using Stip.Stipstonks.Models;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.Helpers
{
    public class PriceCalculator(
        PriceCalculatorHelper _priceCalculatorHelper)
        : IInjectable
    {
        public virtual void Crash(
            IEnumerable<Product> products,
            double maxPriceDeviationFactor,
            int priceResulutionInCents)
        {
            foreach (var product in products)
            {
                _priceCalculatorHelper.CrashProduct(
                    product,
                    maxPriceDeviationFactor,
                    priceResulutionInCents);
            }

            _priceCalculatorHelper.CalculatePriceLevels(products);
        }

        public virtual void ResetPricesAfterCrash(IEnumerable<Product> products)
        {
            foreach (var product in products)
            {
                _priceCalculatorHelper.SetBasePriceForProduct(product);
            }

            _priceCalculatorHelper.CalculatePriceLevels(products);
        }

        public virtual void ResetEntirely(IEnumerable<Product> products)
        {
            foreach (var product in products)
            {
                _priceCalculatorHelper.ResetProduct(product);
            }

            _priceCalculatorHelper.CalculatePriceLevels(products);
        }

        public virtual void RecalculatePrices(
            IEnumerable<Product> products,
            double maxPriceDeviationFactor,
            int priceResulutionInCents)
        {
            var minAmountSold = products.Min(x => x.VirtualAmountSold);
            var maxAmountSold = products.Max(x => x.VirtualAmountSold);
            if (maxAmountSold == minAmountSold)
            {
                // Do not calculate, the devisor for priceChangeFactor will be 0.
                // This happens when all products have an equal amount sold.
                // It is also the case on startup and after each crash
                // so we need to explicitly set base price.
                foreach (var product in products)
                {
                    _priceCalculatorHelper.SetBasePriceForProduct(product);
                }

                _priceCalculatorHelper.CalculatePriceLevels(products);
                return;
            }

            var averageAmountSold = products.Sum(x => x.VirtualAmountSold) / (double)products.Count();

            foreach (var product in products)
            {
                _priceCalculatorHelper.SetNewPriceForProduct(
                    product,
                    minAmountSold,
                    maxAmountSold,
                    averageAmountSold,
                    maxPriceDeviationFactor,
                    priceResulutionInCents);
            }

            _priceCalculatorHelper.CalculatePriceLevels(products);
        }
    }
}
