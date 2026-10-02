using System.ComponentModel;

namespace ReolmarkedetG12.UI.Services;

// Fælles, simpel adgangsspærre for de "følsomme" faneblade (Søg/ret salg, månedsopgørelse, ...).
// Det er IKKE rigtig sikkerhed - bare en nem spærre mod at klikke forkert ind.
public interface ISecureAreaService : INotifyPropertyChanged
{
    bool IsUnlocked { get; }
    bool TryUnlock(string password);
}