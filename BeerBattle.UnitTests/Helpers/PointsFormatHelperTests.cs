using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.BattleGames.UnitTestsCommon;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Models;
using System.Globalization;
using System.Threading;

namespace Stip.BeerBattle.UnitTests.Helpers;

[TestClass]
public class PointsFormatHelperTests
{
    [DataTestMethod]
    [DataRow(0, "0]00")]
    [DataRow(-0, "0]00")]
    [DataRow(123, "123]00")]
    [DataRow(-123, "-123]00")]
    [DataRow(1.05, "1]05")]
    [DataRow(1.035, "1]04")]
    [DataRow(1.045, "1]05")]
    [DataRow(1.333, "1]33")]
    [DataRow(-1.05, "-1]05")]
    [DataRow(-1.035, "-1]04")]
    [DataRow(-1.045, "-1]05")]
    [DataRow(-1.333, "-1]33")]
    public void Format_CorrectlyFormatsPriceWith2Decimals(
        double points,
        string expected)
    {
        var actual = Format(
            points,
            2);

        Assert.AreEqual(expected, actual);
    }

    [DataTestMethod]
    [DataRow(0, "0")]
    [DataRow(-0, "0")]
    [DataRow(123, "123")]
    [DataRow(-123, "-123")]
    [DataRow(2.5, "3")]
    [DataRow(1.5, "2")]
    [DataRow(1.4, "1")]
    [DataRow(-2.5, "-3")]
    [DataRow(-1.5, "-2")]
    [DataRow(-1.4, "-1")]
    public void Format_CorrectlyFormatsPriceWith0Decimals(
        double points,
        string expected)
    {
        var actual = Format(
            points,
            0);

        Assert.AreEqual(expected, actual);
    }

    private static string Format(
        double points,
        int decimals)
    {
        var fixture = FixtureFactory.Create();

        var config = fixture
            .Build<Config>()
            .With(x => x.TotalPointsNumberOfDecimals, decimals)
            .Create();

        string actual = null;

        var thread = new Thread(new ThreadStart(() =>
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = "]";
            Thread.CurrentThread.CurrentCulture = culture;

            var target = new PointsFormatHelper();
            target.Initialize(config);

            actual = target.Format((decimal)points);
        }));

        thread.Start();
        thread.Join();

        return actual;
    }
}
