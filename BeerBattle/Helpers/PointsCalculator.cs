using Stip.BattleGames.Common;
using Stip.BeerBattle.Models;
using System.Collections.Generic;
using System.Linq;

namespace Stip.BeerBattle.Helpers
{
    public class PointsCalculator : IInjectable
    {
        public virtual void CalculatePointLevels(IEnumerable<Group> groups)
        {
            var maxPoints = groups.Max(x => x.TotalPoints);

            if (maxPoints == 0)
            {
                foreach (var group in groups)
                {
                    group.Level = 1;
                }
            }
            else
            {
                foreach (var group in groups)
                {
                    group.Level = (double)group.TotalPoints / (double)maxPoints;
                }
            }
        }

        public virtual void Reset(IEnumerable<Group> groups)
        {
            foreach (var group in groups)
            {
                group.TotalPoints = 0;
            }

            CalculatePointLevels(groups);
        }
    }
}
