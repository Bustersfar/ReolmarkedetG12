using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RackViewModel RackViewModel { get; }
    public RenterViewModel RenterViewModel { get; }
    public SalesViewModel SalesViewModel { get; }
    public SearchSalesViewModel SearchSalesViewModel { get; }
    public MonthlyStatementViewModel MonthlyStatementViewModel { get; }

    // Fanens plads i TabControl'en i MainWindow.xaml (0 = Kunder ... 4 = Månedsopgørelse)
    public const int MonthlyStatementTabIndex = 4;

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            // Når man åbner Månedsopgørelse, genberegnes opgørelsen, så man ikke ser
            // forældede tal efter f.eks. at have registreret nye salg på en anden fane.
            if (SetProperty(ref _selectedTabIndex, value) && value == MonthlyStatementTabIndex)
                MonthlyStatementViewModel.Refresh();
        }
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
    }
}