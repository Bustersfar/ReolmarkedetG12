using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.UI.MVVM;
using ReolmarkedetG12.UI.Services;
using System.Collections.ObjectModel;

namespace ReolmarkedetG12.UI.ViewModels;

public class SalesViewModel : ViewModelBase
{
    private readonly IRepository<Rack> _rackRepository;
    private readonly IRepository<Rental> _rentalRepository;
    private readonly IRepository<Sale> _saleRepository;
    private readonly IDialogService _dialogService;

    // --- "Tilføj vare"-felterne ---

    private int? _newRackNumber;
    public int? NewRackNumber
    {
        get => _newRackNumber;
        set => SetProperty(ref _newRackNumber, value);
    }

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
        set => SetProperty(ref _newPrice, value);
    }

    public RelayCommand AddItemCommand { get; }

    // --- "Nuværende salg"-kurven ---

    public ObservableCollection<CartLineItem> CurrentSaleItems { get; } = new();

    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        private set => SetProperty(ref _totalAmount, value);
    }

    public RelayCommand EditItemCommand { get; }
    public RelayCommand DeleteItemCommand { get; }

    // --- Betaling ---

    public IEnumerable<PaymentMethod> PaymentMethods => Enum.GetValues<PaymentMethod>();

    private PaymentMethod _paymentMethod = PaymentMethod.Cash;
    public PaymentMethod PaymentMethod
    {
        get => _paymentMethod;
        set
        {
            if (SetProperty(ref _paymentMethod, value))
                RecalculateChange();
        }
    }

    private decimal? _cashReceived;
    public decimal? CashReceived
    {
        get => _cashReceived;
        set
        {
            if (SetProperty(ref _cashReceived, value))
                RecalculateChange();
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
        IDialogService dialogService)
    {
        _rackRepository = rackRepository;
        _rentalRepository = rentalRepository;
        _saleRepository = saleRepository;
        _dialogService = dialogService;

        AddItemCommand = new RelayCommand(_ => AddItem());
        EditItemCommand = new RelayCommand(param => EditItem(param as CartLineItem));
        DeleteItemCommand = new RelayCommand(param => DeleteItem(param as CartLineItem));
        RegisterSaleCommand = new RelayCommand(_ => RegisterSale(), _ => CurrentSaleItems.Count > 0);

        CurrentSaleItems.CollectionChanged += (_, _) => RecalculateTotals();
    }

    private void AddItem()
    {
        if (NewRackNumber == null || NewPrice <= 0)
            return;

        CurrentSaleItems.Add(new CartLineItem
        {
            RackNumber = NewRackNumber.Value,
            Remark = NewRemark,
            Amount = NewPrice
        });

        NewRackNumber = null;
        NewRemark = string.Empty;
        NewPrice = 0;
    }

    private void EditItem(CartLineItem? item)
    {
        if (item == null)
            return;

        // Hent linjen tilbage op i tilføj-felterne, så den kan rettes og lægges ind igen.
        NewRackNumber = item.RackNumber;
        NewRemark = item.Remark;
        NewPrice = item.Amount;
        CurrentSaleItems.Remove(item);
    }

    private void DeleteItem(CartLineItem? item)
    {
        if (item == null)
            return;

        CurrentSaleItems.Remove(item);
    }

    private void RecalculateTotals()
    {
        TotalAmount = CurrentSaleItems.Sum(i => i.Amount);
        RecalculateChange();
    }

    private void RecalculateChange()
    {
        Change = PaymentMethod == PaymentMethod.Cash && CashReceived.HasValue
            ? CashReceived.Value - TotalAmount
            : 0m;
    }

    private void RegisterSale()
    {
        foreach (var item in CurrentSaleItems)
        {
            var rack = _rackRepository.GetAll().FirstOrDefault(r => r.Number == item.RackNumber);
            if (rack == null)
            {
                _dialogService.ShowError($"Reol {item.RackNumber} findes ikke.", "Kunne ikke registrere salg");
                return;
            }

            var activeRental = _rentalRepository.GetAll()
                .FirstOrDefault(r => r.RackId == rack.RackId && r.EndDate == null);
            if (activeRental == null)
            {
                _dialogService.ShowError($"Reol {item.RackNumber} har ingen aktiv lejer lige nu.", "Kunne ikke registrere salg");
                return;
            }

            _saleRepository.Add(new Sale
            {
                RackId = rack.RackId,
                RenterId = activeRental.RenterId,
                Date = DateTime.Now,
                Amount = item.Amount,
                Description = string.IsNullOrWhiteSpace(item.Remark) ? null : item.Remark
            });
        }

        CurrentSaleItems.Clear();
        CashReceived = null;
    }
}