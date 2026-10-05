using System;
using System.Collections.Generic;
using System.Text;

namespace ReolmarkedetG12.Core.Models
{
    public class SaleLine
    {
        public int SaleLineId { get; set; }
        public int SaleId { get; set; }
        public int ItemId { get; set; }
        public decimal SalePrice { get; set; }
    }
}
