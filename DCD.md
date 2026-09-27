```mermaid
classDiagram

%%======================
%% MODELS
%%======================
namespace Core.Models{
class Renter{
+ int RenterId
+ string FirstName
+ string LastName
+ string Email
+ string Phone
+ string Address
+ string City
+ int PostalCode
}

class Rental{
+ int RentalId
+ int RackId
+ int RenterId
+ DateTime StartDate
+ DateTime EndDate
+ decimal MonthlyRent
}

class Rack{
+ int RackId
+ int Number
+ RackStatus Status
}

class RentalPriceTier{
+ int TierId
+ int MinRacks
+ int MaxRacks
+ decimal PricePerRack
}

class RackStatus{
<<enumeration>>
	Available
	Rented
	Terminated
}
}

%%======================
%% REPOSITORIES
%%======================
namespace Core.Repositories{

class IRepository~T~{
<<interface>>
+ GetAll()
+ GetById(int id)
+ Add(T entity)
+ Update(T entity)
+ Delete(int id)
}

class IRentalPriceTierRepository{
<<interface>>
+ GetAll(RentalPriceTier)
}

class RenterRepository
class RentalRepository
class RackRepository
class RentalPriceTierRepository

}


%%======================
%% Services
%%======================
namespace Core.Services{
class RentalPriceCalculator{
+ decimal CalculateMonthlyRent(int numberOfRacks, IEnumerable<RentalPriceTier> tiers)
}
}

%%======================
%% MVVM
%%======================
namespace UI.MVVM{

class ViewModelBase{
+ void OnPropertyChanged(string propertyName)
+ void SetProperty<T>(ref T field, T value, string propertyName)
}

class RelayCommand{
+ void Execute(object parameter)
+ bool CanExecute(object parameter)
+ CanExecuteChanged event
}
}

%%======================
%% SERVICES
%%======================
namespace UI.Services{
class IDialogService{
<<interface>>
+ void ShowError(string message)
+ void ShowInfo(string message)
+ bool Confirm(string message)
}

class MessageBoxDialogService{
+ void ShowError(string message)
+ void ShowInfo(string message)
+ bool Confirm(string message)
}
}

%%======================
VIEWMODELS
%%======================
namespace UI.ViewModels{
class MainViewModel

class RackViewModel

class RenterViewModel


}



Renter "1" -- "0..*" Rental : rents
Rack "1" -- "0..*" Rental : contains
Rack -- RackStatus : 
```