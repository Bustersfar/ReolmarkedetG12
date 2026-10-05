using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    private readonly IItemRepository _itemRepository;
    private readonly IRepository<SaleLine> _saleLineRepository;
    private readonly IRepository<Sale> _saleRepository;
    private readonly IDialogService _dialogService;

    public ObservableCollection<Item> Items { get; }

    public ObservableCollection<CartLineItem> CartItems { get;  } = new();

    private Item? _selectedItem;
    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                OnPropertyChanged(nameof(SelectedItem));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    private int? _itemNumber;
    public int? ItemNumber
    {
        get => _itemNumber;
        set
        {
            if (SetProperty(ref _itemNumber, value))
            {
                LookupItem();
            }
        }
    }

    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetProperty(ref _totalAmount, value);
    }

    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;
    public PaymentMethod SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set
        {
            if (SetProperty(ref _selectedPaymentMethod, value))
            {
                OnPropertyChanged(nameof(IsCashPayment));
                CommandManager.InvalidateRequerySuggested();
                RecalculateChange();
            }
        }
    }

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

    private decimal? _change;
    public decimal? Change
    {
        get => _change;
        set => SetProperty(ref _change, value);
    }

    private string _itemStatusMessage = string.Empty;
    public string ItemStatusMessage 
    { 
        get => _itemStatusMessage;
        set => SetProperty(ref _itemStatusMessage, value);
    }

    public RelayCommand AddItemCommand { get; }
    public RelayCommand RemoveItemCommand { get; }
    public RelayCommand RegisterSaleCommand { get; }
    public RelayCommand ClearCartCommand { get; }

    public SalesViewModel(IItemRepository itemRepository, IRepository<SaleLine> saleLineRepository, IRepository<Sale> saleRepository, IDialogService dialogService)
    {
        _itemRepository = itemRepository;
        _saleLineRepository = saleLineRepository;
        _saleRepository = saleRepository;
        _dialogService = dialogService;

        Items = new ObservableCollection<Item>(_itemRepository.GetAll());

        AddItemCommand = new RelayCommand(_=> AddItem(), _=> SelectedItem != null);
        ClearCartCommand = new RelayCommand(_=> CartItems.Clear(), _=> CartItems.Any());

        RemoveItemCommand = new RelayCommand(item => RemoveItem(item as CartLineItem));

        RegisterSaleCommand = new RelayCommand(_=>RegisterSale(), _=> CanRegisterSale());

        CartItems.CollectionChanged += (_, _) => 
        {
            RecalculateTotalAmount();
            CommandManager.InvalidateRequerySuggested();
        };
    }

    public IEnumerable<PaymentMethod> PaymentMethods => Enum.GetValues<PaymentMethod>();

    public bool IsCashPayment => SelectedPaymentMethod == PaymentMethod.Cash;

    private void AddItem()
    {
        if (SelectedItem == null)
            return;

        CartItems.Add(new CartLineItem
        {
            ItemId = SelectedItem.ItemId,
            ItemNumber = SelectedItem.ItemNumber,
            ItemName = SelectedItem.Name,
            RackId = SelectedItem.RackId,
            Price = SelectedItem.Price
        });
        
        SelectedItem = null;
        ItemNumber = null;
    }

    private void RemoveItem(CartLineItem? item)
    {
        if (item != null)
        {
            CartItems.Remove(item);
        }
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = CartItems.Sum(item => item.Price);
        RecalculateChange();
    }

    private void RecalculateChange()
    {
        if (SelectedPaymentMethod == PaymentMethod.Cash && CashReceived.HasValue)
        {
            Change = CashReceived.Value - TotalAmount;
        }
        else
        {
            Change = 0;
        }
    }

    private void RegisterSale()
    {
        try
        {
            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                TotalAmount = TotalAmount,
                PaymentMethod = SelectedPaymentMethod
            };

            _saleRepository.Add(sale);
            foreach (var item in CartItems)
            {
                _saleLineRepository.Add(new SaleLine
                {
                    SaleId = sale.SaleId,
                    ItemId = item.ItemId,
                    SalePrice = item.Price
                });
            }

            _dialogService.ShowInfo($"Salg registreret. \nTotalbeløb: {TotalAmount:C}", "Salg registreret");

            CartItems.Clear();

            CashReceived = null;
            Change = 0;
            TotalAmount = 0;
            SelectedItem = null;
            ItemNumber = null;
            SelectedPaymentMethod = PaymentMethod.Cash;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, "Fejl ved registrering af salg");
        }
    }


    private bool CanRegisterSale()
    {
        if (!CartItems.Any())
        return false;

        if (SelectedPaymentMethod == PaymentMethod.Cash)
        {
            return CashReceived.HasValue && CashReceived.Value >= TotalAmount;
        }
        return true;
    }

    private void LookupItem()
    {
        if (ItemNumber.HasValue)
        {
            SelectedItem = _itemRepository.GetByItemNumber(ItemNumber.Value);

            if (SelectedItem == null)
            {
                ItemStatusMessage = "Varen blev ikke fundet";
            }
            else
            {
                ItemStatusMessage = string.Empty;
            }
        }
        else
        {
            SelectedItem = null;
            ItemStatusMessage = string.Empty;
        }
    }

}




   