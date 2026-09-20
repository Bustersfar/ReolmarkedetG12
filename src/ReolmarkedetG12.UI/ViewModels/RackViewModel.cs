using System.Collections.ObjectModel;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class RackViewModel : ViewModelBase
{
    private readonly IRepository<Rack> _rackRepository;

    public ObservableCollection<Rack> Racks { get; } = new();

    public RackViewModel(IRepository<Rack> rackRepository)
    {
        _rackRepository = rackRepository;

        foreach (Rack rack in _rackRepository.GetAll())
        {
            Racks.Add(rack);
        }
    }
}