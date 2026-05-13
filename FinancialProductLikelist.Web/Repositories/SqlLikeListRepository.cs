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
            AddNVarChar(cmd, "@UserID", 20, userId);
            AddInt(cmd, "@ProductNo", productNo);
            AddInt(cmd, "@OrderQty", item.OrderQty);
            AddNVarChar(cmd, "@Account", 20, item.Account);
            AddDecimal(cmd, "@TotalFee", 18, 2, item.TotalFee);
            AddDecimal(cmd, "@TotalAmount", 18, 2, item.TotalAmount);
            AddNVarChar(cmd, "@Email", 256, item.Email);

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

    public IReadOnlyList<Product> GetProducts()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "SELECT No, ProductName, Price, FeeRate FROM Product ORDER BY ProductName";
        using var reader = cmd.ExecuteReader();
        var products = new List<Product>();
        while (reader.Read())
        {
            products.Add(new Product(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.GetDecimal(3)));
        }

        return products;
    }

    public Product? GetProductByNo(int productNo)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "SELECT No, ProductName, Price, FeeRate FROM Product WHERE No = @No";
        AddInt(cmd, "@No", productNo);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Product(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetDecimal(2),
            reader.GetDecimal(3));
    }

    public IReadOnlyList<LikeListItem> GetByUserId(string userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var cmd = CreateCommand(connection, transaction: null, "SP_LikeList_GetList");
        AddNVarChar(cmd, "@UserID", 20, userId);
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
            AddInt(cmd, "@SN", item.Sn);
            AddNVarChar(cmd, "@UserID", 20, userId);
            AddInt(cmd, "@ProductNo", productNo);
            AddInt(cmd, "@OrderQty", item.OrderQty);
            AddNVarChar(cmd, "@Account", 20, item.Account);
            AddDecimal(cmd, "@TotalFee", 18, 2, item.TotalFee);
            AddDecimal(cmd, "@TotalAmount", 18, 2, item.TotalAmount);
            AddNVarChar(cmd, "@Email", 256, item.Email);
            var affectedRows = cmd.ExecuteNonQuery();
            if (affectedRows == 0)
            {
                throw new InvalidOperationException("Like list record not found.");
            }
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

        // Stored procedures use SET NOCOUNT ON, so ExecuteNonQuery() is not a
        // reliable affected-row signal. Check existence before delete.
        using (var existsCmd = connection.CreateCommand())
        {
            existsCmd.CommandType = CommandType.Text;
            existsCmd.CommandText = "SELECT 1 FROM LikeList WHERE SN = @SN AND UserID = @UserID";
            AddInt(existsCmd, "@SN", sn);
            AddNVarChar(existsCmd, "@UserID", 20, userId);
            if (existsCmd.ExecuteScalar() is null)
            {
                return false;
            }
        }

        using var cmd = CreateCommand(connection, transaction: null, "SP_LikeList_Delete");
        AddInt(cmd, "@SN", sn);
        AddNVarChar(cmd, "@UserID", 20, userId);
        cmd.ExecuteNonQuery();
        return true;
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
        AddNVarChar(upsertProduct, "@ProductName", 200, item.ProductName);
        AddDecimal(upsertProduct, "@Price", 18, 2, item.Price);
        AddDecimal(upsertProduct, "@FeeRate", 9, 4, item.FeeRate);
        return Convert.ToInt32(upsertProduct.ExecuteScalar());
    }

    private static void AddNVarChar(SqlCommand cmd, string name, int size, string value)
    {
        cmd.Parameters.Add(name, SqlDbType.NVarChar, size).Value = value;
    }

    private static void AddInt(SqlCommand cmd, string name, int value)
    {
        cmd.Parameters.Add(name, SqlDbType.Int).Value = value;
    }

    private static void AddDecimal(SqlCommand cmd, string name, byte precision, byte scale, decimal value)
    {
        var param = cmd.Parameters.Add(name, SqlDbType.Decimal);
        param.Precision = precision;
        param.Scale = scale;
        param.Value = value;
    }
}
