using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RackViewModel RackViewModel { get; }
    public RenterViewModel RenterViewModel { get; }
    public SalesViewModel SalesViewModel { get; }
    public SearchSalesViewModel SearchSalesViewModel { get; }

    public MainViewModel(RackViewModel rackVm, RenterViewModel RenterVm, SalesViewModel salesVm, SearchSalesViewModel searchSalesVm)
    {
        RackViewModel = rackVm;
        RenterViewModel = RenterVm;
        SalesViewModel = salesVm;
        SearchSalesViewModel = searchSalesVm;
    }
}