using System.Collections.ObjectModel;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class ShelfViewModel : ViewModelBase
{
    private readonly IRepository<Shelf> _shelfRepository;

    public ObservableCollection<Shelf> Shelves { get; } = new();

    public ShelfViewModel(IRepository<Shelf> shelfRepository)
    {
        _shelfRepository = shelfRepository;

        foreach (Shelf shelf in _shelfRepository.GetAll())
        {
            Shelves.Add(shelf);
        }
    }
}
