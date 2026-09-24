using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.Core.Services;
using ReolmarkedetG12.UI.MVVM;
using ReolmarkedetG12.UI.Services;
using Microsoft.Data.SqlClient;

namespace ReolmarkedetG12.UI.ViewModels;

public class RackViewModel : ViewModelBase
{
    private readonly IRepository<Rack> _rackRepository;
    private readonly IRepository<Renter> _renterRepository;
    private readonly IRepository<Rental> _rentalRepository;
    private readonly RentalPriceTierRepository _rentalPriceTierRepository;
    private readonly IDialogService _dialogService;
    private readonly DispatcherTimer _terminationCheckTimer;

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

    public ObservableCollection<RenterRackDisplayItem> RenterRacks { get; } = new();

    // Samlet månedsbeløb for lejerens aktive/opsagte reoler — udledes af RenterRacks,
    // ikke gemt separat. Raises OnPropertyChanged manuelt, hver gang RenterRacks ændres.
    public decimal TotalMonthlyRent => RenterRacks.Sum(r => r.Rental.MonthlyRent);

    public ObservableCollection<Renter> SearchResults { get; } = new();

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
    public ICommand SelectRenterCommand { get; }
    public ICommand CreateRentalCommand { get; }
    public ICommand TerminateRentalCommand { get; }
    public ICommand CancelTerminationCommand { get; }

    public RackViewModel(IRepository<Rack> rackRepository, IRepository<Renter> renterRepository, IRepository<Rental> rentalRepository, RentalPriceTierRepository rentalPriceTierRepository, IDialogService dialogService)
    {
        _rackRepository = rackRepository;
        _renterRepository = renterRepository;
        _rentalRepository = rentalRepository;
        _rentalPriceTierRepository = rentalPriceTierRepository;
        _dialogService = dialogService;

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

        CheckAllTerminationDates();

        _terminationCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _terminationCheckTimer.Tick += (_, _) => CheckAllTerminationDates();
        _terminationCheckTimer.Start();

        SelectRackCommand = new RelayCommand(item =>
        {
            SafeExecute(() =>
            {
                var clicked = (RackDisplayItem)item!;

                RefreshTerminationInfo(clicked);

                if (!clicked.IsSelected &&
                    (clicked.Rack.Status == RackStatus.Rented || clicked.Rack.Status == RackStatus.Terminated))
                {
                    var relevantRental = clicked.Rack.Status == RackStatus.Rented
                        ? _rentalRepository.GetAll().FirstOrDefault(r => r.RackId == clicked.Rack.RackId && r.EndDate == null)
                        : _rentalRepository.GetAll().Where(r => r.RackId == clicked.Rack.RackId && r.EndDate != null)
                            .OrderByDescending(r => r.EndDate).FirstOrDefault();

                    if (relevantRental != null)
                    {
                        if (FoundRenter != null && relevantRental.RenterId != FoundRenter.RenterId)
                        {
                            _dialogService.ShowInfo(
                                "Denne reol tilhører en anden lejer end den, du allerede har valgt. " +
                                "Ryd søgefeltet, hvis du vil skifte til en anden lejer.",
                                "Reol tilhører en anden lejer");
                            return;
                        }

                        var renter = _renterRepository.GetById(relevantRental.RenterId);
                        if (renter != null)
                        {
                            FoundRenter = renter;
                            SearchResults.Clear();
                            LoadRenterRacks();
                        }
                    }
                }

                clicked.IsSelected = !clicked.IsSelected;

                if (clicked.IsSelected)
                {
                    _selectedRacks.Add(clicked);
                }
                else
                {
                    _selectedRacks.Remove(clicked);

                    if (_selectedRacks.Count == 0)
                    {
                        ClearRenterSelection();
                    }
                }
            });
        });

        SelectRenterCommand = new RelayCommand(item =>
        {
            SafeExecute(() =>
            {
                FoundRenter = (Renter)item!;
                SearchResults.Clear();
                LoadRenterRacks();
            });
        });

        CreateRentalCommand = new RelayCommand(
            _ => SafeExecute(CreateRental),
            _ => FoundRenter != null && _selectedRacks.Any(r => r.Rack.Status == RackStatus.Available));

        TerminateRentalCommand = new RelayCommand(
            _ => SafeExecute(TerminateRental),
            _ => FoundRenter != null && _selectedRacks.Any(r => r.Rack.Status == RackStatus.Rented));

        CancelTerminationCommand = new RelayCommand(
            _ => SafeExecute(CancelTermination),
            _ => FoundRenter != null && _selectedRacks.Any(r => r.Rack.Status == RackStatus.Terminated));
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
        catch (SqlException ex)
        {
            _dialogService.ShowError(
                $"Der opstod en fejl i databasen.\n\nTeknisk besked: {ex.Message}",
                "Databasefejl");
        }
    }

    private void CheckAllTerminationDates()
    {
        SafeExecute(() =>
        {
            foreach (var item in Racks)
                RefreshTerminationInfo(item);
        });
    }

