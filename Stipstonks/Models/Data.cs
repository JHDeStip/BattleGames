using System.Collections.Generic;

namespace Stip.Stipstonks.Models
{
    public class Data
    {
        public Config Config { get; set; }
        public List<Product> Products { get; set; } = new();
    }
}
