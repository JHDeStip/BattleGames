using Stip.BattleGames.Common.Items;
using Stip.BeerBattle.Models;

namespace Stip.BeerBattle.Items
{
    public abstract class GroupItemBase : ItemBase
    {
        public Group Group { get; set; }

        protected void UpdateWith(Group group)
        {
            Group = group;
            Name = group.Name;
            Color = group.Color;
        }
    }
}
