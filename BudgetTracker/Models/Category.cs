namespace BudgetTracker.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsIncome { get; set; }

    public string TypeDisplay => IsIncome ? "Доход" : "Расход";

    public override string ToString() => $"{Name} ({TypeDisplay})";
}
