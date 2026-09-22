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
    private readonly ObservableCollection<RackDisplayItem> _selectedRacks = new();

    public ObservableCollection<RackDisplayItem> Racks { get; } = new();

    // Venstre søjle: reol 13 (øverst) ned til reol 1 (nederst)
    public ObservableCollection<RackDisplayItem> LeftColumnRacks { get; } = new();

    // Klynger, samme rækkefølge og gruppering som plantegningen
    public ObservableCollection<RackDisplayItem> Cluster_14_18 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_19_24 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_25_38 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_39_52 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_53_66 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_67_76 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_77_78 { get; } = new();
    public ObservableCollection<RackDisplayItem> Cluster_79_80 { get; } = new();

    // Alle reoler brugeren aktuelt har markeret (understøtter flervalg)
    public ObservableCollection<RackDisplayItem> SelectedRacks => _selectedRacks;

    // Reoler den aktuelt valgte kunde allerede lejer. Tom indtil Renter/Rental-data er koblet på.
    public ObservableCollection<RackDisplayItem> RenterRacks { get; } = new();

    public ICommand SelectRackCommand { get; }

    public RackViewModel(IRepository<Rack> rackRepository)
    {
        _rackRepository = rackRepository;

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