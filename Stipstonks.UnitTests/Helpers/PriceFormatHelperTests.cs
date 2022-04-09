using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.Stipstonks.Helpers;
using System.Globalization;
using System.Threading;

namespace Stip.Stipstonks.UnitTests.Helpers
{
    [TestClass]
    public class PriceFormatHelperTests
    {
        [DataTestMethod]
        [DataRow(0, "&0]00")]
        [DataRow(-0, "&0]00")]
        [DataRow(-123, "&-1]23")]
        [DataRow(123, "&1]23")]
        [DataRow(123456789, "&1234567]89")]
        [DataRow(-123456789, "&-1234567]89")]
        public void Format_CorrectlyFormatsPrice(
            int priceInCents,
            string expected)
        {
            string actual = null;

            var thread = new Thread(new ThreadStart(() =>
            {
                var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                culture.NumberFormat.CurrencyDecimalSeparator = "]";
                culture.NumberFormat.CurrencySymbol = "&";
                Thread.CurrentThread.CurrentCulture = culture;

                var target = new PriceFormatHelper();

                actual = target.Format(priceInCents);
            }));

            thread.Start();
            thread.Join();

            Assert.AreEqual(expected, actual);
        }
    }
}
