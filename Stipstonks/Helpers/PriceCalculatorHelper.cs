using Stip.BattleGames.Common;
using Stip.Stipstonks.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.Helpers
{
    public class PriceCalculatorHelper(
        MathHelper _mathHelper)
        : IInjectable
    {
        public virtual void CalculatePriceLevels(IEnumerable<Product> products)
        {
            var maxPrice = products.Max(x => x.CurrentPriceInCents);

            if (maxPrice == 0)
            {
                foreach (var product in products)
                {
                    product.Level = 0;
                }
            }
            else
            {
                foreach (var product in products)
                {
                    product.Level = product.CurrentPriceInCents / (double)maxPrice;
                }
            }
        }

        public virtual void CrashProduct(
            Product product,
            double maxPriceDeviationFactor,
            int priceResulutionInCents)
        {
            product.CurrentPriceInCents = (int)_mathHelper.RoundToResolution(
                product.BasePriceInCents - product.BasePriceInCents * maxPriceDeviationFactor,
                priceResulutionInCents);

            product.VirtualAmountSold = 0;
        }

        public virtual void SetBasePriceForProduct(Product product)
            => product.CurrentPriceInCents = product.BasePriceInCents;

        public virtual void ResetProduct(Product product)
        {
            SetBasePriceForProduct(product);
            product.VirtualAmountSold = 0;
            product.TotalAmountSold = 0;
        }

        public virtual void SetNewPriceForProduct(
            Product product,
            int minAmountSold,
            int maxAmountSold,
            double averageAmountSold,
            double maxPriceDeviationFactor,
            int priceResulutionInCents)
        {
            var absoluteRelativeAmountSold = Math.Abs(product.VirtualAmountSold - averageAmountSold);

            var priceChangeFactor = absoluteRelativeAmountSold
                / ((product.VirtualAmountSold < averageAmountSold
                    ? minAmountSold
                    : maxAmountSold)
                    - averageAmountSold);

            var newPriceInCents =
                product.BasePriceInCents
                + product.BasePriceInCents
                    * priceChangeFactor
                    * maxPriceDeviationFactor;

            product.CurrentPriceInCents = (int)_mathHelper.RoundToResolution(
                newPriceInCents,
                priceResulutionInCents);
        }
    }
}
