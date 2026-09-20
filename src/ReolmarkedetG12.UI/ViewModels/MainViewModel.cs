using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RackViewModel RackViewModel { get; }
    public VendorViewModel VendorViewModel { get; }

    public MainViewModel(RackViewModel rackVm, VendorViewModel vendorVm)
    {
        RackViewModel = rackVm;
        VendorViewModel = vendorVm;
    }
}