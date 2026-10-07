namespace BudgetTracker.Models;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Note { get; set; } = string.Empty;
    public int CategoryId { get; set; }

    // Заполняются репозиторием при чтении JOIN-запросом.
    public string CategoryName { get; set; } = string.Empty;
    public bool IsIncome { get; set; }

    public string TypeDisplay => IsIncome ? "Доход" : "Расход";
}
