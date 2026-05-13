using FinancialProductLikelist.Models;

namespace FinancialProductLikelist.Services;

public interface ILikeListRepository
{
    LikeListItem Create(string userId, LikeListItem item);
    IReadOnlyList<LikeListItem> GetByUserId(string userId);
    LikeListItem? GetById(string userId, int sn);
    LikeListItem Update(string userId, LikeListItem item);
    bool Delete(string userId, int sn);
}
