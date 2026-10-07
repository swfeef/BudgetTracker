using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class ReportService
{
    public ReportSummary BuildSummary(IEnumerable<Transaction> transactions)
    {
        var list = transactions.ToList();
        var income = list.Where(x => x.IsIncome).Sum(x => x.Amount);
        var expense = list.Where(x => !x.IsIncome).Sum(x => x.Amount);

        return new ReportSummary
        {
            Income = income,
            Expense = expense,
            Balance = income - expense
        };
    }

    public IReadOnlyList<CategoryTotal> BuildCategoryTotals(IEnumerable<Transaction> transactions)
        => transactions
            .GroupBy(x => new { x.CategoryName, x.IsIncome })
            .Select(g => new CategoryTotal
            {
                CategoryName = g.Key.CategoryName,
                IsIncome = g.Key.IsIncome,
                Total = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();
}

public class ReportSummary
{
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Balance { get; init; }
}

public class CategoryTotal
{
    public string CategoryName { get; init; } = string.Empty;
    public bool IsIncome { get; init; }
    public decimal Total { get; init; }
}
