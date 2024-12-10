using Authentication_Application.Models;
using Authentication_Application.ViewModels;
using System.Linq.Expressions;

namespace Authentication_Application.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUser(RegisterViewModel registerRequest);
        Task<bool> AuthenticateUser(LoginViewModel loginRequest);
        Task<User> GetUser(Expression<Func<User, bool>> expression);
        Task<(string secretKey, string qrCodeUrl)> GenerateTwoFactorInfo(string username);
        Task<bool> EnableAutheticator(EnableAuthenticatorViewModel model);
        Task<bool> DisableAutheticator(string username);
        Task<bool> VerifyTwoFactorAuthentication(TwoStepLoginViewModel model);
    }
}
