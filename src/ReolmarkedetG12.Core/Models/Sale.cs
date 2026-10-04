using System;

namespace ReolmarkedetG12.Core.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int? RackId { get; set; }
        public int? RenterId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public PaymentMethod PaymentMethod { get; set; }

        public Sale()
        {
        }

        public Sale(int? rackId, int? renterId, decimal amount, string description, PaymentMethod paymentMethod, DateTime? date = null)
        {
            RackId = rackId;
            RenterId = renterId;
            Amount = amount;
            Description = description;
            PaymentMethod = paymentMethod;
            Date = date ?? DateTime.UtcNow;
        }
    }
}