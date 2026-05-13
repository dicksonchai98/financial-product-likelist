using FinancialProductLikelist.Models;
using FinancialProductLikelist.Services;

namespace FinancialProductLikelist.Tests;

public sealed class LikeListServiceTests
{
    [Fact]
    public void CreateLikeListItem_RecomputesTotalAmountAndFee()
    {
        var repository = new FakeLikeListRepository();
        var service = new LikeListService(repository);

        var input = new LikeListItemInput(
            ProductName: "ETF-A",
            Price: 100m,
            FeeRate: 0.01m,
            Account: "1111999666",
            OrderQty: 3);

        var created = service.Create(userId: "A123", input);

        Assert.Equal(300m, created.TotalAmount);
        Assert.Equal(3m, created.TotalFee);
    }

    [Fact]
    public void GetByUserId_ReturnsOnlyTheUsersOwnRecords()
    {
        var repository = new FakeLikeListRepository();
        var service = new LikeListService(repository);

        service.Create("U1", new LikeListItemInput("Fund-A", 10m, 0.01m, "1", 2));
        service.Create("U2", new LikeListItemInput("Fund-B", 20m, 0.01m, "2", 2));

        var user1Records = service.GetByUserId("U1").ToList();

        Assert.Single(user1Records);
        Assert.Equal("U1", user1Records[0].UserId);
    }

    [Fact]
    public void Delete_RemovesLikeListOnly_AndKeepsProductMaster()
    {
        var repository = new FakeLikeListRepository();
        var service = new LikeListService(repository);

        var created = service.Create("U1", new LikeListItemInput("Fund-C", 30m, 0.01m, "3", 2));

        service.Delete("U1", created.Sn);

        Assert.Empty(service.GetByUserId("U1"));
        Assert.Contains(repository.Products, p => p.ProductName == "Fund-C");
    }

    private sealed class FakeLikeListRepository : ILikeListRepository
    {
        private int _sn;
        private int _productNo;
        private readonly List<LikeListItem> _items = [];

        public List<Product> Products { get; } = [];

        public IReadOnlyList<Product> GetProducts() => Products;

        public Product? GetProductByNo(int productNo)
            => Products.FirstOrDefault(x => x.No == productNo);

        public LikeListItem Create(string userId, LikeListItem item)
        {
            var product = Products.FirstOrDefault(x =>
                x.ProductName == item.ProductName &&
                x.Price == item.Price &&
                x.FeeRate == item.FeeRate);

            if (product is null)
            {
                product = new Product(++_productNo, item.ProductName, item.Price, item.FeeRate);
                Products.Add(product);
            }

            var created = item with
            {
                Sn = ++_sn,
                UserId = userId,
                ProductNo = product.No
            };

            _items.Add(created);
            return created;
        }

        public IReadOnlyList<LikeListItem> GetByUserId(string userId)
            => _items.Where(x => x.UserId == userId).ToList();

        public bool Delete(string userId, int sn)
        {
            var target = _items.FirstOrDefault(x => x.UserId == userId && x.Sn == sn);
            if (target is null)
            {
                return false;
            }

            _items.Remove(target);
            return true;
        }

        public LikeListItem? GetById(string userId, int sn)
            => _items.FirstOrDefault(x => x.UserId == userId && x.Sn == sn);

        public LikeListItem Update(string userId, LikeListItem item)
        {
            var current = _items.First(x => x.UserId == userId && x.Sn == item.Sn);
            _items.Remove(current);
            _items.Add(item);
            return item;
        }
    }
}
