using System;
using ReolmarkedetG12.UI.MVVM;

namespace ReolmarkedetG12.UI.ViewModels;

public class RackHistoryDisplayItem : ViewModelBase
{
    public int RentalId { get; set; }
    public int RackNumber { get; set; }
    public string RenterName { get; set; } = string.Empty;
    public string RenterContact { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlyRent { get; set; }

    public string PeriodText => $"{StartDate:dd-MM-yyyy} – {(EndDate.HasValue ? EndDate.Value.ToString("dd-MM-yyyy") : "Aktiv")}";
}