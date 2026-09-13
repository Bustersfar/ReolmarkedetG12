using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ShelfViewModel ShelfViewModel { get; }
    public VendorViewModel VendorViewModel { get; }

    public MainViewModel(ShelfViewModel shelfVm, VendorViewModel vendorVm)
    {
        ShelfViewModel = shelfVm;
        VendorViewModel = vendorVm;
    }
}
