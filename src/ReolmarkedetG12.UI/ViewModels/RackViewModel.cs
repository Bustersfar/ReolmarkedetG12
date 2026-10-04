using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.Core.Services;
using ReolmarkedetG12.UI.MVVM;
using ReolmarkedetG12.UI.Services;

namespace ReolmarkedetG12.UI.ViewModels;

public class RackViewModel : ViewModelBase, IDisposable
{
    // Navngivne konstanter i stedet for "magic numbers" spredt i koden.
    // Disse definerer butikkens fysiske plantegning: reol 1-13 står i en
    // venstre søjle, og reol 14-80 er opdelt i klynger (se RackView.xaml).
    private const decimal FallbackMonthlyRent = 850m; // Brugt hvis der ingen prisregler findes i databasen
    private const int LeftColumnRackStart = 1;
    private const int LeftColumnRackEnd = 13;
    private const int Cluster1418Start = 14;
    private const int Cluster1418End = 18;
    private const int Cluster1924Start = 19;
    private const int Cluster1924End = 24;
    private const int Cluster2538Start = 25;
    private const int Cluster2538End = 38;
    private const int Cluster3952Start = 39;
    private const int Cluster3952End = 52;
    private const int Cluster5366Start = 53;
    private const int Cluster5366End = 66;
    private const int Cluster6776Start = 67;
    private const int Cluster6776End = 76;
    private const int Cluster7778Start = 77;
    private const int Cluster7778End = 78;
    private const int Cluster7980Start = 79;
    private const int Cluster7980End = 80;

    private readonly IRepository<Rack> _rackRepository;
    private readonly IRepository<Renter> _renterRepository;
    private readonly IRepository<Rental> _rentalRepository;
    private readonly IRentalPriceTierRepository? _priceTierRepository;
    private readonly IRepository<Payment>? _paymentRepository;
    private readonly IDialogService _dialogService;

    private readonly DispatcherTimer? _terminationCheckTimer;
    private bool _hasShownTimerDbError;

    // --- Samlinger af reoler ---
    // "Racks" er den flade liste (bruges bl.a. til opslag andre steder i ViewModel'en).
    // De øvrige samlinger genskaber butikkens fysiske plantegning og bruges af
    // RackView.xaml til at tegne reolerne, som de reelt står i butikken.
    public ObservableCollection<RackDisplayItem> Racks { get; } = [];
    public ObservableCollection<RackDisplayItem> LeftColumnRacks { get; } = [];
    public ObservableCollection<RackDisplayItem> Cluster_14_18 { get; } = [];
    public ObservableCollection<RackDisplayItem> Cluster_19_24 { get; } = [];
    public ObservableCollection<RackDisplayItem> Cluster_25_38 { get; } = [];
    public ObservableCollection<RackDisplayItem> Cluster_39_52 { get; } = [];
    public ObservableCollection<RackDisplayItem> Cluster_53_66 { get; } = [];
    public ObservableCollection<RackDisplayItem> Cluster_67_76 { get; } = [];
    public ObservableCollection<RackDisplayItem> Cluster_77_78 { get; } = [];
    public ObservableCollection<RackDisplayItem> Cluster_79_80 { get; } = [];

    public ObservableCollection<RackDisplayItem> SelectedRacks { get; } = [];

    // --- Lejersøgning og visning ---
    public ObservableCollection<Renter> SearchResults { get; } = [];
    public ObservableCollection<RenterRackDisplayItem> RenterRacks { get; } = [];

    // --- Lejemålshistorik (Punkt 14) ---
    public ObservableCollection<RackHistoryDisplayItem> RackHistory { get; } = [];

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

    private Renter? _foundRenter;
    public Renter? FoundRenter
    {
        get => _foundRenter;
        set => SetProperty(ref _foundRenter, value);
    }

    private RackDisplayItem? _selectedRack;
    public RackDisplayItem? SelectedRack
    {
        get => _selectedRack;
        set => SetProperty(ref _selectedRack, value);
    }

    // Sand, når FoundRenter blev valgt direkte via SelectRenterCommand (søgningen),
    // i modsætning til at være fundet automatisk ved at klikke en udlejet reol.
    // Bruges til at afgøre, om lejeren skal blive stående, når man bagefter
    // vælger en ledig reol til samme kunde (f.eks. reol nr. 5, 6, 7...).
    private bool _renterSelectedManually;

