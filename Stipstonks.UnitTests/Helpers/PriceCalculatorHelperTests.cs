using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;
using System.Linq;

namespace Stip.Stipstonks.UnitTests.Helpers;

[TestClass]
public class PriceCalculatorHelperTests
{
    
    [TestMethod]
    public void CalculatePriceLevels_CorrectlyCalculatesPriceLevels()
    {
        var fixture = FixtureFactory.Create();

        var products = fixture.CreateMany<Product>(3).ToList();
        products[0].CurrentPriceInCents = 3;
        products[1].CurrentPriceInCents = 8;
        products[2].CurrentPriceInCents = 5;

        var referenceProducts = products.Select(x => x with { }).ToList();

        var target = fixture.Create<PriceCalculatorHelper>();

        target.CalculatePriceLevels(products);

        Assert.AreEqual(3 / (double)8, products[0].Level);
        Assert.AreEqual(1, products[1].Level);
        Assert.AreEqual(5 / (double)8, products[2].Level);

        for (int i = 0; i < referenceProducts.Count; ++i)
        {
            products[i].Level = referenceProducts[i].Level;
        }

        Assert.IsTrue(products.DeeplyEquals(referenceProducts));
    }

    [TestMethod]
    public void CalculatePriceLevels_CorrectlyCalculatesPriceLevelsWith0Cents()
    {
        var fixture = FixtureFactory.Create();

        var products = fixture
            .Build<Product>()
            .With(x => x.CurrentPriceInCents, 0)
            .CreateMany()
            .ToList();

        var referenceProducts = products.Select(x => x with { }).ToList();

        var target = fixture.Create<PriceCalculatorHelper>();

        target.CalculatePriceLevels(products);

        for (int i = 0; i < referenceProducts.Count; ++i)
        {
            Assert.AreEqual(0, products[i].Level);
            products[i].Level = referenceProducts[i].Level;
        }

        Assert.IsTrue(products.DeeplyEquals(referenceProducts));
    }

    [TestMethod]
    public void CrashProduct_CorrectlyCrashesProduct()
    {
        var fixture = FixtureFactory.Create();

        var product = fixture
           .Build<Product>()
           .With(x => x.BasePriceInCents, 123)
           .Create();

        var referenceProduct = product with { };

        var roundedPrice = fixture.Create<int>();

        var mockMathHelper = fixture.FreezeMock<MathHelper>();
        mockMathHelper
            .Setup(x => x.RoundToResolution(
                It.IsAny<double>(),
                It.IsAny<double>()))
            .Returns(roundedPrice);

        var target = fixture.Create<PriceCalculatorHelper>();

        target.CrashProduct(
            product,
            0.66,
            7);

        Assert.AreEqual(roundedPrice, product.CurrentPriceInCents);
        Assert.AreEqual(0, product.VirtualAmountSold);

        mockMathHelper.Verify(
            x => x.RoundToResolution(
                It.Is<double>(y => NumberComparison.AreRelativelyClose(41.82, y, NumberComparison.DefaultEpsilon)),
                7),
            Times.Once);

        product.CurrentPriceInCents = referenceProduct.CurrentPriceInCents;
        product.VirtualAmountSold = referenceProduct.VirtualAmountSold;
        Assert.IsTrue(product.DeeplyEquals(referenceProduct));
    }

    [TestMethod]
    public void SetBasePriceForProduct_CorrectlySetsBasePrice()
    {
        var fixture = FixtureFactory.Create();

        var product = fixture.Create<Product>();
        var referenceProduct = product with { };

        var target = fixture.Create<PriceCalculatorHelper>();

        target.SetBasePriceForProduct(product);

        Assert.AreEqual(referenceProduct.BasePriceInCents, product.CurrentPriceInCents);

        product.CurrentPriceInCents = referenceProduct.CurrentPriceInCents;
        Assert.IsTrue(product.DeeplyEquals(referenceProduct));
    }

    [TestMethod]
    public void ResetProduct_CorrectlyResetsProduct()
    {
        var fixture = FixtureFactory.Create();

        var product = fixture.Create<Product>();
        var referenceProduct = product with { };

        var target = fixture.Create<PriceCalculatorHelper>();

        target.ResetProduct(product);

        Assert.AreEqual(referenceProduct.BasePriceInCents, product.CurrentPriceInCents);
        Assert.AreEqual(0, product.VirtualAmountSold);
        Assert.AreEqual(0, product.TotalAmountSold);

        product.CurrentPriceInCents = referenceProduct.CurrentPriceInCents;
        product.VirtualAmountSold = referenceProduct.VirtualAmountSold;
        product.TotalAmountSold = referenceProduct.TotalAmountSold;
        Assert.IsTrue(product.DeeplyEquals(referenceProduct));
    }

    [DataTestMethod]
    [DataRow(4, 346.892405063291139)]
    [DataRow(7, 349.88562091503268)]
    public void SetNewPriceForProduct_CorrectlySetsPrice(
        int virtualAmountSold,
        double expectedNewPrice)
    {
        var fixture = FixtureFactory.Create();

        var product = fixture
            .Build<Product>()
            .With(x => x.VirtualAmountSold, virtualAmountSold)
            .With(x => x.BasePriceInCents, 345)
            .Create();
        var referenceProduct = product with { };

        var mockMathHelper = fixture.FreezeMock<MathHelper>();
        mockMathHelper
            .Setup(x => x.RoundToResolution(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(789.9);

        var target = fixture.Create<PriceCalculatorHelper>();

        target.SetNewPriceForProduct(
            product,
            123,
            234,
            4.5,
            1.3,
            7);

        Assert.AreEqual(789, product.CurrentPriceInCents);

        mockMathHelper.Verify(
            x => x.RoundToResolution(
                It.Is<double>(
                    y => NumberComparison.AreRelativelyClose(
                        expectedNewPrice,
                        y,
                        NumberComparison.DefaultEpsilon)),
                7),
            Times.Once);

        mockMathHelper.VerifyNoOtherCalls();
    }
}
