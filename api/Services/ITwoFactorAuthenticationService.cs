using IdaWebApplicationTemplate.Data;

namespace IdaWebApplicationTemplate.Services
{
    public interface ITwoFactorAuthenticationService
    {
        bool SendVerificationCodeAsync(User user);
        bool ConfirmVerificationCodeAsync(User user, string code);
    }
}