    // --- Kommandoer ---
    public RelayCommand SelectRackCommand { get; }
    public RelayCommand SelectRenterCommand { get; }
    public RelayCommand CreateRentalCommand { get; }
    public RelayCommand TerminateRentalCommand { get; }
    public RelayCommand CancelTerminationCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public RackViewModel(
        IRepository<Rack> rackRepository,
        IRepository<Renter> renterRepository,
        IRepository<Rental> rentalRepository,
        IRentalPriceTierRepository? priceTierRepository,
        IRepository<Payment>? paymentRepository,
        IDialogService dialogService)
        : base(dialogService)
    {
        _rackRepository = rackRepository;
        _renterRepository = renterRepository;
        _rentalRepository = rentalRepository;
        _priceTierRepository = priceTierRepository;
        _paymentRepository = paymentRepository;
        _dialogService = dialogService;

        SelectRackCommand = new RelayCommand(param => SafeExecute(() => OnSelectRack(param as RackDisplayItem)));
        SelectRenterCommand = new RelayCommand(param => SafeExecute(() => OnSelectRenter(param as Renter, retainSelectedRack: false)));
        CreateRentalCommand = new RelayCommand(_ => SafeExecute(OnCreateRental), _ => CanCreateRental());
        TerminateRentalCommand = new RelayCommand(_ => SafeExecute(OnTerminateRental), _ => CanTerminateRental());
        CancelTerminationCommand = new RelayCommand(_ => SafeExecute(OnCancelTermination), _ => CanCancelTermination());
        RefreshCommand = new RelayCommand(_ => SafeExecute(LoadAllRacks));

        // Tjek udløbne opsigelser ved opstart
        CheckAndApplyTerminations();

        // Hent data
        LoadAllRacks();

        // Timer til periodisk tjek af udløbne opsigelser (hvis UI-tråd er til stede)
        if (Dispatcher.CurrentDispatcher != null)
        {
            _terminationCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(10)
            };
            _terminationCheckTimer.Tick += (_, _) => CheckTerminationDatesFromTimer();
            _terminationCheckTimer.Start();
        }
    }

    public static DateTime CalculateTerminationEffectiveDate(DateTime now)
    {
        if (now.Day < 20)
        {
            var nextMonth = now.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, 1);
        }
        else
        {
            var nextNextMonth = now.AddMonths(2);
            return new DateTime(nextNextMonth.Year, nextNextMonth.Month, 1);
        }
    }

    public void CheckTerminationDatesFromTimer()
    {
        try
        {
            CheckAndApplyTerminations();
            _hasShownTimerDbError = false;
        }
        catch (DatabaseConnectionException ex)
        {
            if (!_hasShownTimerDbError)
            {
                _dialogService.ShowError(
                    $"{ex.Message}\n\nTeknisk besked: {ex.InnerException?.Message ?? "ukendt"}",
                    "Forbindelsesfejl");
                _hasShownTimerDbError = true;
            }
        }
        catch (Exception ex)
        {
            if (!_hasShownTimerDbError)
            {
                _dialogService.ShowError(ex.Message, "Fejl");
                _hasShownTimerDbError = true;
            }
        }
    }

    private void CheckAndApplyTerminations()
    {
        var terminatedRacks = _rackRepository.GetAll().Where(r => r.Status == RackStatus.Terminated).ToList();
        var rentals = _rentalRepository.GetAll().ToList();
        var now = DateTime.Now;

        foreach (var rack in terminatedRacks)
        {
            var rental = rentals.FirstOrDefault(r => r.RackId == rack.RackId && r.EndDate.HasValue && r.EndDate.Value <= now);
            if (rental != null)
            {
                rack.Status = RackStatus.Available;
                _rackRepository.Update(rack);
            }
        }
    }

    public void LoadAllRacks()
    {
        Racks.Clear();
        LeftColumnRacks.Clear();
        Cluster_14_18.Clear();
        Cluster_19_24.Clear();
        Cluster_25_38.Clear();
        Cluster_39_52.Clear();
        Cluster_53_66.Clear();
        Cluster_67_76.Clear();
        Cluster_77_78.Clear();
        Cluster_79_80.Clear();
        SelectedRacks.Clear();
        RackHistory.Clear();

        var racks = _rackRepository.GetAll().OrderBy(r => r.Number).ToList();

        foreach (var rack in racks)
        {
            var item = new RackDisplayItem(rack);
            Racks.Add(item);

            // Reol 0 (butikssalg) hører ikke til nogen fysisk klynge i plantegningen.
            // Venstre søjle vises højeste nummer først (13 i toppen, 1 i bunden),
            // ligesom i butikkens oprindelige plantegning.
            if (rack.Number is >= LeftColumnRackStart and <= LeftColumnRackEnd)
                LeftColumnRacks.Insert(0, item);
            else if (rack.Number is >= Cluster1418Start and <= Cluster1418End)
                Cluster_14_18.Add(item);
            else if (rack.Number is >= Cluster1924Start and <= Cluster1924End)
                Cluster_19_24.Add(item);
            else if (rack.Number is >= Cluster2538Start and <= Cluster2538End)
                Cluster_25_38.Add(item);
            else if (rack.Number is >= Cluster3952Start and <= Cluster3952End)
                Cluster_39_52.Add(item);
            else if (rack.Number is >= Cluster5366Start and <= Cluster5366End)
                Cluster_53_66.Add(item);
            else if (rack.Number is >= Cluster6776Start and <= Cluster6776End)
                Cluster_67_76.Add(item);
            else if (rack.Number is >= Cluster7778Start and <= Cluster7778End)
                Cluster_77_78.Add(item);
            else if (rack.Number is >= Cluster7980Start and <= Cluster7980End)
                Cluster_79_80.Add(item);
        }
    }

    private void OnSelectRack(RackDisplayItem? item)
    {
        if (item == null) return;

        SelectedRack = item;

        // Toggle markering
        if (item.IsSelected)
        {
            item.IsSelected = false;
            SelectedRacks.Remove(item);

            if (SelectedRacks.Count == 0)
            {
                FoundRenter = null;
                RenterRacks.Clear();
                RackHistory.Clear();
                _renterSelectedManually = false;
            }
            CommandManager.InvalidateRequerySuggested();
            return;
        }

        item.IsSelected = true;
        SelectedRacks.Add(item);

        // Hent lejemålshistorik for den valgte reol (Punkt 14)
        LoadRackHistory(item.Rack.RackId, item.Rack.Number);

        // Håndtering af tilknyttet lejer, hvis reolen er udlejet
        if (item.Rack.Status is RackStatus.Rented or RackStatus.Terminated)
        {
            var activeRental = _rentalRepository.GetAll()
                .Where(r => r.RackId == item.Rack.RackId && (r.EndDate == null || r.EndDate > DateTime.UtcNow))
                .OrderByDescending(r => r.StartDate)
                .FirstOrDefault();

            if (activeRental != null)
            {
                var renter = _renterRepository.GetById(activeRental.RenterId);
                if (renter != null)
                {
                    OnSelectRenter(renter, retainSelectedRack: true);
                }
            }
        }
        else if (item.Rack.Status == RackStatus.Available)
        {
            // Nulstil kun den fundne lejer, hvis vedkommende ikke allerede er valgt
            // via SelectRenterCommand (så "Kundens reoler" ikke forsvinder, når man
            // bagefter vælger en ledig reol til den samme kunde - uanset hvor mange
            // reoler kunden allerede har).
            bool hasOtherRentedSelected = SelectedRacks.Any(r => r != item && r.Rack.Status == RackStatus.Rented);

            if (!hasOtherRentedSelected && !_renterSelectedManually)
            {
                FoundRenter = null;
                RenterRacks.Clear();
            }
        }

        CommandManager.InvalidateRequerySuggested();
    }

    private void OnSelectRenter(Renter? renter, bool retainSelectedRack = false)
    {
        if (renter == null) return;

        FoundRenter = renter;
        RenterRacks.Clear();
        _renterSelectedManually = !retainSelectedRack;

        if (!retainSelectedRack)
        {
            foreach (var r in SelectedRacks)
                r.IsSelected = false;
            SelectedRacks.Clear();
        }

        var renterRentals = _rentalRepository.GetAll()
            .Where(r => r.RenterId == renter.RenterId && (r.EndDate == null || r.EndDate > DateTime.UtcNow))
            .ToList();

        foreach (var rental in renterRentals)
        {
            var rackItem = Racks.FirstOrDefault(r => r.Rack.RackId == rental.RackId);
            if (rackItem != null)
            {
                RenterRacks.Add(new RenterRackDisplayItem(rackItem, rental));
            }
        }

        CommandManager.InvalidateRequerySuggested();
    }

    private void LoadRackHistory(int rackId, int rackNumber)
    {
        RackHistory.Clear();

        IEnumerable<Rental> completedRentals;
        if (_rentalRepository is RentalRepository concreteRentalRepo)
        {
            completedRentals = concreteRentalRepo.GetCompletedRentalsByRackId(rackId);
        }
        else
        {
            completedRentals = _rentalRepository.GetAll()
                .Where(r => r.RackId == rackId && r.EndDate != null && r.EndDate <= DateTime.UtcNow)
                .OrderByDescending(r => r.EndDate);
        }

        var renters = _renterRepository.GetAll().ToDictionary(r => r.RenterId);

        foreach (var rental in completedRentals)
        {
            string renterName = "Ukendt lejer";
            string contact = "-";

            if (renters.TryGetValue(rental.RenterId, out var renter))
            {
                renterName = $"{renter.FirstName} {renter.LastName}";
                contact = !string.IsNullOrWhiteSpace(renter.Phone) ? renter.Phone : renter.Email ?? "-";
            }

            RackHistory.Add(new RackHistoryDisplayItem
            {
                RentalId = rental.RentalId,
                RackNumber = rackNumber,
                RenterName = renterName,
                RenterContact = contact,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                MonthlyRent = rental.MonthlyRent
            });
        }
    }

    private void FilterRenters()
    {
        SearchResults.Clear();
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        var query = SearchQuery.Trim().ToLowerInvariant();
        var renters = _renterRepository.GetAll().Where(r =>
            r.FirstName.ToLowerInvariant().Contains(query) ||
            r.LastName.ToLowerInvariant().Contains(query) ||
            $"{r.FirstName} {r.LastName}".ToLowerInvariant().Contains(query) ||
            r.Address.ToLowerInvariant().Contains(query) ||
            r.City.ToLowerInvariant().Contains(query) ||
            (r.Phone != null && r.Phone.Contains(query))
        ).ToList();

        foreach (var r in renters)
            SearchResults.Add(r);
    }

    private bool CanCreateRental()
    {
        return FoundRenter != null &&
               SelectedRacks.Any(r => r.Rack.Status == RackStatus.Available);
    }

    private void OnCreateRental()
    {
        if (FoundRenter == null) return;

        var availableSelected = SelectedRacks.Where(r => r.Rack.Status == RackStatus.Available).ToList();
        if (availableSelected.Count == 0) return;

        var tiers = _priceTierRepository?.GetAll().ToList() ?? [];

        var existingActiveCount = _rentalRepository.GetAll()
            .Count(r => r.RenterId == FoundRenter.RenterId && (r.EndDate == null || r.EndDate > DateTime.UtcNow));

        var sortedSelectedRacks = availableSelected.OrderBy(r => r.Rack.Number).ToList();
        var rentalsToCreate = new List<(RackDisplayItem item, Rental rental)>();

        for (int i = 0; i < sortedSelectedRacks.Count; i++)
        {
            int position = existingActiveCount + i + 1;
            decimal price = tiers.Count > 0
                ? RentalPriceCalculator.CalculateRackPriceAtPosition(position, tiers)
                : FallbackMonthlyRent;

            var rental = new Rental
            {
                RackId = sortedSelectedRacks[i].Rack.RackId,
                RenterId = FoundRenter.RenterId,
                StartDate = DateTime.UtcNow,
                EndDate = null,
                MonthlyRent = price
            };

            rentalsToCreate.Add((sortedSelectedRacks[i], rental));
        }

        // Beregn total 1. måneds leje ud fra samlet månedlig leje for at undgå afrundingsafvigelser
        var today = DateOnly.FromDateTime(DateTime.Now);
        decimal totalMonthlyRent = rentalsToCreate.Sum(r => r.rental.MonthlyRent);
        decimal totalFirstMonth = RentalPriceCalculator.CalculatePartialMonthRent(totalMonthlyRent, today);

        bool confirmed = _dialogService.Confirm(
            $"Opret udlejning af {rentalsToCreate.Count} reol(er) til {FoundRenter.FirstName} {FoundRenter.LastName}?\n" +
            $"1. måneds leje i alt: {totalFirstMonth:0.00} kr.",
            "Bekræft udlejning");

        if (!confirmed) return;

        decimal accumulatedPayments = 0m;

        for (int i = 0; i < rentalsToCreate.Count; i++)
        {
            var (item, rental) = rentalsToCreate[i];

            if (_rentalRepository is RentalRepository concreteRepo)
            {
                concreteRepo.AddRentalWithRackStatus(rental, (int)RackStatus.Rented);
            }
            else
            {
                _rentalRepository.Add(rental);
                item.Rack.Status = RackStatus.Rented;
                _rackRepository.Update(item.Rack);
            }

            if (_paymentRepository != null)
            {
                decimal partialRent = (i == rentalsToCreate.Count - 1)
                    ? totalFirstMonth - accumulatedPayments
                    : RentalPriceCalculator.CalculatePartialMonthRent(rental.MonthlyRent, today);

                accumulatedPayments += partialRent;

                _paymentRepository.Add(new Payment
                {
                    RenterId = FoundRenter.RenterId,
                    Date = DateTime.Now,
                    Amount = partialRent,
                    Type = PaymentType.FirstMonthPayment,
                    PaymentMethod = PaymentMethod.MobilePay
                });
            }

            item.Rack.Status = RackStatus.Rented;
            item.RefreshStatus();
            item.IsSelected = false;
        }

        SelectedRacks.Clear();
        OnSelectRenter(FoundRenter, retainSelectedRack: false);
    }

    private bool CanTerminateRental()
    {
        return SelectedRacks.Count > 0 &&
               SelectedRacks.All(r => r.Rack.Status == RackStatus.Rented);
    }

    private void OnTerminateRental()
    {
        if (!CanTerminateRental()) return;

        var effectiveDate = CalculateTerminationEffectiveDate(DateTime.Now);
        var rentals = _rentalRepository.GetAll().ToList();

        foreach (var item in SelectedRacks.ToList())
        {
            var rental = rentals.FirstOrDefault(r => r.RackId == item.Rack.RackId && (r.EndDate == null || r.EndDate > DateTime.UtcNow));
            if (rental != null)
            {
                rental.EndDate = effectiveDate;
                _rentalRepository.Update(rental);
            }

            item.Rack.Status = RackStatus.Terminated;
            item.TerminationDate = effectiveDate;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();
        }

        CommandManager.InvalidateRequerySuggested();
    }

    private bool CanCancelTermination()
    {
        return SelectedRacks.Count > 0 &&
               SelectedRacks.All(r => r.Rack.Status == RackStatus.Terminated);
    }

    private void OnCancelTermination()
    {
        if (!CanCancelTermination()) return;

        var rentals = _rentalRepository.GetAll().ToList();

        foreach (var item in SelectedRacks.ToList())
        {
            var rental = rentals.FirstOrDefault(r => r.RackId == item.Rack.RackId && r.EndDate.HasValue);
            if (rental != null)
            {
                rental.EndDate = null;
                _rentalRepository.Update(rental);
            }

            item.Rack.Status = RackStatus.Rented;
            item.TerminationDate = null;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();
        }

        CommandManager.InvalidateRequerySuggested();
    }

    // SafeExecute ligger nu i ViewModelBase og deles af alle ViewModels.

    // Stopper den periodiske timer, så den ikke fortsætter med at køre i baggrunden,
    // hvis denne ViewModel på et tidspunkt bliver kasseret (f.eks. ved lukning af vinduet).
    public void Dispose()
    {
        _terminationCheckTimer?.Stop();
    }
}