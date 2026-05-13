using System.Data;
using FinancialProductLikelist.Models;
using FinancialProductLikelist.Services;
using Microsoft.Data.SqlClient;

namespace FinancialProductLikelist.Repositories;

public sealed class SqlLikeListRepository : ILikeListRepository
{
    private readonly string _connectionString;

    public SqlLikeListRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: DefaultConnection.");
    }

    public LikeListItem Create(string userId, LikeListItem item)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productNo = EnsureProduct(connection, transaction, item);
            using var cmd = CreateCommand(connection, transaction, "SP_LikeList_Create");
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@ProductNo", productNo);
            cmd.Parameters.AddWithValue("@OrderQty", item.OrderQty);
            cmd.Parameters.AddWithValue("@Account", item.Account);
            cmd.Parameters.AddWithValue("@TotalFee", item.TotalFee);
            cmd.Parameters.AddWithValue("@TotalAmount", item.TotalAmount);
            cmd.Parameters.AddWithValue("@Email", item.Email);

            var newSn = Convert.ToInt32(cmd.ExecuteScalar());
            transaction.Commit();
            return item with { Sn = newSn, UserId = userId, ProductNo = productNo };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public IReadOnlyList<LikeListItem> GetByUserId(string userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = CreateCommand(connection, transaction: null, "SP_LikeList_GetList");
        cmd.Parameters.AddWithValue("@UserID", userId);
        using var reader = cmd.ExecuteReader();
        var results = new List<LikeListItem>();
        while (reader.Read())
        {
            results.Add(ReadLikeListItem(reader));
        }

        return results;
    }

    public LikeListItem? GetById(string userId, int sn)
    {
        return GetByUserId(userId).FirstOrDefault(x => x.Sn == sn);
    }

    public LikeListItem Update(string userId, LikeListItem item)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productNo = EnsureProduct(connection, transaction, item);
            using var cmd = CreateCommand(connection, transaction, "SP_LikeList_Update");
            cmd.Parameters.AddWithValue("@SN", item.Sn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@ProductNo", productNo);
            cmd.Parameters.AddWithValue("@OrderQty", item.OrderQty);
            cmd.Parameters.AddWithValue("@Account", item.Account);
            cmd.Parameters.AddWithValue("@TotalFee", item.TotalFee);
            cmd.Parameters.AddWithValue("@TotalAmount", item.TotalAmount);
            cmd.Parameters.AddWithValue("@Email", item.Email);
            cmd.ExecuteNonQuery();
            transaction.Commit();
            return item with { UserId = userId, ProductNo = productNo };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public bool Delete(string userId, int sn)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = CreateCommand(connection, transaction: null, "SP_LikeList_Delete");
        cmd.Parameters.AddWithValue("@SN", sn);
        cmd.Parameters.AddWithValue("@UserID", userId);
        return cmd.ExecuteNonQuery() > 0;
    }

    private static SqlCommand CreateCommand(SqlConnection connection, SqlTransaction? transaction, string storedProcedure)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = storedProcedure;
        return command;
    }

    private static LikeListItem ReadLikeListItem(SqlDataReader reader)
    {
        return new LikeListItem
        {
            Sn = reader.GetInt32(reader.GetOrdinal("SN")),
            UserId = reader.GetString(reader.GetOrdinal("UserID")),
            ProductNo = reader.GetInt32(reader.GetOrdinal("ProductNo")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            FeeRate = reader.GetDecimal(reader.GetOrdinal("FeeRate")),
            Account = reader.GetString(reader.GetOrdinal("Account")),
            OrderQty = reader.GetInt32(reader.GetOrdinal("OrderQty")),
            TotalFee = reader.GetDecimal(reader.GetOrdinal("TotalFee")),
            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
            Email = reader.GetString(reader.GetOrdinal("Email"))
        };
    }

    private static int EnsureProduct(SqlConnection connection, SqlTransaction transaction, LikeListItem item)
    {
        using var upsertProduct = CreateCommand(connection, transaction, "SP_Product_Upsert");
        upsertProduct.Parameters.AddWithValue("@ProductName", item.ProductName);
        upsertProduct.Parameters.AddWithValue("@Price", item.Price);
        upsertProduct.Parameters.AddWithValue("@FeeRate", item.FeeRate);
        return Convert.ToInt32(upsertProduct.ExecuteScalar());
    }
}
