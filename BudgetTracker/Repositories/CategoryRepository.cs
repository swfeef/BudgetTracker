using BudgetTracker.Models;

namespace BudgetTracker.Repositories;

public class CategoryRepository
{
    public IReadOnlyList<Category> GetAll()
    {
        var result = new List<Category>();

        using var connection = DatabaseHelper.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, IsIncome FROM Categories ORDER BY IsIncome DESC, Name;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                IsIncome = reader.GetInt32(2) == 1
            });
        }

        return result;
    }

    public Category? GetById(int id)
    {
        using var connection = DatabaseHelper.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, IsIncome FROM Categories WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return new Category
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            IsIncome = reader.GetInt32(2) == 1
        };
    }
}
