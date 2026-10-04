using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RackViewModel RackViewModel { get; }
    public RenterViewModel RenterViewModel { get; }
    public SalesViewModel SalesViewModel { get; }
    public SearchSalesViewModel SearchSalesViewModel { get; }
    public MonthlyStatementViewModel MonthlyStatementViewModel { get; }

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public MainViewModel(
        RackViewModel rackVm,
        RenterViewModel renterVm,
        SalesViewModel salesVm,
        SearchSalesViewModel searchSalesVm,
        MonthlyStatementViewModel monthlyStatementVm)
    {
        RackViewModel = rackVm;
        RenterViewModel = renterVm;
        SalesViewModel = salesVm;
        SearchSalesViewModel = searchSalesVm;
        MonthlyStatementViewModel = monthlyStatementVm;

        RackViewModel.OnSendToCheckout = (rackNumbers, amount, renterName) =>
        {
            foreach (var rackNo in rackNumbers)
            {
                decimal itemAmount = amount / rackNumbers.Count;
                SalesViewModel.AddRentalPaymentItem(rackNo, itemAmount, renterName);
            }

            // Skifter direkte til Kasse / Salg-fanen (indeks 2)
            SelectedTabIndex = 2;
        };
    }
}