using FinancialProductLikelist.Models;

namespace FinancialProductLikelist.Services;

public interface IUserRepository
{
    bool UserIdExists(string userId);
    bool EmailExists(string email);
    void Register(UserAuthRecord userAuthRecord);
    UserAuthRecord? GetAuthByEmail(string email);
}
