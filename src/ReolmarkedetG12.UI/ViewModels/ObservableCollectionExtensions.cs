using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ReolmarkedetG12.UI.ViewModels;

public static class ObservableCollectionExtensions
{
    public static void RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
    {
        foreach (var item in collection.Where(predicate).ToList())
            collection.Remove(item);
    }
}