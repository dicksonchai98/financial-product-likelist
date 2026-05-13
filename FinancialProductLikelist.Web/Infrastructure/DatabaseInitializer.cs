using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace FinancialProductLikelist.Infrastructure;

public static class DatabaseInitializer
{
    public static void EnsureInitialized(IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        var appConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: DefaultConnection.");

        var appBuilder = new SqlConnectionStringBuilder(appConnectionString);
        var databaseName = appBuilder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("DefaultConnection must include a database name.");
        }

        var masterBuilder = new SqlConnectionStringBuilder(appConnectionString)
        {
            InitialCatalog = "master"
        };

        EnsureDatabaseExists(masterBuilder.ConnectionString, databaseName);
        EnsureSchemaInitialized(appConnectionString, databaseName);
    }

    private static void EnsureDatabaseExists(string masterConnectionString, string databaseName)
    {
        var escapedDatabaseName = databaseName.Replace("]", "]]", StringComparison.Ordinal);
        var createSql = $"""
                         IF DB_ID(N'{databaseName.Replace("'", "''", StringComparison.Ordinal)}') IS NULL
                         BEGIN
                             CREATE DATABASE [{escapedDatabaseName}];
                         END
                         """;

        using var masterConnection = new SqlConnection(masterConnectionString);
        masterConnection.Open();
        using var createCmd = new SqlCommand(createSql, masterConnection);
        createCmd.ExecuteNonQuery();
    }

    private static void EnsureSchemaInitialized(string appConnectionString, string databaseName)
    {
        using var connection = new SqlConnection(appConnectionString);
        connection.Open();

        if (HasUserTable(connection))
        {
            return;
        }

        var contentRoot = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "..", ".."));
        var dbDir = Path.Combine(solutionRoot, "FinancialProductLikelist.Web", "DB");
        var scripts = new[]
        {
            Path.Combine(dbDir, "DDL.sql"),
            Path.Combine(dbDir, "StoredProcedures.sql"),
            Path.Combine(dbDir, "DML.sql")
        };

        foreach (var scriptPath in scripts)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Missing database script: {scriptPath}");
            }

            var script = File.ReadAllText(scriptPath);
            foreach (var batch in SplitBatches(script))
            {
                using var cmd = new SqlCommand(batch, connection);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private static bool HasUserTable(SqlConnection connection)
    {
        const string sql = "SELECT CASE WHEN OBJECT_ID(N'[dbo].[User]', N'U') IS NOT NULL THEN 1 ELSE 0 END;";
        using var cmd = new SqlCommand(sql, connection);
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    private static IEnumerable<string> SplitBatches(string script)
    {
        var batches = Regex.Split(script, @"^\s*GO(?:\s+--.*)?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        foreach (var batch in batches)
        {
            var trimmed = batch.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                yield return trimmed;
            }
        }
    }
}
