using System.Windows.Controls;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.UI.Views;

/// <summary>
/// Interaction logic for RackView.xaml
/// </summary>
public partial class RackView : UserControl
{
    public RackView()
    {
        InitializeComponent();
    }

    // Klik på en reol-række i venstre tabel: send den valgte reol til
    // RackViewModel.SelectRackCommand, som allerede indeholder al logikken
    // (markering, hent lejer, hent historik osv.) og er dækket af tests.
    private void RacksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 &&
            DataContext is RackViewModel viewModel &&
            e.AddedItems[0] is RackDisplayItem selectedRack)
        {
            viewModel.SelectRackCommand.Execute(selectedRack);

            // Nulstil DataGrid'ens egen markering, så et klik på den SAMME reol
            // igen også udløser SelectionChanged (ellers kan man ikke afmarkere
            // en reol ved at klikke på den en gang til).
            if (sender is DataGrid dataGrid)
            {
                dataGrid.SelectedItem = null;
            }
        }
    }

    // Klik på en lejer i søgeresultat-listen: send lejeren til SelectRenterCommand.
    private void SearchResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 &&
            DataContext is RackViewModel viewModel &&
            e.AddedItems[0] is Renter selectedRenter)
        {
            viewModel.SelectRenterCommand.Execute(selectedRenter);

            // Ryd markeringen i listen, så samme lejer kan vælges igen senere.
            if (sender is ListBox listBox)
            {
                listBox.SelectedItem = null;
            }
        }
    }
}