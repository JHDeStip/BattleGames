using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using System.Collections.Generic;

namespace Stip.Stipstonks.UnitTests.Items
{
    [TestClass]
    public class ChartItemTests
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void PriceInCents_NotifiesPriceStringUpdate(
            bool hasValueChanged)
        {
            var target = new ChartItem(null)
            {
                PriceInCents = 123
            };

            var notifiedProperties = new List<string>(2);

            target.PropertyChanged += (s, e) => notifiedProperties.Add(e.PropertyName);

            target.PriceInCents = hasValueChanged
                ? target.PriceInCents + 1
                : target.PriceInCents;

            Assert.AreEqual(hasValueChanged, notifiedProperties.Contains(nameof(target.PriceString)));
        }

        [TestMethod]
        public void PriceString_ReturnsCorrectString()
        {
            var fixture = FixtureFactory.Create();

            var totalPriceString = fixture.Create<string>();

            var mockPriceFormatHelper = fixture.FreezeMock<PriceFormatHelper>();
            mockPriceFormatHelper
                .Setup(x => x.Format(It.IsAny<int>()))
                .Returns(totalPriceString);

            var target = fixture
                .Build<ChartItem>()
                .With(x => x.PriceInCents, 123)
                .Create();

            var actual = target.PriceString;

            Assert.AreEqual(totalPriceString, actual);

            mockPriceFormatHelper.Verify(x => x.Format(123), Times.Once);

            mockPriceFormatHelper.VerifyNoOtherCalls();
        }
    }
}
