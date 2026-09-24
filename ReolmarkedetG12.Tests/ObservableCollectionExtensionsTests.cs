using System.Collections.ObjectModel;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class ObservableCollectionExtensionsTests
{
    [TestMethod]
    public void RemoveAll_RemovesOnlyMatchingItems()
    {
        // Arrange
        var collection = new ObservableCollection<int> { 1, 2, 3, 4 };

        // Act: fjern alle lige tal
        collection.RemoveAll(x => x % 2 == 0);

        // Assert
        CollectionAssert.AreEqual(new[] { 1, 3 }, collection.ToList());
    }
}