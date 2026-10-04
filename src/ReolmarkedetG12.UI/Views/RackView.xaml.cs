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

    // Reol-plantegningen (venstre side) bruger nu knapper bundet direkte til
    // RackViewModel.SelectRackCommand via Command/CommandParameter i XAML,
    // så der er ikke længere brug for en SelectionChanged-handler for reoler.

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