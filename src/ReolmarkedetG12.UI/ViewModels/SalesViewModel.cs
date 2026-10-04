using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.UI.MVVM;
using ReolmarkedetG12.UI.Services;

namespace ReolmarkedetG12.UI.ViewModels;

public class SalesViewModel : ViewModelBase
{
    private readonly IRepository<Rack> _rackRepository;
    private readonly IRepository<Rental> _rentalRepository;
    private readonly IRepository<Sale> _saleRepository;
    private readonly IRepository<Renter> _renterRepository;
    private readonly IDialogService _dialogService;

    // --- 1. Indtast vare ---

    private int? _newRackNumber;
    public int? NewRackNumber
    {
        get => _newRackNumber;
        set
        {
            if (SetProperty(ref _newRackNumber, value))
            {
                ValidateEnteredRack();
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    private string _rackValidationMessage = string.Empty;
    public string RackValidationMessage
    {
        get => _rackValidationMessage;
        private set => SetProperty(ref _rackValidationMessage, value);
    }

    private bool _isRackValid;
    public bool IsRackValid
    {
        get => _isRackValid;
        private set => SetProperty(ref _isRackValid, value);
    }

    private int? _activeRenterId;

    private string _newRemark = string.Empty;
    public string NewRemark
    {
        get => _newRemark;
        set => SetProperty(ref _newRemark, value);
    }

    private decimal _newPrice;
    public decimal NewPrice
    {
        get => _newPrice;
        set
        {
            if (SetProperty(ref _newPrice, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public RelayCommand AddItemCommand { get; }

    // --- 2. Aktuelt salg (kurv) ---

    public ObservableCollection<CartLineItem> CurrentSaleItems { get; } = new();

    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        private set => SetProperty(ref _totalAmount, value);
    }

    public RelayCommand EditItemCommand { get; }
    public RelayCommand DeleteItemCommand { get; }
    public RelayCommand ClearCartCommand { get; }

    // --- 3. Betaling & afslutning ---

    public IEnumerable<PaymentMethod> PaymentMethods => Enum.GetValues<PaymentMethod>();

    private PaymentMethod _paymentMethod = PaymentMethod.Cash;
    public PaymentMethod PaymentMethod
    {
        get => _paymentMethod;
        set
        {
            if (SetProperty(ref _paymentMethod, value))
            {
                OnPropertyChanged(nameof(IsCashPayment));
                RecalculateChange();
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsCashPayment => PaymentMethod == PaymentMethod.Cash;

    private decimal? _cashReceived;
    public decimal? CashReceived
    {
        get => _cashReceived;
        set
        {
            if (SetProperty(ref _cashReceived, value))
            {
                RecalculateChange();
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    private decimal _change;
    public decimal Change
    {
        get => _change;
        private set => SetProperty(ref _change, value);
    }

    public RelayCommand RegisterSaleCommand { get; }

    public SalesViewModel(
        IRepository<Rack> rackRepository,
        IRepository<Rental> rentalRepository,
        IRepository<Sale> saleRepository,
        IRepository<Renter> renterRepository,
        IDialogService dialogService)
    {
        _rackRepository = rackRepository;
        _rentalRepository = rentalRepository;
        _saleRepository = saleRepository;
        _renterRepository = renterRepository;
        _dialogService = dialogService;

        AddItemCommand = new RelayCommand(
            _ => AddItem(),
            _ => IsRackValid && NewPrice > 0 && NewRackNumber.HasValue);

        EditItemCommand = new RelayCommand(param => EditItem(param as CartLineItem));
        DeleteItemCommand = new RelayCommand(param => DeleteItem(param as CartLineItem));
        ClearCartCommand = new RelayCommand(_ => ClearCart(), _ => CurrentSaleItems.Count > 0);

        RegisterSaleCommand = new RelayCommand(
            _ => SafeExecute(RegisterSale),
            _ => CanRegisterSale());

        CurrentSaleItems.CollectionChanged += (_, _) => RecalculateTotals();
    }

    private void ValidateEnteredRack()
    {
        _activeRenterId = null;

        if (!NewRackNumber.HasValue)
        {
            RackValidationMessage = string.Empty;
            IsRackValid = false;
            return;
        }

        // Reol 0 er butikkens eget salg (poser, mærker mv.)
        if (NewRackNumber.Value == 0)
        {
            RackValidationMessage = "Butikken (poser/mærker)";
            IsRackValid = true;
            _activeRenterId = null;
            return;
        }

        SafeExecute(() =>
        {
            var rack = _rackRepository.GetAll().FirstOrDefault(r => r.Number == NewRackNumber.Value);
            if (rack == null)
            {
                RackValidationMessage = $"Reol {NewRackNumber.Value} findes ikke.";
                IsRackValid = false;
                return;
            }

            var now = DateTime.Now;
            var activeRental = _rentalRepository.GetAll()
                .Where(r => r.RackId == rack.RackId && r.StartDate <= now && (r.EndDate == null || r.EndDate > now))
                .OrderByDescending(r => r.StartDate)
                .FirstOrDefault();

            if (activeRental == null)
            {
                RackValidationMessage = $"Reol {NewRackNumber.Value} har ingen aktiv lejer!";
                IsRackValid = false;
                return;
            }

            var renter = _renterRepository.GetById(activeRental.RenterId);
            if (renter != null)
            {
                RackValidationMessage = $"Lejer: {renter.FirstName} {renter.LastName}";
                _activeRenterId = renter.RenterId;
                IsRackValid = true;
            }
            else
            {
                RackValidationMessage = "Ukendt lejer tilknyttet reolen.";
                IsRackValid = false;
            }
        });
    }

    private void AddItem()
    {
        if (!IsRackValid || !NewRackNumber.HasValue || NewPrice <= 0)
            return;

        CurrentSaleItems.Add(new CartLineItem
        {
            RackNumber = NewRackNumber.Value,
            Remark = string.IsNullOrWhiteSpace(NewRemark) ? string.Empty : NewRemark.Trim(),
            Amount = NewPrice
        });

        NewRackNumber = null;
        NewRemark = string.Empty;
        NewPrice = 0;
        RackValidationMessage = string.Empty;
        IsRackValid = false;
        _activeRenterId = null;
    }

    private void EditItem(CartLineItem? item)
    {
        if (item == null)
            return;

        NewRackNumber = item.RackNumber;
        NewRemark = item.Remark;
        NewPrice = item.Amount;
        CurrentSaleItems.Remove(item);
    }

    private void DeleteItem(CartLineItem? item)
    {
        if (item != null)
            CurrentSaleItems.Remove(item);
    }

    private void ClearCart()
    {
        if (_dialogService.Confirm("Vil du annullere det igangværende salg og tømme kurven?", "Annuller salg"))
        {
            CurrentSaleItems.Clear();
            CashReceived = null;
        }
    }

    private void RecalculateTotals()
    {
        TotalAmount = CurrentSaleItems.Sum(i => i.Amount);
        RecalculateChange();
        CommandManager.InvalidateRequerySuggested();
    }

    private void RecalculateChange()
    {
        if (PaymentMethod == PaymentMethod.Cash && CashReceived.HasValue)
        {
            Change = CashReceived.Value - TotalAmount;
        }
        else
        {
            Change = 0m;
        }
    }

    private bool CanRegisterSale()
    {
        if (CurrentSaleItems.Count == 0)
            return false;

        if (PaymentMethod == PaymentMethod.Cash)
        {
            return CashReceived.HasValue && CashReceived.Value >= TotalAmount;
        }

        return true;
    }

    private void RegisterSale()
    {
        var now = DateTime.Now;
        var salesToInsert = new List<Sale>();

        foreach (var item in CurrentSaleItems)
        {
            int? rackId = null;
            int? renterId = null;

            if (item.RackNumber == 0)
            {
                // Find reol 0 i databasen hvis oprettet, ellers forbliver den null
                var internalRack = _rackRepository.GetAll().FirstOrDefault(r => r.Number == 0);
                rackId = internalRack?.RackId;
                renterId = null;
            }
            else
            {
                var rack = _rackRepository.GetAll().FirstOrDefault(r => r.Number == item.RackNumber);
                if (rack == null)
                    throw new InvalidOperationException($"Reol {item.RackNumber} blev ikke fundet.");

                var activeRental = _rentalRepository.GetAll()
                    .Where(r => r.RackId == rack.RackId && r.StartDate <= now && (r.EndDate == null || r.EndDate > now))
                    .OrderByDescending(r => r.StartDate)
                    .FirstOrDefault();

                if (activeRental == null)
                    throw new InvalidOperationException($"Reol {item.RackNumber} har ikke længere et aktivt lejemål.");

                rackId = rack.RackId;
                renterId = activeRental.RenterId;
            }

            salesToInsert.Add(new Sale
            {
                RackId = rackId,
                RenterId = renterId,
                Date = now,
                Amount = item.Amount,
                Description = string.IsNullOrWhiteSpace(item.Remark) ? string.Empty : item.Remark.Trim(),
                PaymentMethod = PaymentMethod
            });
        }

        // Transaktionsstyret indsættelse af hele kurven
        if (_saleRepository is SaleRepository concreteRepo)
        {
            concreteRepo.AddMany(salesToInsert);
        }
        else
        {
            foreach (var sale in salesToInsert)
            {
                _saleRepository.Add(sale);
            }
        }

        _dialogService.ShowInfo($"Salget på {TotalAmount:0.00} kr. er gennemført!", "Salg afsluttet");

        CurrentSaleItems.Clear();
        CashReceived = null;
        Change = 0;
    }

    private bool SafeExecute(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (DatabaseConnectionException ex)
        {
            _dialogService.ShowError(
                $"{ex.Message}\n\nTeknisk besked: {ex.InnerException?.Message ?? "ukendt"}",
                "Forbindelsesfejl");
            return false;
        }
        catch (SqlException ex)
        {
            _dialogService.ShowError(
                $"Der opstod en fejl i databasen.\n\nTeknisk besked: {ex.Message}",
                "Databasefejl");
            return false;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, "Fejl");
            return false;
        }
    }
}