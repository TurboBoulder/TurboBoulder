using TurboBoulder.Data;

namespace TurboBoulder.Services
{
    public interface ITwoFactorAuthenticationService
    {
        bool SendVerificationCodeAsync(User user);
        bool ConfirmVerificationCodeAsync(User user, string code);
    }
}
