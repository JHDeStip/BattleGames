using Stip.BattleGames.Common;
using Stip.BeerBattle.Models;
using System.Collections.Generic;

namespace Stip.BeerBattle;

public class ApplicationContext : IInjectable
{
    public Config Config { get; set; }
    public IReadOnlyList<Group> Groups { get; set; } = [];
    public IReadOnlyList<Product> Products { get; set; } = [];
}
