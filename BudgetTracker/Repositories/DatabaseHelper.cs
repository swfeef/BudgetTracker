using Microsoft.Data.Sqlite;

namespace BudgetTracker.Repositories;

public static class DatabaseHelper
{
    private static readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BudgetTracker");

    public static readonly string DatabasePath = Path.Combine(AppFolder, "budget.db");

    public static SqliteConnection CreateConnection()
    {
        Directory.CreateDirectory(AppFolder);
        return new SqliteConnection($"Data Source={DatabasePath};Foreign Keys=True");
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(AppFolder);

        using var connection = CreateConnection();
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    IsIncome INTEGER NOT NULL CHECK (IsIncome IN (0, 1))
                );

                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Amount REAL NOT NULL CHECK (Amount > 0),
                    Date TEXT NOT NULL,
                    Note TEXT NOT NULL DEFAULT '',
                    CategoryId INTEGER NOT NULL,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
                );
                """;
            command.ExecuteNonQuery();
        }

        SeedCategories(connection);
    }

    private static void SeedCategories(SqliteConnection connection)
    {
        var categories = new (string Name, bool IsIncome)[]
        {
            ("Зарплата", true),
            ("Подработка", true),
            ("Подарки", true),
            ("Продукты", false),
            ("Транспорт", false),
            ("Жильё", false),
            ("Развлечения", false),
            ("Здоровье", false),
            ("Покупки", false),
            ("Прочее", false)
        };

        foreach (var category in categories)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT OR IGNORE INTO Categories (Name, IsIncome) VALUES ($name, $isIncome);";
            command.Parameters.AddWithValue("$name", category.Name);
            command.Parameters.AddWithValue("$isIncome", category.IsIncome ? 1 : 0);
            command.ExecuteNonQuery();
        }
    }
}
