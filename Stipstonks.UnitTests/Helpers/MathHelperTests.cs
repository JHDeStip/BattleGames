using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Helpers;

namespace Stip.Stipstonks.UnitTests.Helpers
{
    [TestClass]
    public class MathHelperTests
    {
        [DataTestMethod]
        [DataRow(0, 0.3, 0)]
        [DataRow(4, 0.3, 3.9)]
        [DataRow(3.9, 0.3, 3.9)]
        [DataRow(4.1, 0.3, 4.2)]
        [DataRow(4.2, 0.3, 4.2)]
        [DataRow(4, 0, 4)]
        [DataRow(4, -0.3, 4)]
        [DataRow(-4, 0.3, -3.9)]
        [DataRow(-3.9, 0.3, -3.9)]
        [DataRow(-4.1, 0.3, -4.2)]
        [DataRow(-4.2, 0.3, -4.2)]
        [DataRow(-4, 0, -4)]
        [DataRow(-4, -0.3, -4)]
        public void RoundToResolution_CorrectlyRounds(
            double value,
            double valueToRoundTo,
            double expected)
        {
            var target = new MathHelper();

            var actual = target.RoundToResolution(
                value,
                valueToRoundTo);

            Assert.AreEqual(expected, actual, NumberComparison.DefaultEpsilon);
        }
    }
}
