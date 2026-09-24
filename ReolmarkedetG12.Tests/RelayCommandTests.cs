using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class RelayCommandTests
{
    [TestMethod]
    public void Execute_RunsTheAction()
    {
        // Arrange
        bool wasCalled = false;
        var command = new RelayCommand(_ => wasCalled = true);

        // Act
        command.Execute(null);

        // Assert
        Assert.IsTrue(wasCalled);
    }

    [TestMethod]
    public void CanExecute_NoCondition_ReturnsTrue()
    {
        var command = new RelayCommand(_ => { });

        Assert.IsTrue(command.CanExecute(null));
    }

    [TestMethod]
    public void CanExecute_ConditionIsFalse_ReturnsFalse()
    {
        var command = new RelayCommand(_ => { }, _ => false);

        Assert.IsFalse(command.CanExecute(null));
    }
}