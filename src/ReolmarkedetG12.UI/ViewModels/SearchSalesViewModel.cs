using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.UI.MVVM;
using ReolmarkedetG12.UI.Services;
using System.Collections.ObjectModel;

namespace ReolmarkedetG12.UI.ViewModels;

public class SearchSalesViewModel : ViewModelBase
{
    private readonly IRepository<Rack> _rackRepository;
    private readonly IRepository<Sale> _saleRepository;

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

    public ObservableCollection<SaleDisplayItem> SaleResults { get; } = new();

    public RelayCommand SearchCommand { get; }
    public RelayCommand UpdateCommand { get; }

    public SearchSalesViewModel(
        IRepository<Rack> rackRepository,
        IRepository<Sale> saleRepository,
        IDialogService dialogService,
        ISecureAreaService secureAreaService)
    {
        _rackRepository = rackRepository;
        _saleRepository = saleRepository;

        Lock = new LockScreenViewModel(secureAreaService, dialogService);

        SearchCommand = new RelayCommand(_ => Search());
        UpdateCommand = new RelayCommand(_ => Update());
    }

    private void Search()
    {
        SaleResults.Clear();

        var racks = _rackRepository.GetAll().ToList();

        foreach (var sale in _saleRepository.GetAll())
        {
            var rack = racks.FirstOrDefault(r => r.RackId == sale.RackId);
            if (rack == null)
                continue;

            if (SearchRackNumber.HasValue && rack.Number != SearchRackNumber.Value)
                continue;

            if (SearchDate.HasValue && sale.Date.Date != SearchDate.Value.Date)
                continue;

            SaleResults.Add(new SaleDisplayItem
            {
                SaleId = sale.SaleId,
                RackNumber = rack.Number,
                Date = sale.Date,
                Amount = sale.Amount,
                Description = sale.Description ?? string.Empty
            });
        }
    }

    private void Update()
    {
        foreach (var item in SaleResults)
        {
            var sale = _saleRepository.GetById(item.SaleId);
            if (sale == null)
                continue;

            sale.Amount = item.Amount;
            sale.Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description;
            _saleRepository.Update(sale);
        }
    }
}