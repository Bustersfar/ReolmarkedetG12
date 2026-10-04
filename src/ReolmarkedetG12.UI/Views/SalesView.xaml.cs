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

        Loaded += SalesView_Loaded;
        DataContextChanged += SalesView_DataContextChanged;
    }

    private void SalesView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Sæt markøren i Reolnummer med det samme, når fanen åbnes
        RackNumberBox.Focus();
    }

    private void SalesView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SalesViewModel oldVm)
        {
            oldVm.CurrentSaleItems.CollectionChanged -= CurrentSaleItems_CollectionChanged;
        }

        if (e.NewValue is SalesViewModel newVm)
        {
            newVm.CurrentSaleItems.CollectionChanged += CurrentSaleItems_CollectionChanged;
        }
    }

    private void CurrentSaleItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Når der tilføjes en vare (eller kurven tømmes), flyt fokus tilbage til Reolnummer
        if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Reset)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                RackNumberBox.Focus();
                RackNumberBox.SelectAll();
            }));
        }
    }
}