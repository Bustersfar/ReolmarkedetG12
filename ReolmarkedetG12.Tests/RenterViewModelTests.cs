using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Tests.Fakes;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class RenterViewModelTests
{
    [TestMethod]
    public void Constructor_RentersInRepository_LoadsThemIntoList()
    {
        // Arrange: to lejere i den falske database
        var repo = new FakeRenterRepository();
        repo.Add(new Renter { FirstName = "Anna", LastName = "Andersen", Address = "Vej 1", PostalCode = 4200, City = "Slagelse" });
        repo.Add(new Renter { FirstName = "Bo", LastName = "Bertelsen", Address = "Vej 2", PostalCode = 4200, City = "Slagelse" });

        // Act
        var viewModel = new RenterViewModel(repo, new FakeDialogService());

        // Assert
        Assert.HasCount(2, viewModel.Renters);
    }

    [TestMethod]
    public void SaveCommand_NewRenter_AddsRenterToRepository()
    {
        // Arrange: udfyld formularen
        var repo = new FakeRenterRepository();
        var viewModel = new RenterViewModel(repo, new FakeDialogService());
        viewModel.FirstName = "Anna";
        viewModel.LastName = "Andersen";
        viewModel.Address = "Testvej 1";
        viewModel.PostalCode = 4200;
        viewModel.City = "Slagelse";

        // Act: tryk Gem
        viewModel.SaveCommand.Execute(null);

        // Assert
        Assert.HasCount(1, repo.Renters);
        Assert.AreEqual("Anna", repo.Renters[0].FirstName);
    }

    [TestMethod]
    public void SaveCommand_MissingFirstName_ShowsErrorAndSavesNothing()
    {
        // Arrange: fornavn mangler
        var repo = new FakeRenterRepository();
        var dialog = new FakeDialogService();
        var viewModel = new RenterViewModel(repo, dialog);
        viewModel.LastName = "Andersen";
        viewModel.Address = "Testvej 1";
        viewModel.PostalCode = 4200;
        viewModel.City = "Slagelse";

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        Assert.IsNotNull(dialog.LastError);
        Assert.HasCount(0, repo.Renters);
    }

    [TestMethod]
    public void SaveCommand_DatabaseDown_ShowsErrorInsteadOfCrashing()
    {
        // Arrange: databasen er fin ved opstart, men går ned bagefter
        var repo = new FakeRenterRepository();
        var dialog = new FakeDialogService();
        var viewModel = new RenterViewModel(repo, dialog);
        viewModel.FirstName = "Anna";
        viewModel.LastName = "Andersen";
        viewModel.Address = "Testvej 1";
        viewModel.PostalCode = 4200;
        viewModel.City = "Slagelse";
        repo.SimulateDatabaseDown = true;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        Assert.IsNotNull(dialog.LastError);
        Assert.Contains("Kunne ikke forbinde", dialog.LastError);
    }

    [TestMethod]
    public void Constructor_DatabaseDown_ShowsErrorInsteadOfCrashing()
    {
        // Arrange: databasen er nede fra start
        var repo = new FakeRenterRepository { SimulateDatabaseDown = true };
        var dialog = new FakeDialogService();

        // Act
        var viewModel = new RenterViewModel(repo, dialog);

        // Assert
        Assert.IsNotNull(dialog.LastError);
        Assert.HasCount(0, viewModel.Renters);
    }
    [TestMethod]
    public void DeleteCommand_UserConfirms_RemovesRenter()
    {
        // Arrange: én lejer, og brugeren svarer Ja
        var repo = new FakeRenterRepository();
        repo.Add(new Renter { FirstName = "Anna", LastName = "Andersen", Address = "Vej 1", PostalCode = 4200, City = "Slagelse" });
        var dialog = new FakeDialogService { ConfirmResult = true };
        var viewModel = new RenterViewModel(repo, dialog);
        viewModel.SelectedRenter = viewModel.Renters[0];

        // Act
        viewModel.DeleteCommand.Execute(null);

        // Assert
        Assert.HasCount(0, repo.Renters);
    }

    [TestMethod]
    public void DeleteCommand_UserDeclines_KeepsRenter()
    {
        // Arrange: én lejer, og brugeren svarer Nej
        var repo = new FakeRenterRepository();
        repo.Add(new Renter { FirstName = "Anna", LastName = "Andersen", Address = "Vej 1", PostalCode = 4200, City = "Slagelse" });
        var dialog = new FakeDialogService { ConfirmResult = false };
        var viewModel = new RenterViewModel(repo, dialog);
        viewModel.SelectedRenter = viewModel.Renters[0];

        // Act
        viewModel.DeleteCommand.Execute(null);

        // Assert
        Assert.HasCount(1, repo.Renters);
    }

    [TestMethod]
    public void SaveCommand_ExistingRenter_UpdatesInsteadOfAddingNew()
    {
        // Arrange: vælg en eksisterende lejer, og ret byen
        var repo = new FakeRenterRepository();
        repo.Add(new Renter { FirstName = "Anna", LastName = "Andersen", Address = "Vej 1", PostalCode = 4200, City = "Slagelse" });
        var viewModel = new RenterViewModel(repo, new FakeDialogService());
        viewModel.SelectedRenter = viewModel.Renters[0];
        viewModel.City = "Korsør";

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert: der er stadig kun én lejer, og byen er ændret
        Assert.HasCount(1, repo.Renters);
        Assert.AreEqual("Korsør", repo.Renters[0].City);
    }
}