using Caliburn.Micro;
using Stip.Stipstonks.Models;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.Helpers
{
    public class PriceCalculator : IInjectable
    {
        public PriceCalculatorHelper PriceCalculatorHelper { get; set; }

        public virtual void Crash(
            IEnumerable<Product> products,
            double maxPriceDeviationFactor,
            int priceResulutionInCents)
        {
            products.Apply(
                x => PriceCalculatorHelper.CrashProduct(
                    x,
                    maxPriceDeviationFactor,
                    priceResulutionInCents));

            PriceCalculatorHelper.CalculatePriceLevels(products);
        }

        public virtual void ResetPricesAfterCrash(IEnumerable<Product> products)
        {
            products.Apply(PriceCalculatorHelper.SetBasePriceForProduct);
            PriceCalculatorHelper.CalculatePriceLevels(products);
        }

        public virtual void ResetEntirely(IEnumerable<Product> products)
        {
            products.Apply(PriceCalculatorHelper.ResetProduct);
            PriceCalculatorHelper.CalculatePriceLevels(products);
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
                products.Apply(PriceCalculatorHelper.SetBasePriceForProduct);
                PriceCalculatorHelper.CalculatePriceLevels(products);
                return;
            }

            var averageAmountSold = products.Sum(x => x.VirtualAmountSold) / (double)products.Count();

            products.Apply(
                x => PriceCalculatorHelper.SetNewPriceForProduct(
                    x,
                    minAmountSold,
                    maxAmountSold,
                    averageAmountSold,
                    maxPriceDeviationFactor,
                    priceResulutionInCents));

            PriceCalculatorHelper.CalculatePriceLevels(products);
        }
    }
}
