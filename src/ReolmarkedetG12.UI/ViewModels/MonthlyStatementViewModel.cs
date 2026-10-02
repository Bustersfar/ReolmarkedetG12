using ReolmarkedetG12.UI.MVVM;
using ReolmarkedetG12.UI.Services;

namespace ReolmarkedetG12.UI.ViewModels;

// Skelet for nu - selve indholdet (hvad en månedsopgørelse faktisk skal vise) bygges senere.
public class MonthlyStatementViewModel : ViewModelBase
{
    public LockScreenViewModel Lock { get; }

    public MonthlyStatementViewModel(ISecureAreaService secureAreaService, IDialogService dialogService)
    {
        Lock = new LockScreenViewModel(secureAreaService, dialogService);
    }
}