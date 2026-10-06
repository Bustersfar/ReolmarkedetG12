using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.UI.MVVM;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.UI.Services;
using Microsoft.Data.SqlClient;

namespace ReolmarkedetG12.UI.ViewModels;

public class RenterViewModel : ViewModelBase
{
    private readonly IRepository<Renter> _renterRepository;
    private readonly IRepository<Rental> _rentalRepository;
    private readonly IDialogService _dialogService;
    // Valgfri: bruges kun til at tjekke om en lejer har betalingshistorik, før den slettes.
    // Nullable, så eksisterende tests der opretter RenterViewModel uden den stadig virker.
    private readonly IRepository<Payment>? _paymentRepository;

    public ObservableCollection<Renter> Renters { get; }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                FilterRenters();
            }
        }
    }

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

    public RelayCommand NewCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand GetAllCommand { get; }

    public RenterViewModel(
        IRepository<Renter> renterRepository,
        IRepository<Rental> rentalRepository,
        IDialogService dialogService,
        IRepository<Payment>? paymentRepository = null)
        : base(dialogService)
    {
        _renterRepository = renterRepository;
        _rentalRepository = rentalRepository;
        _dialogService = dialogService;
        _paymentRepository = paymentRepository;

        Renters = new ObservableCollection<Renter>();

        NewCommand = new RelayCommand(_ => NewRenter());
        SaveCommand = new RelayCommand(_ => SafeExecute(SaveRenter));
        DeleteCommand = new RelayCommand(_ => SafeExecute(DeleteRenter), _ => SelectedRenter != null && SelectedRenter.RenterId > 0);
        GetAllCommand = new RelayCommand(_ => SafeExecute(LoadRenters));

        SafeExecute(LoadRenters);
        NewRenter();
    }

    // SafeExecute ligger nu i ViewModelBase og deles af alle ViewModels.

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
            _dialogService.ShowError("Fornavn, Efternavn, Adresse, Postnummer og By skal udfyldes.", "Fejl");
            return;
        }

        // Danske postnumre er altid 4 cifre (1000-9999).
        if (PostalCode < 1000 || PostalCode > 9999)
        {
            _dialogService.ShowError("Postnummer skal være 4 cifre (f.eks. 4200).", "Fejl");
            return;
        }

        // E-mail er valgfri, men hvis den er udfyldt, skal den se ud som en e-mail.
        if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            _dialogService.ShowError("E-mailadressen ser ikke korrekt ud (f.eks. navn@eksempel.dk).", "Fejl");
            return;
        }

        // Telefon er valgfri, men hvis den er udfyldt, skal det være 8 cifre (dansk mobil/fastnet).
        if (!string.IsNullOrWhiteSpace(Phone) && !Regex.IsMatch(Phone, @"^\d{8}$"))
        {
            _dialogService.ShowError("Telefonnummer skal være 8 cifre (f.eks. 12345678).", "Fejl");
            return;
        }

        // Tjek om lejeren allerede findes (samme navn og adresse), så vi ikke får dubletter.
        var findesAllerede = _renterRepository.GetAll().Any(r =>
            r.RenterId != RenterId &&
            string.Equals(r.FirstName, FirstName, System.StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.LastName, LastName, System.StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.Address, Address, System.StringComparison.OrdinalIgnoreCase));

        if (findesAllerede)
        {
            _dialogService.ShowError($"{FirstName} {LastName} findes allerede på adressen {Address}.", "Lejer findes allerede");
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

        var harLejemaal = _rentalRepository.GetAll().Any(r => r.RenterId == SelectedRenter.RenterId);
        var harBetalinger = _paymentRepository?.GetAll().Any(p => p.RenterId == SelectedRenter.RenterId) ?? false;

        if (harLejemaal || harBetalinger)
        {
            _dialogService.ShowError(
                $"{SelectedRenter.FirstName} {SelectedRenter.LastName} har (eller har haft) lejemål eller betalinger registreret og kan derfor ikke slettes.",
                "Kan ikke slette lejer");
            return;
        }

        if (_dialogService.Confirm($"Er du sikker på, at du vil slette lejer: {SelectedRenter.FirstName} {SelectedRenter.LastName}?", "Bekræft sletning"))
        {
            _renterRepository.Delete(SelectedRenter.RenterId);
            LoadRenters();
            NewRenter();
        }
    }

    private void FilterRenters()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            LoadRenters();
            return;
        }

        var search = SearchQuery.ToLower();

        var results = _renterRepository.GetAll().Where(r =>
            r.FirstName.ToLower().Contains(search) ||
            r.LastName.ToLower().Contains(search) ||
            (r.Phone != null && r.Phone.ToLower().Contains(search)) ||
            (r.Email != null && r.Email.ToLower().Contains(search)));

        Renters.Clear();
        foreach (var renter in results)
        {
            Renters.Add(renter);
        }
    }
}