using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ReolerViewModel ReolerViewModel { get; }
    public KunderViewModel KunderViewModel { get; }

    public MainViewModel(ReolerViewModel reolerVm, KunderViewModel kunderVm)
    {
        ReolerViewModel = reolerVm;
        KunderViewModel = kunderVm;
    }
}
