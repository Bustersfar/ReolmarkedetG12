using System.Collections.ObjectModel;
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
    private readonly IRentalPriceTierRepository _rentalPriceTierRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IDialogService _dialogService;
    private readonly DispatcherTimer _terminationCheckTimer;
    private bool _timerCheckFailed;

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

    public ObservableCollection<RackDisplayItem> SelectedRacks { get; } = new();
    public ObservableCollection<RenterRackDisplayItem> RenterRacks { get; } = new();
    public ObservableCollection<Renter> SearchResults { get; } = new();

    public Action<List<int>, decimal, string>? OnSendToCheckout { get; set; }

    // =========================================================================
    // MÆNGDE- OG STATUSPROPERTIES
    // =========================================================================

    public int ActiveRentalsCount => RenterRacks.Count(r => r.Rental.EndDate == null);
    public int SelectedAvailableRacksCount => SelectedRacks.Count(r => r.Rack.Status == RackStatus.Available);
    public int SelectedRentedRacksCount => SelectedRacks.Count(r => r.Rack.Status == RackStatus.Rented);
    public int SelectedTerminatedRacksCount => SelectedRacks.Count(r => r.Rack.Status == RackStatus.Terminated);

    public bool ShowNewRentalPriceBox => SelectedAvailableRacksCount > 0;

    public decimal CurrentMonthlyRent
    {
        get
        {
            if (ActiveRentalsCount == 0) return 0m;
            var tiers = _rentalPriceTierRepository.GetAll();
            return RentalPriceCalculator.CalculateMonthlyRent(ActiveRentalsCount, tiers);
        }
    }

    public decimal NewMonthlyRent
    {
        get
        {
            int total = ActiveRentalsCount + SelectedAvailableRacksCount;
            if (total == 0) return 0m;
            var tiers = _rentalPriceTierRepository.GetAll();
            return RentalPriceCalculator.CalculateMonthlyRent(total, tiers);
        }
    }

    public decimal AdditionalMonthlyRent => NewMonthlyRent - CurrentMonthlyRent;

    public decimal FirstMonthPaymentAmount
    {
        get
        {
            if (SelectedAvailableRacksCount == 0) return 0m;
            var today = DateOnly.FromDateTime(DateTime.Now);
            return RentalPriceCalculator.CalculatePartialMonthRent(AdditionalMonthlyRent, today);
        }
    }

    public decimal TotalMonthlyRent => CurrentMonthlyRent;

    // =========================================================================
    // SØGNING OG KUNDE
    // =========================================================================

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
        private set
        {
            _foundRenter = value;
            OnPropertyChanged();
            NotifyPriceChanges();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ICommand SelectRackCommand { get; }
    public ICommand SelectRenterCommand { get; }
    public ICommand ClearRenterCommand { get; }
    public ICommand CreateRentalCommand { get; }
    public ICommand TerminateRentalCommand { get; }
    public ICommand CancelTerminationCommand { get; }

    public RackViewModel(
        IRepository<Rack> rackRepository,
        IRepository<Renter> renterRepository,
        IRepository<Rental> rentalRepository,
        IRentalPriceTierRepository rentalPriceTierRepository,
        IRepository<Payment> paymentRepository,
        IDialogService dialogService)
    {
        _rackRepository = rackRepository;
        _renterRepository = renterRepository;
        _rentalRepository = rentalRepository;
        _rentalPriceTierRepository = rentalPriceTierRepository;
        _paymentRepository = paymentRepository;
        _dialogService = dialogService;

        List<RackDisplayItem> allItems;
        try
        {
            allItems = _rackRepository.GetAll()
                .Select(rack => new RackDisplayItem(rack))
                .ToList();
        }
        catch (DatabaseConnectionException ex)
        {
            _dialogService.ShowError(
                $"{ex.Message}\n\nTeknisk besked: {ex.InnerException?.Message ?? "ukendt"}",
                "Forbindelsesfejl");
            allItems = new List<RackDisplayItem>();
        }
        catch (SqlException ex)
        {
            _dialogService.ShowError(
                $"Der opstod en fejl i databasen.\n\nTeknisk besked: {ex.Message}",
                "Databasefejl");
            allItems = new List<RackDisplayItem>();
        }

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

        SafeExecute(RefreshAllTerminationInfo);

        _terminationCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _terminationCheckTimer.Tick += (_, _) => CheckTerminationDatesFromTimer();
        _terminationCheckTimer.Start();

        SelectRackCommand = new RelayCommand(item =>
        {
            SafeExecute(() =>
            {
                var clicked = (RackDisplayItem)item!;
                RefreshTerminationInfo(clicked);

                if (clicked.Rack.Status == RackStatus.Available)
                {
                    // LEDIG REOL: fjern evt. markerede optagne/opsagte reoler fra valget,
                    // men behold den fundne kunde - det er netop sådan man lejer en
                    // ekstra reol ud til en kunde man allerede har fundet (ved søgning,
                    // eller ved at klikke en af deres nuværende reoler).
                    foreach (var r in SelectedRacks.Where(r => r.Rack.Status != RackStatus.Available).ToList())
                    {
                        r.IsSelected = false;
                        SelectedRacks.Remove(r);
                    }
                }
                else
                {
                    // OPTAGET/OPSAGT REOL: find og vis dens faktiske lejer.
                    // Et klik her skifter ALTID roligt til den rigtige lejer -
                    // der vises aldrig en blokerende fejl.
                    var relevantRental = clicked.Rack.Status == RackStatus.Rented
                        ? _rentalRepository.GetAll().FirstOrDefault(r => r.RackId == clicked.Rack.RackId && r.EndDate == null)
                        : _rentalRepository.GetAll().Where(r => r.RackId == clicked.Rack.RackId && r.EndDate != null)
                            .OrderByDescending(r => r.EndDate).FirstOrDefault();

                    if (relevantRental != null && (FoundRenter == null || relevantRental.RenterId != FoundRenter.RenterId))
                    {
                        var renter = _renterRepository.GetById(relevantRental.RenterId);
                        if (renter != null)
                        {
                            // Skifter vi til en anden kunde, giver gamle markeringer
                            // ikke længere mening - ryd dem.
                            foreach (var r in SelectedRacks.ToList())
                            {
                                r.IsSelected = false;
                                SelectedRacks.Remove(r);
                            }

                            FoundRenter = renter;
                            SearchResults.Clear();
                            LoadRenterRacks();
                        }
                    }

                    // Fjern ledige reoler, så vi kun har egne optagne valgt
                    foreach (var a in SelectedRacks.Where(r => r.Rack.Status == RackStatus.Available).ToList())
                    {
                        a.IsSelected = false;
                        SelectedRacks.Remove(a);
                    }
                }

                // Skift markering
                clicked.IsSelected = !clicked.IsSelected;

                if (clicked.IsSelected)
                {
                    if (!SelectedRacks.Contains(clicked))
                        SelectedRacks.Add(clicked);
                }
                else
                {
                    SelectedRacks.Remove(clicked);

                    // Fravalgte vi den sidste markerede reol, er der ikke længere
                    // nogen grund til at holde kunden "åben" - nulstil helt.
                    if (SelectedRacks.Count == 0)
                    {
                        FoundRenter = null;
                        RenterRacks.Clear();
                    }
                }

                NotifyPriceChanges();
                CommandManager.InvalidateRequerySuggested();
            });
        });

        SelectRenterCommand = new RelayCommand(item =>
        {
            SafeExecute(() =>
            {
                FoundRenter = (Renter)item!;
                SearchResults.Clear();
                ClearSelectedRacks();
                LoadRenterRacks();
            });
        });

        ClearRenterCommand = new RelayCommand(_ => ClearAllSelection());

        CreateRentalCommand = new RelayCommand(
            _ => SafeExecute(CreateRental),
            _ => FoundRenter != null && SelectedAvailableRacksCount > 0 && SelectedAvailableRacksCount == SelectedRacks.Count);

        TerminateRentalCommand = new RelayCommand(
            _ => SafeExecute(TerminateRental),
            _ => FoundRenter != null && SelectedRentedRacksCount > 0 && SelectedRentedRacksCount == SelectedRacks.Count);

        CancelTerminationCommand = new RelayCommand(
            _ => SafeExecute(CancelTermination),
            _ => FoundRenter != null && SelectedTerminatedRacksCount > 0 && SelectedTerminatedRacksCount == SelectedRacks.Count);
    }

    public void ClearAllSelection()
    {
        ClearSelectedRacks();
        FoundRenter = null;
        SearchQuery = string.Empty;
        SearchResults.Clear();
        RenterRacks.Clear();
        NotifyPriceChanges();
        CommandManager.InvalidateRequerySuggested();
    }

    private void ClearSelectedRacks()
    {
        foreach (var r in SelectedRacks.ToList())
        {
            r.IsSelected = false;
        }
        SelectedRacks.Clear();
        NotifyPriceChanges();
        CommandManager.InvalidateRequerySuggested();
    }

    private void NotifyPriceChanges()
    {
        OnPropertyChanged(nameof(ActiveRentalsCount));
        OnPropertyChanged(nameof(SelectedAvailableRacksCount));
        OnPropertyChanged(nameof(SelectedRentedRacksCount));
        OnPropertyChanged(nameof(SelectedTerminatedRacksCount));
        OnPropertyChanged(nameof(ShowNewRentalPriceBox));
        OnPropertyChanged(nameof(CurrentMonthlyRent));
        OnPropertyChanged(nameof(NewMonthlyRent));
        OnPropertyChanged(nameof(AdditionalMonthlyRent));
        OnPropertyChanged(nameof(FirstMonthPaymentAmount));
        OnPropertyChanged(nameof(TotalMonthlyRent));
    }

    private bool SafeExecute(Action action, bool showError = true)
    {
        try
        {
            action();
            return true;
        }
        catch (DatabaseConnectionException ex)
        {
            if (showError)
            {
                _dialogService.ShowError(
                    $"{ex.Message}\n\nTeknisk besked: {ex.InnerException?.Message ?? "ukendt"}",
                    "Forbindelsesfejl");
            }
            return false;
        }
        catch (SqlException ex)
        {
            if (showError)
            {
                _dialogService.ShowError(
                    $"Der opstod en fejl i databasen.\n\nTeknisk besked: {ex.Message}",
                    "Databasefejl");
            }
            return false;
        }
    }

    private void RefreshAllTerminationInfo()
    {
        foreach (var item in Racks)
            RefreshTerminationInfo(item);
    }

    public void CheckTerminationDatesFromTimer()
    {
        _timerCheckFailed = !SafeExecute(RefreshAllTerminationInfo, showError: !_timerCheckFailed);
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
            if (FoundRenter != null && SelectedRacks.Count == 0)
            {
                FoundRenter = null;
                RenterRacks.Clear();
            }
            NotifyPriceChanges();
            return;
        }

        SafeExecute(() =>
        {
            var matches = _renterRepository.GetAll()
                .Where(r =>
                    $"{r.FirstName} {r.LastName} {r.Email} {r.Phone} {r.Address} {r.PostalCode} {r.City}"
                        .Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            foreach (var renter in matches)
                SearchResults.Add(renter);
        });

        NotifyPriceChanges();
    }

    private void LoadRenterRacks()
    {
        RenterRacks.Clear();

        if (FoundRenter == null)
        {
            NotifyPriceChanges();
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

        NotifyPriceChanges();
    }

    private void CreateRental()
    {
        if (FoundRenter == null)
            return;

        var newItems = SelectedRacks.Where(r => r.Rack.Status == RackStatus.Available).ToList();
        if (newItems.Count == 0)
            return;

        int existingCount = ActiveRentalsCount;
        int newTotal = existingCount + newItems.Count;
        decimal newTotalMonthly = NewMonthlyRent;
        decimal paymentNow = FirstMonthPaymentAmount;

        var rackNumbersText = string.Join(", ", newItems.Select(r => $"Reol {r.Rack.Number}"));

        string message = $"Vil du oprette lejeaftale for {FoundRenter.FirstName} {FoundRenter.LastName}?\n\n" +
                         $"• Reoler: {rackNumbersText}\n" +
                         $"• Til betaling ved kassen nu: {paymentNow:0.00} kr.\n" +
                         $"• Fast husleje fremover: {newTotalMonthly:0.00} kr./md. ({newTotal} stk.)\n\n" +
                         $"Tryk 'Ja' for at oprette og sende betalingen til kassen.";

        if (!_dialogService.Confirm(message, "Bekræft oprettelse"))
            return;

        var tiers = _rentalPriceTierRepository.GetAll();
        int position = existingCount;
        var createdRackNumbers = new List<int>();

        foreach (var item in newItems)
        {
            position++;
            decimal tierPrice = RentalPriceCalculator.CalculateRackPriceAtPosition(position, tiers);

            var rental = new Rental
            {
                RackId = item.Rack.RackId,
                RenterId = FoundRenter.RenterId,
                StartDate = DateTime.Now,
                EndDate = null,
                MonthlyRent = tierPrice
            };
            _rentalRepository.Add(rental);

            item.Rack.Status = RackStatus.Rented;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();

            item.IsSelected = false;
            createdRackNumbers.Add(item.Rack.Number);
        }

        if (paymentNow > 0 && OnSendToCheckout != null)
        {
            OnSendToCheckout(createdRackNumbers, paymentNow, $"{FoundRenter.FirstName} {FoundRenter.LastName}");
        }

        ClearSelectedRacks();
        LoadRenterRacks();
    }

    private void TerminateRental()
    {
        if (FoundRenter == null)
            return;

        var itemsToTerminate = SelectedRacks.Where(r => r.Rack.Status == RackStatus.Rented).ToList();
        if (itemsToTerminate.Count == 0)
            return;

        var effectiveDate = CalculateTerminationEffectiveDate(DateTime.Now);

        foreach (var item in itemsToTerminate)
        {
            var rental = _rentalRepository.GetAll()
                .FirstOrDefault(r => r.RackId == item.Rack.RackId && r.RenterId == FoundRenter.RenterId && r.EndDate == null);

            if (rental == null)
                continue;

            rental.EndDate = effectiveDate;
            _rentalRepository.Update(rental);

            item.Rack.Status = RackStatus.Terminated;
            item.TerminationDate = effectiveDate;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();

            item.IsSelected = false;
        }

        ClearSelectedRacks();
        LoadRenterRacks();
    }

    private void CancelTermination()
    {
        if (FoundRenter == null)
            return;

        var itemsToCancel = SelectedRacks.Where(r => r.Rack.Status == RackStatus.Terminated).ToList();
        if (itemsToCancel.Count == 0)
            return;

        foreach (var item in itemsToCancel)
        {
            var rental = _rentalRepository.GetAll()
                .Where(r => r.RackId == item.Rack.RackId && r.RenterId == FoundRenter.RenterId && r.EndDate != null)
                .OrderByDescending(r => r.EndDate)
                .FirstOrDefault();

            if (rental == null)
                continue;

            rental.EndDate = null;
            _rentalRepository.Update(rental);

            item.Rack.Status = RackStatus.Rented;
            item.TerminationDate = null;
            _rackRepository.Update(item.Rack);
            item.RefreshStatus();

            item.IsSelected = false;
        }

        ClearSelectedRacks();
        LoadRenterRacks();
    }

    public static DateTime CalculateTerminationEffectiveDate(DateTime today)
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