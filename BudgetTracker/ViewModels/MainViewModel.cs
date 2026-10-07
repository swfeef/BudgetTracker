using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly TransactionService _transactionService;
    private readonly ReportService _reportService;

    private Transaction? _selectedTransaction;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    private decimal _totalIncome;
    private decimal _totalExpense;
    private decimal _balance;

    public ObservableCollection<Transaction> Transactions { get; } = new();

    public Transaction? SelectedTransaction
    {
        get => _selectedTransaction;
        set
        {
            if (SetField(ref _selectedTransaction, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public DateTime? DateFrom
    {
        get => _dateFrom;
        set => SetField(ref _dateFrom, value);
    }

    public DateTime? DateTo
    {
        get => _dateTo;
        set => SetField(ref _dateTo, value);
    }

    public decimal TotalIncome
    {
        get => _totalIncome;
        private set => SetField(ref _totalIncome, value);
    }

    public decimal TotalExpense
    {
        get => _totalExpense;
        private set => SetField(ref _totalExpense, value);
    }

    public decimal Balance
    {
        get => _balance;
        private set => SetField(ref _balance, value);
    }

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ResetFilterCommand { get; }

    public event Action? AddRequested;
    public event Action<Transaction>? EditRequested;
    public event Action<Transaction>? DeleteRequested;

    public MainViewModel(TransactionService transactionService, ReportService reportService)
    {
        _transactionService = transactionService;
        _reportService = reportService;

        DateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        DateTo = DateTime.Today;

        AddCommand = new RelayCommand(() => AddRequested?.Invoke());
        EditCommand = new RelayCommand(
            () => { if (SelectedTransaction is not null) EditRequested?.Invoke(SelectedTransaction); },
            () => SelectedTransaction is not null);
        DeleteCommand = new RelayCommand(
            () => { if (SelectedTransaction is not null) DeleteRequested?.Invoke(SelectedTransaction); },
            () => SelectedTransaction is not null);
        RefreshCommand = new RelayCommand(Load);
        ResetFilterCommand = new RelayCommand(() =>
        {
            DateFrom = null;
            DateTo = null;
            Load();
        });
    }

    public void Load()
    {
        var data = _transactionService.GetTransactions(DateFrom, DateTo);

        Transactions.Clear();
        foreach (var item in data)
            Transactions.Add(item);

        var summary = _reportService.BuildSummary(data);
        TotalIncome = summary.Income;
        TotalExpense = summary.Expense;
        Balance = summary.Balance;
    }
}

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
