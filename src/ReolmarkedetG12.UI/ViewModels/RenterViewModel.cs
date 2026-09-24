using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.UI.MVVM;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.UI.Services;


namespace ReolmarkedetG12.UI.ViewModels;

public class RenterViewModel : ViewModelBase
{
    private readonly IRepository<Renter> _renterRepository;
    private readonly IDialogService _dialogService;

    public ObservableCollection<Renter> Renters { get; }

    private Renter? _selectedRenter;
    public Renter? SelectedRenter
    {
        get => _selectedRenter;
        set
        {
            if (SetProperty(ref _selectedRenter, value))
            {
                if (value != null)
                {
                    RenterId = value.RenterId;
                    FirstName = value.FirstName;
                    LastName = value.LastName;
                    Address = value.Address;
                    PostalCode = value.PostalCode;
                    City = value.City;
                    Phone = value.Phone ?? string.Empty;
                    Email = value.Email ?? string.Empty;
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    private int _renterId;
    public int RenterId
    {
        get => _renterId;
        set => SetProperty(ref _renterId, value);
    }

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }
    private string _city = string.Empty;
    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }
    private string _phone = string.Empty;
    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }
    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }
    private int _postalCode = 0;
    public int PostalCode
    {
        get => _postalCode;
        set => SetProperty(ref _postalCode, value);
    }
    private string _address = string.Empty;
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }


    public RenterViewModel(IRepository<Renter> renterRepository, IDialogService dialogService)
    {
        _renterRepository = renterRepository;
        _dialogService = dialogService;

        Renters = new ObservableCollection<Renter>();

        NewCommand = new RelayCommand(_ => NewRenter());
        SaveCommand = new RelayCommand(_ => SafeExecute(SaveRenter));
        DeleteCommand = new RelayCommand(_ => SafeExecute(DeleteRenter), _ => SelectedRenter != null && SelectedRenter.RenterId > 0);

        SafeExecute(LoadRenters);
        NewRenter();
    }

    private void SafeExecute(Action action)
    {
        try
        {
            action();
        }
        catch (DatabaseConnectionException ex)
        {
            _dialogService.ShowError(
                $"{ex.Message}\n\nTeknisk besked: {ex.InnerException?.Message ?? "ukendt"}",
                "Forbindelsesfejl");
        }
    }

    private void NewRenter()
    {
        SelectedRenter = null;

        RenterId = 0;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        PostalCode = 0;
        Address = string.Empty;
        City = string.Empty;
        Phone = string.Empty;
    }

    private void LoadRenters()
    {
        Renters.Clear();
        foreach (var renter in _renterRepository.GetAll())
        {
            Renters.Add(renter);
        }
    }

    private void SaveRenter()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Address) || PostalCode <= 0 || string.IsNullOrWhiteSpace(City))
        {
            MessageBox.Show("Fornavn, Efternavn, Adresse, Postnummer og By skal udfyldes.", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        var renter = new Renter
        {
            RenterId = RenterId,
            FirstName = FirstName,
            LastName = LastName,
            Address = Address,
            PostalCode = PostalCode,
            City = City,
            Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone,
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email
        };

        if (renter.RenterId == 0)
        {
            // New renter
            _renterRepository.Add(renter);
        }
        else
        {
            // Existing renter
            _renterRepository.Update(renter);
        }

        LoadRenters();
        NewRenter(); // Clear fields after saving
    }

    private void DeleteRenter()
    {
        if (SelectedRenter == null)
            return;

        var result = MessageBox.Show($"Er du sikker på, at du vil slette lejer: {SelectedRenter.FirstName} {SelectedRenter.LastName}?", "Bekræft sletning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _renterRepository.Delete(SelectedRenter.RenterId);
            LoadRenters();
            NewRenter();
        }
    }
}