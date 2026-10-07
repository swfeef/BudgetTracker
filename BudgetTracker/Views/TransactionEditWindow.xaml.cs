using System.Windows;
using BudgetTracker.ViewModels;

namespace BudgetTracker.Views;

public partial class TransactionEditWindow : Window
{
    public TransactionEditWindow(TransactionEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.Saved += () =>
        {
            DialogResult = true;
            Close();
        };

        viewModel.Cancelled += () =>
        {
            DialogResult = false;
            Close();
        };
    }
}
