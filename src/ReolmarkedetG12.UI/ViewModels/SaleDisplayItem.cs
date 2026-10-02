using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

// Én række i søge/ret-gridet på "Søg/ret salg"-fanen.
public class SaleDisplayItem : ViewModelBase
{
    private decimal _amount;
    private string _description = string.Empty;

    public int SaleId { get; set; }
    public int RackNumber { get; set; }
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