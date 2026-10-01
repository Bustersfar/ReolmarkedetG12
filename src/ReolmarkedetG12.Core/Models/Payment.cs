namespace ReolmarkedetG12.Core.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int RenterId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.MobilePay;
}
