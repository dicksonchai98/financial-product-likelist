using FinancialProductLikelist.Models;
using FinancialProductLikelist.Services;

namespace FinancialProductLikelist.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public void Register_Throws_WhenEmailAlreadyExists()
    {
        var repository = new FakeUserRepository();
        var hasher = new PasswordHasher();
        var service = new AuthService(repository, hasher);

        service.Register(new RegisterInput("Name", "u1@mail.com", "Pass#1"));

        Assert.Throws<InvalidOperationException>(() =>
            service.Register(new RegisterInput("Name2", "u1@mail.com", "Pass#2")));
    }

    [Fact]
    public void Login_ReturnsUser_WhenPasswordMatches()
    {
        var repository = new FakeUserRepository();
        var hasher = new PasswordHasher();
        var service = new AuthService(repository, hasher);
        service.Register(new RegisterInput("Name", "u1@mail.com", "Pass#1"));

        var loggedIn = service.Login("u1@mail.com", "Pass#1");

        Assert.NotNull(loggedIn);
        Assert.False(string.IsNullOrWhiteSpace(loggedIn.UserID));
    }

    [Fact]
    public void Login_ReturnsNull_WhenPasswordDoesNotMatch()
    {
        var repository = new FakeUserRepository();
        var hasher = new PasswordHasher();
        var service = new AuthService(repository, hasher);
        service.Register(new RegisterInput("Name", "u1@mail.com", "Pass#1"));

        var loggedIn = service.Login("u1@mail.com", "WrongPassword");

        Assert.Null(loggedIn);
    }

    [Theory]
    [InlineData("/LikeList", true)]
    [InlineData("https://evil.example", false)]
    public void IsSafeReturnUrl_ValidatesLocalOnly(string returnUrl, bool expected)
    {
        var actual = ReturnUrlGuard.IsSafeLocal(returnUrl);
        Assert.Equal(expected, actual);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<string, UserAuthRecord> _storage = new();

        public UserAuthRecord? GetAuthByEmail(string email)
            => _storage.Values.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        public bool UserIdExists(string userId)
            => _storage.ContainsKey(userId);

        public bool EmailExists(string email)
            => _storage.Values.Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        public void Register(UserAuthRecord userAuthRecord)
            => _storage[userAuthRecord.UserID] = userAuthRecord;
    }
}
