using IdaWebApplicationTemplate.Data;

namespace IdaWebApplicationTemplate.Services
{
    public interface ITwoFactorAuthentication
    {
        bool SendVerificationCodeAsync(User user);
        bool ConfirmVerificationCodeAsync(User user, string code);
    }
}