    private void RefreshTerminationInfo(RackDisplayItem item)
    {
        if (item.Rack.Status != RackStatus.Terminated)
        {
            item.TerminationDate = null;
            return;
        }

        var relevantRental = _rentalRepository.GetAll()
            .Where(r => r.RackId == item.Rack.RackId && r.EndDate != null)
            .OrderByDescending(r => r.EndDate)
            .FirstOrDefault();

        if (relevantRental == null)
        {
            item.TerminationDate = null;
            return;
        }

        if (relevantRental.EndDate <= DateTime.Now.Date)
        {
            item.Rack.Status = RackStatus.Available;
            _rackRepository.Update(item.Rack);
            item.TerminationDate = null;
            item.RefreshStatus();
        }
        else
        {
            item.TerminationDate = relevantRental.EndDate;
        }
    }

    private void PerformSearch()
    {
        SearchResults.Clear();

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FoundRenter = null;
            RenterRacks.Clear();
            OnPropertyChanged(nameof(TotalMonthlyRent));
            return;
        }

        SafeExecute(() =>
        {
            var matches = _renterRepository.GetAll()
                .Where(r =>
                    (r.Email != null && r.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (r.Phone != null && r.Phone.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));

            foreach (var renter in matches)
                SearchResults.Add(renter);
        });

        FoundRenter = null;
        RenterRacks.Clear();
        OnPropertyChanged(nameof(TotalMonthlyRent));
    }

    private void LoadRenterRacks()
    {
        RenterRacks.Clear();

        if (FoundRenter == null)
        {
            OnPropertyChanged(nameof(TotalMonthlyRent));
            return;
        }

        var relevantRentals = _rentalRepository.GetAll()
            .Where(r => r.RenterId == FoundRenter.RenterId &&
                        (r.EndDate == null || r.EndDate > DateTime.Now.Date));

        foreach (var rental in relevantRentals)
        {
            var rackItem = Racks.FirstOrDefault(r => r.Rack.RackId == rental.RackId);
            if (rackItem != null)
                RenterRacks.Add(new RenterRackDisplayItem(rackItem, rental));
        }

        OnPropertyChanged(nameof(TotalMonthlyRent));
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
            RenterRacks.Add(new RenterRackDisplayItem(item, rental));
        }

        RecalculateRentPricing();
        OnPropertyChanged(nameof(TotalMonthlyRent));
        _selectedRacks.Clear();
        ClearRenterSelection();
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

            var effectiveDate = CalculateTerminationEffectiveDate(DateTime.Now);

            rental.EndDate = effectiveDate;
            _rentalRepository.Update(rental);

            item.Rack.Status = RackStatus.Terminated;
            item.TerminationDate = effectiveDate;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();

            item.IsSelected = false;
            RenterRacks.RemoveAll(r => r.RackItem == item);
            RenterRacks.Add(new RenterRackDisplayItem(item, rental));
        }

        RecalculateRentPricing();
        OnPropertyChanged(nameof(TotalMonthlyRent));
        _selectedRacks.Clear();
        ClearRenterSelection();
    }

    private void CancelTermination()
    {
        if (FoundRenter == null)
            return;

        foreach (var item in _selectedRacks.ToList())
        {
            if (item.Rack.Status != RackStatus.Terminated)
            {
                item.IsSelected = false;
                continue;
            }

            var rental = _rentalRepository.GetAll()
                .Where(r => r.RackId == item.Rack.RackId && r.RenterId == FoundRenter.RenterId && r.EndDate != null)
                .OrderByDescending(r => r.EndDate)
                .FirstOrDefault();

            if (rental == null)
            {
                item.IsSelected = false;
                continue;
            }

            rental.EndDate = null;
            _rentalRepository.Update(rental);

            item.Rack.Status = RackStatus.Rented;
            item.TerminationDate = null;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();

            item.IsSelected = false;
            RenterRacks.RemoveAll(r => r.RackItem == item);
            RenterRacks.Add(new RenterRackDisplayItem(item, rental));
        }

        RecalculateRentPricing();
        OnPropertyChanged(nameof(TotalMonthlyRent));
        _selectedRacks.Clear();
        ClearRenterSelection();
    }

    private void RecalculateRentPricing()
    {
        if (FoundRenter == null)
            return;

        var activeRentals = _rentalRepository.GetAll()
            .Where(r => r.RenterId == FoundRenter.RenterId && r.EndDate == null)
            .ToList();

        if (activeRentals.Count == 0)
            return;

        var priceTiers = _rentalPriceTierRepository.GetAll();
        var monthlyRentPerRack = RentalPriceCalculator.CalculateMonthlyRent(activeRentals.Count, priceTiers) / activeRentals.Count;

        foreach (var rental in activeRentals)
        {
            rental.MonthlyRent = monthlyRentPerRack;
            _rentalRepository.Update(rental);
        }
    }

    private void ClearRenterSelection()
    {
        FoundRenter = null;
        SearchQuery = string.Empty;
        RenterRacks.Clear();
        OnPropertyChanged(nameof(TotalMonthlyRent));
    }

    private static DateTime CalculateTerminationEffectiveDate(DateTime today)
    {
        var monthsToAdd = today.Day < 20 ? 1 : 2;
        var target = today.AddMonths(monthsToAdd);
        return new DateTime(target.Year, target.Month, 1);
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