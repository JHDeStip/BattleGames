using Stip.BattleGames.Common;
using System;

namespace Stip.Stipstonks.Helpers
{
    public class MathHelper : IInjectable
    {
        public virtual double RoundToResolution(
            double value,
            double resolution)
            => resolution <= 0
            ? value
            : (int)Math.Round(value / (double)resolution, MidpointRounding.AwayFromZero) * resolution;
    }
}
