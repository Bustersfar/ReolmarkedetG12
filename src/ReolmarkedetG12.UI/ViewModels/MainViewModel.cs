using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RackViewModel RackViewModel { get; }
    public RenterViewModel VendorViewModel { get; }

    public MainViewModel(RackViewModel rackVm, RenterViewModel vendorVm)
    {
        RackViewModel = rackVm;
        VendorViewModel = vendorVm;
    }
}