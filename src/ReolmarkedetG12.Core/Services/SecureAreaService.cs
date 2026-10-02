using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReolmarkedetG12.UI.Services;

public class SecureAreaService : ISecureAreaService
{
    private readonly string _password;
    private bool _isUnlocked;

    public SecureAreaService(string password)
    {
        _password = password;
    }

    public bool IsUnlocked
    {
        get => _isUnlocked;
        private set
        {
            if (_isUnlocked == value)
                return;

            _isUnlocked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUnlocked)));
        }
    }

    public bool TryUnlock(string password)
    {
        if (password != _password)
            return false;

        IsUnlocked = true;
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}