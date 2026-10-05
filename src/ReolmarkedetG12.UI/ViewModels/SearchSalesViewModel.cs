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

public class SearchSalesViewModel : ViewModelBase
{
    private readonly IRepository<Sale> _saleRepository;
    private readonly IDialogService _dialogService;
    private readonly ISecureAreaService? _secureAreaService;


    private DateTime? _searchDate;
    public DateTime? SearchDate
    {
        get => _searchDate;
        set => SetProperty(ref _searchDate, value);
    }

    private SaleDisplayItem? _selectedSaleItem;
    public SaleDisplayItem? SelectedSaleItem
    {
        get => _selectedSaleItem;
        set
        {
            if (SetProperty(ref _selectedSaleItem, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ObservableCollection<SaleDisplayItem> SaleResults { get; } = new();

    // Kodeords-lås (samme SecureAreaService som Månedsopgørelse, se ISecureAreaService).
    // Null, hvis ViewModel'en er oprettet uden en ISecureAreaService (bruges af nogle tests),
    // så vinduet i det tilfælde simpelthen forbliver bag låsen.
    public LockScreenViewModel? Lock { get; }

    public RelayCommand SearchCommand { get; }
    public RelayCommand ResetSearchCommand { get; }
    public RelayCommand DeleteCommand { get; }

    // Konstruktør med 4 parametre (til enhedstests)
    public SearchSalesViewModel(
        IRepository<Sale> saleRepository,
        IDialogService dialogService)
        : this(saleRepository, dialogService, null)
    {
    }

    // Konstruktør med 5 parametre (anvendes af App.xaml.cs)
    public SearchSalesViewModel(
        IRepository<Sale> saleRepository,
        IDialogService dialogService,
        ISecureAreaService? secureAreaService)
        : base(dialogService)
    {
        _saleRepository = saleRepository;
        _dialogService = dialogService;
        _secureAreaService = secureAreaService;

        Lock = secureAreaService != null
            ? new LockScreenViewModel(secureAreaService, dialogService)
            : null;

        SearchCommand = new RelayCommand(_ => SafeExecute(Search));
        ResetSearchCommand = new RelayCommand(_ => ResetSearch());
        DeleteCommand = new RelayCommand(param => SafeExecute(() => Delete(param as SaleDisplayItem)));
    }

    private void Search()
    {
        SaleResults.Clear();

        var sales = _saleRepository.GetAll();

        if (SearchDate.HasValue)
        {
            sales = sales.Where(s => s.SaleDate.Date == SearchDate.Value.Date);
        }

        foreach (var sale in sales.OrderByDescending(s => s.SaleDate))
        {
                SaleResults.Add(new SaleDisplayItem
                {
                    SaleId = sale.SaleId,
                    SaleDate = sale.SaleDate,
                    TotalAmount = sale.TotalAmount,
                    PaymentMethod = sale.PaymentMethod,
                });
        }

        if (SaleResults.Count == 0)
        {
            _dialogService.ShowInfo("Ingen salg matchede søgekriterierne.", "Søgeresultat");
        }
    }

    private void ResetSearch()
    {
        SearchDate = null;
        SaleResults.Clear();
        SelectedSaleItem = null;
    }

    private void Delete(SaleDisplayItem? item)
    {
        var target = item ?? SelectedSaleItem;

        if (target == null) 
            return;

        bool confirm = _dialogService.Confirm(
            $"Er du sikker på, at du vil slette salget med ID {target.SaleId}?",
            "Bekræft sletning");

        if (!confirm) return;

        _saleRepository.Delete(target.SaleId);
        SaleResults.Remove(target);
    }

    // SafeExecute ligger nu i ViewModelBase og deles af alle ViewModels.
}