using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class RackViewModel : ViewModelBase
{
    private readonly IRepository<Rack> _rackRepository;
    private readonly IRepository<Renter> _renterRepository;
    private readonly IRepository<Rental> _rentalRepository;

    private readonly ObservableCollection<RackDisplayItem> _selectedRacks = new();

    public ObservableCollection<RackDisplayItem> Racks { get; } = new();

    public ObservableCollection<RackDisplayItem> LeftColumnRacks { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_14_18 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_19_24 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_25_38 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_39_52 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_53_66 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_67_76 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_77_78 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_79_80 { get; } = new();

    public ObservableCollection<RackDisplayItem> SelectedRacks => _selectedRacks;

    // Reoler den aktuelt fundne lejer allerede lejer (aktive lejemål, EndDate = null)
    public ObservableCollection<RackDisplayItem> RenterRacks { get; } = new();

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            OnPropertyChanged();
            PerformSearch();
        }
    }

    private Renter? _foundRenter;
    public Renter? FoundRenter
    {
        get => _foundRenter;
        private set { _foundRenter = value; OnPropertyChanged(); }
    }

    public ICommand SelectRackCommand { get; }
    public ICommand CreateRentalCommand { get; }
    public ICommand TerminateRentalCommand { get; }

    public RackViewModel(IRepository<Rack> rackRepository, IRepository<Renter> renterRepository, IRepository<Rental> rentalRepository)
    {
        _rackRepository = rackRepository;
        _renterRepository = renterRepository;
        _rentalRepository = rentalRepository;

        var allItems = _rackRepository.GetAll()
            .Select(rack => new RackDisplayItem(rack))
            .ToList();

        foreach (var item in allItems)
            Racks.Add(item);

        AddRange(LeftColumnRacks, allItems.Where(i => i.Rack.Number is >= 1 and <= 13)
            .OrderByDescending(i => i.Rack.Number));

        AddRange(Cluster_14_18, InRange(allItems, 14, 18));
        AddRange(Cluster_19_24, InRange(allItems, 19, 24));
        AddRange(Cluster_25_38, InRange(allItems, 25, 38));
        AddRange(Cluster_39_52, InRange(allItems, 39, 52));
        AddRange(Cluster_53_66, InRange(allItems, 53, 66));
        AddRange(Cluster_67_76, InRange(allItems, 67, 76));
        AddRange(Cluster_77_78, InRange(allItems, 77, 78));
        AddRange(Cluster_79_80, InRange(allItems, 79, 80));

        SelectRackCommand = new RelayCommand(item =>
        {
            var clicked = (RackDisplayItem)item!;

            clicked.IsSelected = !clicked.IsSelected;

            if (clicked.IsSelected)
                _selectedRacks.Add(clicked);
            else
                _selectedRacks.Remove(clicked);
        });

        CreateRentalCommand = new RelayCommand(
            _ => CreateRental(),
            _ => FoundRenter != null && _selectedRacks.Count > 0);

        TerminateRentalCommand = new RelayCommand(
            _ => TerminateRental(),
            _ => FoundRenter != null && _selectedRacks.Count > 0);
    }

    private void PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FoundRenter = null;
            RenterRacks.Clear();
            return;
        }

        var match = _renterRepository.GetAll()
            .FirstOrDefault(r =>
                (r.Email != null && r.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                (r.Phone != null && r.Phone.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));

        FoundRenter = match;
        LoadRenterRacks();
    }

    private void LoadRenterRacks()
    {
        RenterRacks.Clear();

        if (FoundRenter == null)
            return;

        var activeRentals = _rentalRepository.GetAll()
            .Where(r => r.RenterId == FoundRenter.RenterId && r.EndDate == null);

        foreach (var rental in activeRentals)
        {
            var rackItem = Racks.FirstOrDefault(r => r.Rack.RackId == rental.RackId);
            if (rackItem != null)
                RenterRacks.Add(rackItem);
        }
    }

    private void CreateRental()
    {
        if (FoundRenter == null)
            return;

        foreach (var item in _selectedRacks.ToList())
        {
            if (item.Rack.Status != RackStatus.Available)
            {
                item.IsSelected = false;
                continue;
            }

            var rental = new Rental
            {
                RackId = item.Rack.RackId,
                RenterId = FoundRenter.RenterId,
                StartDate = DateTime.Now,
                EndDate = null
            };
            _rentalRepository.Add(rental);

            item.Rack.Status = RackStatus.Rented;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();

            item.IsSelected = false;
            RenterRacks.Add(item);
        }

        _selectedRacks.Clear();
    }

    private void TerminateRental()
    {
        if (FoundRenter == null)
            return;

        foreach (var item in _selectedRacks.ToList())
        {
            if (item.Rack.Status != RackStatus.Rented)
            {
                item.IsSelected = false;
                continue;
            }

            var rental = _rentalRepository.GetAll()
                .FirstOrDefault(r => r.RackId == item.Rack.RackId && r.RenterId == FoundRenter.RenterId && r.EndDate == null);

            if (rental == null)
            {
                item.IsSelected = false;
                continue;
            }

            rental.EndDate = DateTime.Now;
            _rentalRepository.Update(rental);

            item.Rack.Status = RackStatus.Available;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();

            item.IsSelected = false;
            RenterRacks.Remove(item);
        }

        _selectedRacks.Clear();
    }
    private static IEnumerable<RackDisplayItem> InRange(List<RackDisplayItem> items, int min, int max) =>
        items.Where(i => i.Rack.Number >= min && i.Rack.Number <= max)
             .OrderBy(i => i.Rack.Number);

    private static void AddRange(ObservableCollection<RackDisplayItem> target, IEnumerable<RackDisplayItem> source)
    {
        foreach (var item in source)
            target.Add(item);
    }
}