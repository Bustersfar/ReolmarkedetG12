using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class SaleDisplayItem : ViewModelBase
{
    private decimal _amount;
    private string _description = string.Empty;

    public int SaleId { get; set; }
    public int RackNumber { get; set; }
    public string RenterName { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
}