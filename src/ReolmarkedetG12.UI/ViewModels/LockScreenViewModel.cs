using ReolmarkedetG12.UI.MVVM;
using ReolmarkedetG12.UI.Services;

namespace ReolmarkedetG12.UI.ViewModels;

// Delt af enhver ViewModel, der skal ligge bag kodeords-spærren.
// IsUnlocked afspejler den fælles SecureAreaService, så man kun skal
// taste kodeordet én gang, uanset hvilket beskyttet faneblad man åbner først.
public class LockScreenViewModel : ViewModelBase
{
    private readonly ISecureAreaService _secureAreaService;
    private readonly IDialogService _dialogService;

    public bool IsUnlocked => _secureAreaService.IsUnlocked;

    private string _passwordInput = string.Empty;
    public string PasswordInput
    {
        get => _passwordInput;
        set => SetProperty(ref _passwordInput, value);
    }

    public RelayCommand UnlockCommand { get; }

    public LockScreenViewModel(ISecureAreaService secureAreaService, IDialogService dialogService)
    {
        _secureAreaService = secureAreaService;
        _dialogService = dialogService;

        _secureAreaService.PropertyChanged += (_, _) => OnPropertyChanged(nameof(IsUnlocked));

        UnlockCommand = new RelayCommand(_ => Unlock());
    }

    private void Unlock()
    {
        if (_secureAreaService.TryUnlock(PasswordInput))
        {
            PasswordInput = string.Empty;
            return;
        }

        _dialogService.ShowError("Forkert kodeord.", "Adgang");
        PasswordInput = string.Empty;
    }
}