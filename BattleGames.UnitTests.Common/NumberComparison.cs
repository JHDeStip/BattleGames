using System;

namespace Stip.BattleGames.UnitTestsCommon
{
    public static class NumberComparison
    {
        public const double DefaultEpsilon = 0.00000001;

        public static bool AreRelativelyClose(double a, double b, double epsilon = DefaultEpsilon)
        {
            if (a == b)
            {
                return true;
            }

            var absDiff = Math.Abs(a - b);

            if (a == default
                || b == default)
            {
                return absDiff <= epsilon;
            }

            var absA = Math.Abs(a);
            var absB = Math.Abs(b);

            if (absA + absB < double.Epsilon)
            {
                return absDiff <= epsilon * double.Epsilon;
            }

            return absDiff / Math.Min(absA + absB, double.MaxValue) <= epsilon;
        }
    }
}
