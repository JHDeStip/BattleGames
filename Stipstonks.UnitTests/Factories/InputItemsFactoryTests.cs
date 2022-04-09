using AutoFixture;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Models;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.UnitTests.Factories
{
    [TestClass]
    public class InputItemsFactoryTests
    {
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(4)]
        [DataRow(3)]
        [DataRow(5)]
        public void Create_CorrectlyCreatesItems(
            int nExistingItems)
        {
            var fixture = FixtureFactory.Create();

            var products = fixture.CreateMany<Product>(3);
            var existingInputItems = fixture.CreateMany<InputItem>(nExistingItems).ToList();

            var inputItems = fixture
                .Build<InputItem>()
                .With(x => x.Amount, 0)
                .CreateMany(4)
                .ToList();

            var mockMapper = fixture.FreezeMock<IMapper>();
            mockMapper
                .Setup(x => x.Map<List<InputItem>>(It.IsAny<object>()))
                .Returns(inputItems);

            var priceFormatHelper = fixture.Freeze<PriceFormatHelper>();

            var target = fixture.Create<InputItemsFactory>();

            var totalPriceChangedCallback = () => { };

            var actual = target.Create(
                products,
                existingInputItems,
                totalPriceChangedCallback);

            Assert.AreSame(inputItems, actual);

            foreach (var item in actual)
            {
                Assert.AreSame(priceFormatHelper, item.PriceFormatHelper);
                Assert.AreSame(totalPriceChangedCallback, item.TotalPriceChangedCallback);
            }

            for (var i = 0; i < actual.Count; ++i)
            {
                var expectedAmount = i < existingInputItems.Count
                    ? existingInputItems[i].Amount
                    : 0;

                Assert.AreEqual(expectedAmount, actual[i].Amount);
            }
        }
    }
}
