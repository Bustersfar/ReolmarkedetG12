using System.Collections.ObjectModel;
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
    private readonly IRepository<Rack> _rackRepository;
    private readonly IRepository<Sale> _saleRepository;
    private readonly IRepository<Renter> _renterRepository;
    private readonly IDialogService _dialogService;

    public LockScreenViewModel Lock { get; }

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

    public RelayCommand SearchCommand { get; }
    public RelayCommand ResetSearchCommand { get; }
    public RelayCommand UpdateCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public SearchSalesViewModel(
        IRepository<Rack> rackRepository,
        IRepository<Sale> saleRepository,
        IRepository<Renter> renterRepository,
        IDialogService dialogService,
        ISecureAreaService secureAreaService)
    {
        _rackRepository = rackRepository;
        _saleRepository = saleRepository;
        _renterRepository = renterRepository;
        _dialogService = dialogService;

        Lock = new LockScreenViewModel(secureAreaService, dialogService);

        SearchCommand = new RelayCommand(_ => SafeExecute(Search));
        ResetSearchCommand = new RelayCommand(_ => ResetSearch());
        UpdateCommand = new RelayCommand(_ => SafeExecute(Update), _ => SaleResults.Count > 0);
        DeleteCommand = new RelayCommand(param => SafeExecute(() => Delete(param as SaleDisplayItem)));
    }

    private void Search()
    {
        SaleResults.Clear();

        var racks = _rackRepository.GetAll().ToDictionary(r => r.RackId, r => r.Number);
        var renters = _renterRepository.GetAll().ToDictionary(r => r.RenterId, r => $"{r.FirstName} {r.LastName}");

        var sales = _saleRepository.GetAll();

        if (SearchRackNumber.HasValue)
        {
            var matchingRackIds = racks.Where(r => r.Value == SearchRackNumber.Value).Select(r => r.Key).ToHashSet();
            sales = sales.Where(s => matchingRackIds.Contains(s.RackId));
        }

        if (SearchDate.HasValue)
        {
            sales = sales.Where(s => s.Date.Date == SearchDate.Value.Date);
        }

        var orderedSales = sales.OrderByDescending(s => s.Date);

        foreach (var sale in orderedSales)
        {
            racks.TryGetValue(sale.RackId, out int rackNumber);
            renters.TryGetValue(sale.RenterId, out string? renterName);

            SaleResults.Add(new SaleDisplayItem
            {
                SaleId = sale.SaleId,
                RackNumber = rackNumber,
                RenterName = renterName ?? "Ukendt lejer",
                Date = sale.Date,
                Amount = sale.Amount,
                Description = sale.Description ?? string.Empty
            });
        }

        if (SaleResults.Count == 0)
        {
            _dialogService.ShowInfo("Ingen salg matchede søgekriterierne.", "Søgeresultat");
        }
    }

    private void ResetSearch()
    {
        SearchRackNumber = null;
        SearchDate = null;
        SaleResults.Clear();
        SelectedSaleItem = null;
    }

    private void Update()
    {
        int updatedCount = 0;

        foreach (var item in SaleResults)
        {
            if (item.Amount <= 0)
            {
                _dialogService.ShowError($"Beløb for salg ID {item.SaleId} skal være større end 0 kr.", "Ugyldigt beløb");
                return;
            }

            var sale = _saleRepository.GetById(item.SaleId);
            if (sale == null)
                continue;

            sale.Amount = item.Amount;
            sale.Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim();
            _saleRepository.Update(sale);
            updatedCount++;
        }

        _dialogService.ShowInfo($"{updatedCount} salg er opdateret i databasen.", "Gemt");
    }

    private void Delete(SaleDisplayItem? item)
    {
        var target = item ?? SelectedSaleItem;
        if (target == null)
            return;

        bool confirm = _dialogService.Confirm(
            $"Er du sikker på, at du vil slette salget på {target.Amount:0.00} kr. fra Reol {target.RackNumber} ({target.Date:dd-MM-yyyy})?",
            "Bekræft sletning af salg");

        if (!confirm)
            return;

        _saleRepository.Delete(target.SaleId);
        SaleResults.Remove(target);

        _dialogService.ShowInfo("Salget er blevet slettet.", "Slettet");
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