using System.Windows;
using System.Windows.Controls;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.UI.Views;

/// <summary>
/// Interaction logic for LockScreenView.xaml
/// </summary>
public partial class LockScreenView : UserControl
{
    public LockScreenView()
    {
        InitializeComponent();
    }

    private void PasswordInputBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LockScreenViewModel viewModel)
            viewModel.PasswordInput = PasswordInputBox.Password;
    }
}