using System.Collections.ObjectModel;
using System.Globalization;
using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.ViewModels;

public class TransactionEditViewModel : ViewModelBase
{
    private readonly TransactionService _transactionService;
    private readonly int _transactionId;

    private string _amountText = string.Empty;
    private DateTime? _date = DateTime.Today;
    private Category? _selectedCategory;
    private string _note = string.Empty;
    private string _errorMessage = string.Empty;

    public ObservableCollection<Category> Categories { get; }

    public string WindowTitle => _transactionId == 0 ? "Новая операция" : "Редактирование операции";

    public string AmountText
    {
        get => _amountText;
        set => SetField(ref _amountText, value);
    }

    public DateTime? Date
    {
        get => _date;
        set => SetField(ref _date, value);
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    public string Note
    {
        get => _note;
        set => SetField(ref _note, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event Action? Saved;
    public event Action? Cancelled;

    public TransactionEditViewModel(TransactionService transactionService, Transaction? transaction = null)
    {
        _transactionService = transactionService;
        _transactionId = transaction?.Id ?? 0;
        Categories = new ObservableCollection<Category>(_transactionService.GetCategories());

        if (transaction is not null)
        {
            AmountText = transaction.Amount.ToString("0.##", CultureInfo.CurrentCulture);
            Date = transaction.Date;
            Note = transaction.Note;
            SelectedCategory = Categories.FirstOrDefault(x => x.Id == transaction.CategoryId);
        }
        else
        {
            SelectedCategory = Categories.FirstOrDefault();
        }

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => Cancelled?.Invoke());
    }

    private void Save()
    {
        ErrorMessage = string.Empty;

        if (!TryParseAmount(AmountText, out var amount) || amount <= 0)
        {
            ErrorMessage = "Введите корректную сумму больше нуля.";
            return;
        }

        if (Date is null)
        {
            ErrorMessage = "Выберите дату.";
            return;
        }

        if (SelectedCategory is null)
        {
            ErrorMessage = "Выберите категорию.";
            return;
        }

        try
        {
            _transactionService.Save(new Transaction
            {
                Id = _transactionId,
                Amount = amount,
                Date = Date.Value,
                CategoryId = SelectedCategory.Id,
                Note = Note
            });

            Saved?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private static bool TryParseAmount(string text, out decimal amount)
    {
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out amount))
            return true;

        var normalized = text.Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }
}
