using Stip.BeerBattle.Models;

namespace Stip.BeerBattle.Items;

public class GroupItem : GroupItemBase
{
    public static GroupItem From(Group group)
    {
        var item = new GroupItem();
        item.UpdateWith(group);
        return item;
    }
}
