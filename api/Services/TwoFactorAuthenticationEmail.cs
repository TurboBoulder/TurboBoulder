using TurboBoulder.Data;

namespace TurboBoulder.Services
{
    public class TwoFactorAuthenticationEmail : ITwoFactorAuthentication
    {
        public bool ConfirmVerificationCodeAsync(User user, string code)
        {
            throw new NotImplementedException();
        }

        public bool SendVerificationCodeAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
