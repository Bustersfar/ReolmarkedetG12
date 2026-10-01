using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.UI.MVVM;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace ReolmarkedetG12.UI.ViewModels;

// PROTOTYPE: ingen Sale-model/repository endnu. RegisterSale/Search/Update
// arbejder kun på lister i hukommelsen, indtil vi bygger den rigtige gemning.
public class SalesViewModel : ViewModelBase
{
    private readonly List<SaleDisplayItem> _allSales = new();

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

    // --- Søg/ret-panel (popup) ---

    private bool _showSearchPanel;
    public bool ShowSearchPanel
    {
        get => _showSearchPanel;
        set => SetProperty(ref _showSearchPanel, value);
    }

    public RelayCommand ToggleSearchPanelCommand { get; }

    private int? _searchRackNumber;
    public int? SearchRackNumber
    {
        get => _searchRackNumber;
        set => SetProperty(ref _searchRackNumber, value);
    }

    private DateTime? _searchDate;
    public DateTime? SearchDate
    {
        get => _searchDate;
        set => SetProperty(ref _searchDate, value);
    }

    public ObservableCollection<SaleDisplayItem> SaleResults { get; } = new();

    public RelayCommand SearchCommand { get; }
    public RelayCommand UpdateCommand { get; }

    public SalesViewModel()
    {
        AddItemCommand = new RelayCommand(_ => AddItem());
        EditItemCommand = new RelayCommand(param => EditItem(param as CartLineItem));
        DeleteItemCommand = new RelayCommand(param => DeleteItem(param as CartLineItem));
        RegisterSaleCommand = new RelayCommand(_ => RegisterSale(), _ => CurrentSaleItems.Count > 0);
        ToggleSearchPanelCommand = new RelayCommand(_ => ShowSearchPanel = !ShowSearchPanel);
        SearchCommand = new RelayCommand(_ => Search());
        UpdateCommand = new RelayCommand(_ => Update());

        CurrentSaleItems.CollectionChanged += (_, _) => RecalculateTotals();

        // Dummy-data til søge/ret-panelet, bare så gridet ikke er tomt.
        _allSales.Add(new SaleDisplayItem { RackNumber = 5, Date = new DateTime(2026, 9, 15), Amount = 150m, Description = "2x kaffekrus" });
        _allSales.Add(new SaleDisplayItem { RackNumber = 12, Date = new DateTime(2026, 9, 20), Amount = 60m });
        foreach (var sale in _allSales)
            SaleResults.Add(sale);
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
        // TODO: opret et rigtigt Sale-objekt pr. linje (RenterId slås op som
        // reolens aktive lejer lige nu) og gem via ISaleRepository.
        foreach (var item in CurrentSaleItems)
        {
            _allSales.Add(new SaleDisplayItem
            {
                RackNumber = item.RackNumber,
                Date = DateTime.Now,
                Amount = item.Amount,
                Description = item.Remark
            });
        }

        CurrentSaleItems.Clear();
        CashReceived = null;
    }

    private void Search()
    {
        // TODO: erstat med et rigtigt opslag i ISaleRepository.
        SaleResults.Clear();
        foreach (var sale in _allSales.Where(s =>
                     (SearchRackNumber == null || s.RackNumber == SearchRackNumber) &&
                     (SearchDate == null || s.Date.Date == SearchDate.Value.Date)))
        {
            SaleResults.Add(sale);
        }
    }

    private void Update()
    {
        // TODO: gem de rettede værdier i SaleResults via ISaleRepository.Update(...).
    }
}