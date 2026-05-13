using System.Data;
using FinancialProductLikelist.Models;
using FinancialProductLikelist.Services;
using Microsoft.Data.SqlClient;

namespace FinancialProductLikelist.Repositories;

public sealed class SqlUserRepository : IUserRepository
{
    private readonly string _connectionString;

    public SqlUserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: DefaultConnection.");
    }

    public bool UserIdExists(string userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = CreateCommand(connection, "SP_User_ExistsByUserId");
        AddNVarChar(cmd, "@UserID", 20, userId);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public bool EmailExists(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = CreateCommand(connection, "SP_User_ExistsByEmail");
        AddNVarChar(cmd, "@Email", 256, email);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public void Register(UserAuthRecord userAuthRecord)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = CreateCommand(connection, "SP_User_Register");
        AddNVarChar(cmd, "@UserID", 20, userAuthRecord.UserID);
        AddNVarChar(cmd, "@UserName", 100, userAuthRecord.UserName);
        AddNVarChar(cmd, "@Email", 256, userAuthRecord.Email);
        AddNVarChar(cmd, "@Account", 20, userAuthRecord.Account);
        AddNVarChar(cmd, "@PasswordHash", 256, userAuthRecord.PasswordHash);
        AddNVarChar(cmd, "@PasswordSalt", 256, userAuthRecord.PasswordSalt);
        cmd.ExecuteNonQuery();
    }

    public UserAuthRecord? GetAuthByEmail(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = CreateCommand(connection, "SP_User_Login");
        AddNVarChar(cmd, "@Email", 256, email);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new UserAuthRecord
        {
            UserID = reader.GetString(reader.GetOrdinal("UserID")),
            UserName = reader.GetString(reader.GetOrdinal("UserName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Account = reader.GetString(reader.GetOrdinal("Account")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            PasswordSalt = reader.GetString(reader.GetOrdinal("PasswordSalt"))
        };
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string storedProcedure)
    {
        var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = storedProcedure;
        return command;
    }

    private static void AddNVarChar(SqlCommand cmd, string name, int size, string value)
    {
        cmd.Parameters.Add(name, SqlDbType.NVarChar, size).Value = value;
    }
}
