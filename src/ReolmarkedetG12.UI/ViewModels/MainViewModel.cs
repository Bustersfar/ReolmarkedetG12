using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RackViewModel RackViewModel { get; }
    public RenterViewModel RenterViewModel { get; }

    public MainViewModel(RackViewModel rackVm, RenterViewModel RenterVm)
    {
        RackViewModel = rackVm;
        RenterViewModel = RenterVm;
    }
}