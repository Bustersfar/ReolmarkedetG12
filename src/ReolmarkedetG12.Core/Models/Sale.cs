using System;
using System.Collections.Generic;
using System.Text;

namespace ReolmarkedetG12.Core.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<SaleLine> SaleLines { get; set; } = new List<SaleLine>();

    }
}
