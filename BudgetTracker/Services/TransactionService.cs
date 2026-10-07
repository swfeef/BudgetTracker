using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Services;

public class TransactionService
{
    private readonly TransactionRepository _transactionRepository = new();
    private readonly CategoryRepository _categoryRepository = new();

    public TransactionService()
    {
        DatabaseHelper.Initialize();
    }

    public IReadOnlyList<Transaction> GetTransactions(DateTime? from = null, DateTime? to = null)
        => _transactionRepository.GetAll(from, to);

    public IReadOnlyList<Category> GetCategories()
        => _categoryRepository.GetAll();

    public void Save(Transaction transaction)
    {
        if (transaction.Amount <= 0)
            throw new ArgumentException("Сумма должна быть больше нуля.");

        if (_categoryRepository.GetById(transaction.CategoryId) is null)
            throw new ArgumentException("Выберите существующую категорию.");

        transaction.Note = (transaction.Note ?? string.Empty).Trim();

        if (transaction.Id == 0)
            transaction.Id = _transactionRepository.Add(transaction);
        else
            _transactionRepository.Update(transaction);
    }

    public void Delete(int id)
    {
        if (id > 0)
            _transactionRepository.Delete(id);
    }
}
