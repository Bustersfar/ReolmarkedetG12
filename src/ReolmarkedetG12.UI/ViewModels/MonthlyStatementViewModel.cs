using System.Collections.ObjectModel;
using System.Globalization;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.Core.Services;
using ReolmarkedetG12.UI.MVVM;
using ReolmarkedetG12.UI.Services;

namespace ReolmarkedetG12.UI.ViewModels;

public class MonthlyStatementViewModel : ViewModelBase
{
    private readonly IRepository<Renter> _renterRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly ISaleRepository _saleRepository;

    public LockScreenViewModel Lock { get; }

    // Januar-december på dansk, til måneds-vælgeren
    public IReadOnlyList<string> MonthNames { get; } =
        new CultureInfo("da-DK").DateTimeFormat.MonthNames.Take(12).ToList();

    public IReadOnlyList<int> Years { get; }

    // 0 = januar, 11 = december (matcher ComboBox.SelectedIndex)
    private int _selectedMonthIndex;
    public int SelectedMonthIndex
    {
        get => _selectedMonthIndex;
        set
        {
            if (SetProperty(ref _selectedMonthIndex, value))
                Refresh();
        }
    }

    private int _selectedYear;
    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (SetProperty(ref _selectedYear, value))
                Refresh();
        }
    }

    public ObservableCollection<MonthlyStatementLine> Lines { get; } = [];

    private decimal _totalRent;
    public decimal TotalRent
    {
        get => _totalRent;
        private set => SetProperty(ref _totalRent, value);
    }

    private decimal _totalSales;
    public decimal TotalSales
    {
        get => _totalSales;
        private set => SetProperty(ref _totalSales, value);
    }

    private decimal _totalCommission;
    public decimal TotalCommission
    {
        get => _totalCommission;
        private set => SetProperty(ref _totalCommission, value);
    }

    private decimal _totalPayout;
    public decimal TotalPayout
    {
        get => _totalPayout;
        private set
        {
            if (SetProperty(ref _totalPayout, value))
                OnPropertyChanged(nameof(IsTotalPayoutNegative));
        }
    }

    public bool IsTotalPayoutNegative => TotalPayout < 0;

    // Sand, når den viste måned ikke er slut endnu - der kan stadig komme salg og opsigelser til.
    private bool _isPreliminary;
    public bool IsPreliminary
    {
        get => _isPreliminary;
        private set => SetProperty(ref _isPreliminary, value);
    }

    public MonthlyStatementViewModel(
        IRepository<Renter> renterRepository,
        IRentalRepository rentalRepository,
        ISaleRepository saleRepository,
        ISecureAreaService secureAreaService,
        IDialogService dialogService)
        : base(dialogService)
    {
        _renterRepository = renterRepository;
        _rentalRepository = rentalRepository;
        _saleRepository = saleRepository;

        Lock = new LockScreenViewModel(secureAreaService, dialogService);

        var today = DateTime.Today;
        Years = Enumerable.Range(today.Year - 4, 5).Reverse().ToList();

        // Forrige måned er standard: det er den seneste måned, der er afsluttet og kan afregnes.
        var previousMonth = today.AddMonths(-1);
        _selectedYear = previousMonth.Year;
        _selectedMonthIndex = previousMonth.Month - 1;

        // Vis opgørelsen med det samme, når kodeordet er accepteret
        Lock.PropertyChanged += (_, _) => Refresh();
    }

    // Genberegner opgørelsen for den valgte måned. Gør ingenting, så længe fanen er låst.
    public void Refresh()
    {
        if (!Lock.IsUnlocked || SelectedMonthIndex < 0)
            return;

        SafeExecute(ShowStatement);
    }

    private void ShowStatement()
    {
        var lines = MonthlyStatementCalculator.Calculate(
            SelectedYear,
            SelectedMonthIndex + 1,
            _renterRepository.GetAll(),
            _rentalRepository.GetAll(),
            _saleRepository.GetAll());

        Lines.Clear();
        foreach (var line in lines)
        {
            Lines.Add(line);
        }

        TotalRent = lines.Sum(l => l.Rent);
        TotalSales = lines.Sum(l => l.Sales);
        TotalCommission = lines.Sum(l => l.Commission);
        TotalPayout = lines.Sum(l => l.Payout);

        var nextMonthStart = new DateTime(SelectedYear, SelectedMonthIndex + 1, 1).AddMonths(1);
        IsPreliminary = DateTime.Today < nextMonthStart;
    }
}
