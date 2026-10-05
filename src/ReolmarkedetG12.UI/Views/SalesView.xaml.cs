using System;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Threading;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.UI.Views;

/// <summary>
/// Interaction logic for SalesView.xaml
/// </summary>
public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();

        DataContextChanged += SalesView_DataContextChanged;
    }


    private void SalesView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SalesViewModel oldVm)
        {
            oldVm.CartItems.CollectionChanged -= CartItems_CollectionChanged;
        }

        if (e.NewValue is SalesViewModel newVm)
        {
            newVm.CartItems.CollectionChanged += CartItems_CollectionChanged;
        }
    }

    private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Når der tilføjes en vare (eller kurven tømmes), flyt fokus tilbage til Reolnummer
        if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Reset)
        {
            
        }
    }
}