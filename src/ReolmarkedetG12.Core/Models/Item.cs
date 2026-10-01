using System;
using System.Collections.Generic;
using System.Text;

namespace ReolmarkedetG12.Core.Models
{
    public class Item
    {
        public int ItemId { get; set; }
        public int ItemNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int RackId { get; set; }
    }
}
