using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class RackDisplayItem : ViewModelBase
{
    public Rack Rack { get; }

    private decimal _monthRent;
    public decimal MonthRent
    {
        get => _monthRent;
        set { _monthRent = value; OnPropertyChanged(); }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    private DateTime? _terminationDate;
    public DateTime? TerminationDate
    {
        get => _terminationDate;
        set { _terminationDate = value; OnPropertyChanged(); }
    }

    public RackDisplayItem(Rack rack)
    {
        Rack = rack;
    }

    public void RefreshStatus() => OnPropertyChanged(nameof(Rack));
}