using System;

namespace ReolmarkedetG12.Core.Models
{
    public class SaleAuditLog
    {
        public int AuditId { get; set; }
        public int SaleId { get; set; }
        public string ActionType { get; set; } = string.Empty; // "UPDATE" eller "DELETE"
        public decimal OldAmount { get; set; }
        public decimal? NewAmount { get; set; }
        public string? OldDescription { get; set; }
        public string? NewDescription { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public SaleAuditLog()
        {
        }

        public SaleAuditLog(int saleId, string actionType, decimal oldAmount, decimal? newAmount, string? oldDescription, string? newDescription, DateTime? timestamp = null)
        {
            SaleId = saleId;
            ActionType = actionType;
            OldAmount = oldAmount;
            NewAmount = newAmount;
            OldDescription = oldDescription;
            NewDescription = newDescription;
            Timestamp = timestamp ?? DateTime.UtcNow;
        }
    }
}