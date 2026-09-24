using System.Globalization;
using System.Windows;
using ReolmarkedetG12.UI.Converters;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class EmptyToVisibilityConverterTests
{
    private readonly EmptyToVisibilityConverter _converter = new();

    // parameter er tom, medmindre testen selv angiver "Invert"
    private object Convert(object? value, string parameter = "") =>
        _converter.Convert(value, typeof(Visibility), parameter, CultureInfo.InvariantCulture);

    [TestMethod]
    public void Convert_Null_ReturnsVisible()
    {
        Assert.AreEqual(Visibility.Visible, Convert(null));
    }

    [TestMethod]
    public void Convert_EmptyString_ReturnsVisible()
    {
        Assert.AreEqual(Visibility.Visible, Convert(""));
    }

    [TestMethod]
    public void Convert_TextWithContent_ReturnsCollapsed()
    {
        Assert.AreEqual(Visibility.Collapsed, Convert("Anna"));
    }

    [TestMethod]
    public void Convert_EmptyList_ReturnsVisible()
    {
        Assert.AreEqual(Visibility.Visible, Convert(new List<int>()));
    }

    [TestMethod]
    public void Convert_ListWithItems_ReturnsCollapsed()
    {
        Assert.AreEqual(Visibility.Collapsed, Convert(new List<int> { 1 }));
    }

    [TestMethod]
    public void Convert_Zero_ReturnsVisible()
    {
        Assert.AreEqual(Visibility.Visible, Convert(0));
    }

    [TestMethod]
    public void Convert_NullWithInvertParameter_ReturnsCollapsed()
    {
        Assert.AreEqual(Visibility.Collapsed, Convert(null, "Invert"));
    }
}