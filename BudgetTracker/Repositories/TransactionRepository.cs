using BudgetTracker.Models;
using Microsoft.Data.Sqlite;

namespace BudgetTracker.Repositories;

public class TransactionRepository
{
    public IReadOnlyList<Transaction> GetAll(DateTime? from = null, DateTime? to = null)
    {
        var result = new List<Transaction>();

        using var connection = DatabaseHelper.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.Id, t.Amount, t.Date, t.Note, t.CategoryId, c.Name, c.IsIncome
            FROM Transactions t
            INNER JOIN Categories c ON c.Id = t.CategoryId
            WHERE ($from IS NULL OR date(t.Date) >= date($from))
              AND ($to IS NULL OR date(t.Date) <= date($to))
            ORDER BY datetime(t.Date) DESC, t.Id DESC;
            """;

        command.Parameters.AddWithValue("$from", from is null ? DBNull.Value : from.Value.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$to", to is null ? DBNull.Value : to.Value.ToString("yyyy-MM-dd"));

        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(ReadTransaction(reader));

        return result;
    }

    public int Add(Transaction transaction)
    {
        using var connection = DatabaseHelper.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Transactions (Amount, Date, Note, CategoryId)
            VALUES ($amount, $date, $note, $categoryId);
            SELECT last_insert_rowid();
            """;
        AddParameters(command, transaction);

        return Convert.ToInt32((long)(command.ExecuteScalar() ?? 0L));
    }

    public void Update(Transaction transaction)
    {
        using var connection = DatabaseHelper.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Transactions
            SET Amount = $amount,
                Date = $date,
                Note = $note,
                CategoryId = $categoryId
            WHERE Id = $id;
            """;
        AddParameters(command, transaction);
        command.Parameters.AddWithValue("$id", transaction.Id);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = DatabaseHelper.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Transactions WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static void AddParameters(SqliteCommand command, Transaction transaction)
    {
        command.Parameters.AddWithValue("$amount", transaction.Amount);
        command.Parameters.AddWithValue("$date", transaction.Date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$note", transaction.Note ?? string.Empty);
        command.Parameters.AddWithValue("$categoryId", transaction.CategoryId);
    }

    private static Transaction ReadTransaction(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Amount = reader.GetDecimal(1),
        Date = DateTime.Parse(reader.GetString(2)),
        Note = reader.GetString(3),
        CategoryId = reader.GetInt32(4),
        CategoryName = reader.GetString(5),
        IsIncome = reader.GetInt32(6) == 1
    };
}
