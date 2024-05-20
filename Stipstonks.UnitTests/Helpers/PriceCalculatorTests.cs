using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.UnitTests.Helpers;

[TestClass]
public class PriceCalculatorTests
{
    [TestMethod]
    public void Crash_CorrectlyCrashes()
    {
        var fixture = FixtureFactory.Create();
        
        var products = fixture.CreateMany<Product>(3).ToList();

        var maxPriceDeviationFactor = fixture.Create<double>();
        var priceResolutionInCents = fixture.Create<int>();

        var mockSequence = new MockSequence();

        var mockPriceCalculatorHelper = fixture.FreezeMock<PriceCalculatorHelper>(MockBehavior.Strict);
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.CrashProduct(It.IsAny<Product>(), It.IsAny<double>(), It.IsAny<int>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.CrashProduct(It.IsAny<Product>(), It.IsAny<double>(), It.IsAny<int>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.CrashProduct(It.IsAny<Product>(), It.IsAny<double>(), It.IsAny<int>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.CalculatePriceLevels(It.IsAny<IEnumerable<Product>>()));

        var target = fixture.Create<PriceCalculator>();

        target.Crash(
            products,
            maxPriceDeviationFactor,
            priceResolutionInCents);

        mockPriceCalculatorHelper.Verify(x => x.CrashProduct(products[0], maxPriceDeviationFactor, priceResolutionInCents), Times.Once);
        mockPriceCalculatorHelper.Verify(x => x.CrashProduct(products[1], maxPriceDeviationFactor, priceResolutionInCents), Times.Once);
        mockPriceCalculatorHelper.Verify(x => x.CrashProduct(products[2], maxPriceDeviationFactor, priceResolutionInCents), Times.Once);

        mockPriceCalculatorHelper.Verify(x => x.CalculatePriceLevels(products), Times.Once);

        mockPriceCalculatorHelper.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void ResetPricesAfterCrash_CorrectlyResetsPrices()
    {
        var fixture = FixtureFactory.Create();

        var products = fixture.CreateMany<Product>(3).ToList();

        var mockSequence = new MockSequence();

        var mockPriceCalculatorHelper = fixture.FreezeMock<PriceCalculatorHelper>(MockBehavior.Strict);
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.SetBasePriceForProduct(It.IsAny<Product>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.SetBasePriceForProduct(It.IsAny<Product>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.SetBasePriceForProduct(It.IsAny<Product>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.CalculatePriceLevels(It.IsAny<IEnumerable<Product>>()));

        var target = fixture.Create<PriceCalculator>();

        target.ResetPricesAfterCrash(products);

        mockPriceCalculatorHelper.Verify(x => x.SetBasePriceForProduct(products[0]), Times.Once);
        mockPriceCalculatorHelper.Verify(x => x.SetBasePriceForProduct(products[1]), Times.Once);
        mockPriceCalculatorHelper.Verify(x => x.SetBasePriceForProduct(products[2]), Times.Once);

        mockPriceCalculatorHelper.Verify(x => x.CalculatePriceLevels(products), Times.Once);

        mockPriceCalculatorHelper.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void ResetEntirely_CorrectlyResets()
    {
        var fixture = FixtureFactory.Create();

        var products = fixture.CreateMany<Product>(3).ToList();

        var mockSequence = new MockSequence();

        var mockPriceCalculatorHelper = fixture.FreezeMock<PriceCalculatorHelper>(MockBehavior.Strict);
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.ResetProduct(It.IsAny<Product>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.ResetProduct(It.IsAny<Product>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.ResetProduct(It.IsAny<Product>()));
        mockPriceCalculatorHelper
            .InSequence(mockSequence)
            .Setup(x => x.CalculatePriceLevels(It.IsAny<IEnumerable<Product>>()));

        var target = fixture.Create<PriceCalculator>();

        target.ResetEntirely(products);

        mockPriceCalculatorHelper.Verify(x => x.ResetProduct(products[0]), Times.Once);
        mockPriceCalculatorHelper.Verify(x => x.ResetProduct(products[1]), Times.Once);
        mockPriceCalculatorHelper.Verify(x => x.ResetProduct(products[2]), Times.Once);

        mockPriceCalculatorHelper.Verify(x => x.CalculatePriceLevels(products), Times.Once);

        mockPriceCalculatorHelper.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void RecalculatePrices_CorrectlyCalculatesPrices(
        bool areMinAndMaxSoldEqual)
    {
        var fixture = FixtureFactory.Create();

        var maxPriceDeviationFactor = fixture.Create<double>();
        var priceResolutionInCents = fixture.Create<int>();

        var products = fixture.CreateMany<Product>(4).ToList();
        if (areMinAndMaxSoldEqual)
        {
            foreach (var product in products)
            {
                product.VirtualAmountSold = 123;
            }
        }
        else
        {
            products[0].VirtualAmountSold = 123;
            products[1].VirtualAmountSold = 10;
            products[2].VirtualAmountSold = 234;
            products[3].VirtualAmountSold = 98;
        }

        var mockPriceCalculatorHelper = fixture.FreezeMock<PriceCalculatorHelper>();

        var target = fixture.Create<PriceCalculator>();

        target.RecalculatePrices(products, maxPriceDeviationFactor, priceResolutionInCents);

        if (areMinAndMaxSoldEqual)
        {
            foreach (var product in products)
            {
                mockPriceCalculatorHelper.Verify(x => x.SetBasePriceForProduct(product), Times.Once);
            }
        }
        else
        {
            foreach (var product in products)
            {
                mockPriceCalculatorHelper.Verify(
                    x => x.SetNewPriceForProduct(
                        product,
                        10,
                        234,
                        116.25,
                        maxPriceDeviationFactor,
                        priceResolutionInCents),
                    Times.Once);
            }
        }

        mockPriceCalculatorHelper.Verify(x => x.CalculatePriceLevels(products), Times.Once);

        mockPriceCalculatorHelper.VerifyNoOtherCalls();
    }
}
