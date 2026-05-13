using FinancialProductLikelist.Models;

namespace FinancialProductLikelist.Services;

public sealed class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public void Register(RegisterInput input)
    {
        if (_userRepository.EmailExists(input.Email))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var userId = GenerateUserId();
        var account = GenerateAccountNo();
        var (hash, salt) = _passwordHasher.Hash(input.Password);
        var userAuth = new UserAuthRecord
        {
            UserID = userId,
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim(),
            Account = account,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _userRepository.Register(userAuth);
    }

    public UserAuthRecord? Login(string email, string password)
    {
        var record = _userRepository.GetAuthByEmail(email.Trim());
        if (record is null)
        {
            return null;
        }

        return _passwordHasher.Verify(password, record.PasswordHash, record.PasswordSalt)
            ? record
            : null;
    }

    private string GenerateUserId()
    {
        string userId;
        do
        {
            userId = $"U{Guid.NewGuid():N}"[..13].ToUpperInvariant();
        } while (_userRepository.UserIdExists(userId));

        return userId;
    }

    private static string GenerateAccountNo()
    {
        var digits = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        return digits.Length > 10 ? digits[^10..] : digits.PadLeft(10, '0');
    }
}
