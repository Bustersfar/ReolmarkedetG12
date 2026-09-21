using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class RackDisplayItem : ViewModelBase
{
    public Rack Rack { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public RackDisplayItem(Rack rack)
    {
        Rack = rack;
    }
}