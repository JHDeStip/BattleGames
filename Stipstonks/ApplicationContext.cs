using Stip.BattleGames.Common;
using Stip.Stipstonks.Models;
using System.Collections.Generic;

namespace Stip.Stipstonks;

public class ApplicationContext : IInjectable
{
    public Config Config { get; set; }
    public IReadOnlyList<Product> Products { get; set; } = [];
    public bool HasCrashed { get; set; }
}
