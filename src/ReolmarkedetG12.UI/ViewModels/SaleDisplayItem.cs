using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class SaleDisplayItem : ViewModelBase
{
    private decimal _totalAmount;

    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetProperty(ref _totalAmount, value);
    }
}