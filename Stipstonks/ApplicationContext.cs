using Stip.Stipstonks.Models;
using System.Collections.Generic;

namespace Stip.Stipstonks
{
    public class ApplicationContext : IInjectable
    {
        public Config Config { get; set; }
        public List<Product> Products { get; set; } = new();
        public bool HasCrashed { get; set; }
    }
}
