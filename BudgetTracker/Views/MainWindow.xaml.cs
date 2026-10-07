using System.Windows;
using BudgetTracker.Models;
using BudgetTracker.Services;
using BudgetTracker.ViewModels;

namespace BudgetTracker.Views;

public partial class MainWindow : Window
{
    private readonly TransactionService _transactionService;
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _transactionService = new TransactionService();
        _viewModel = new MainViewModel(_transactionService, new ReportService());

        _viewModel.AddRequested += AddTransaction;
        _viewModel.EditRequested += EditTransaction;
        _viewModel.DeleteRequested += DeleteTransaction;

        DataContext = _viewModel;
        Loaded += (_, _) => _viewModel.Load();
    }

    private void AddTransaction()
    {
        OpenEditor(null);
    }

    private void EditTransaction(Transaction transaction)
    {
        OpenEditor(transaction);
    }

    private void OpenEditor(Transaction? transaction)
    {
        var editorViewModel = new TransactionEditViewModel(_transactionService, transaction);
        var window = new TransactionEditWindow(editorViewModel)
        {
            Owner = this
        };

        if (window.ShowDialog() == true)
            _viewModel.Load();
    }

    private void DeleteTransaction(Transaction transaction)
    {
        var result = MessageBox.Show(
            $"Удалить операцию «{transaction.CategoryName}» на сумму {transaction.Amount:N2} ₽?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        _transactionService.Delete(transaction.Id);
        _viewModel.Load();
    }
}
