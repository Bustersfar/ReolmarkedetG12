using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.UI.Services;

namespace ReolmarkedetG12.UI.MVVM;

public class ViewModelBase : INotifyPropertyChanged
{
    // Kun sat for ViewModels, der rent faktisk kalder databasen og derfor bruger
    // SafeExecute nedenfor (RackViewModel, RenterViewModel, SalesViewModel,
    // SearchSalesViewModel). Rene display-items (f.eks. RackDisplayItem) har
    // ikke brug for den og bruger i stedet den parameterløse konstruktør.
    private readonly IDialogService? _dialogService;

    protected ViewModelBase()
    {
    }

    protected ViewModelBase(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // Fælles fejlhåndtering for ViewModels, der kalder repositories/databasen.
    // Denne kode lå tidligere som fire næsten identiske kopier i RackViewModel,
    // RenterViewModel, SalesViewModel og SearchSalesViewModel.
    protected void SafeExecute(Action action)
    {
        if (_dialogService == null)
        {
            throw new InvalidOperationException(
                "SafeExecute kræver en IDialogService. Brug ViewModelBase(dialogService)-konstruktøren i den afledte klasse.");
        }

        try
        {
            action();
        }
        catch (DatabaseConnectionException ex)
        {
            _dialogService.ShowError(
                $"{ex.Message}\n\nTeknisk besked: {ex.InnerException?.Message ?? "ukendt"}",
                "Forbindelsesfejl");
        }
        catch (SqlException ex)
        {
            _dialogService.ShowError(
                $"Der opstod en fejl i databasen.\n\nTeknisk besked: {ex.Message}",
                "Databasefejl");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, "Fejl");
        }
    }
}