using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

// Prototype: én række i søge/ret-panelet på Salg-fanen.
// Bliver senere erstattet af en rigtig visning af en Sale-model fra databasen.
public class SaleDisplayItem : ViewModelBase
{
    private decimal _amount;
    private string _description = string.Empty;

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