using TurboBoulder.Data;

namespace TurboBoulder.Services
{
    public interface ITwoFactorAuthentication
    {
        bool SendVerificationCodeAsync(User user);
        bool ConfirmVerificationCodeAsync(User user, string code);
    }
}