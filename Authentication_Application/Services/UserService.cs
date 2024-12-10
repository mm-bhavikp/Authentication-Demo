using Authentication_Application.DataEntities;
using Authentication_Application.Models;
using Authentication_Application.ViewModels;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using System.Linq.Expressions;
using System.Text;

namespace Authentication_Application.Services
{
    public class UserService : IUserService
    {
        private AppDBContext _dbContext;
        public UserService(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AuthenticateUser(LoginViewModel loginRequest)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(i => i.Username == loginRequest.Username);
                if (user != null && BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex) 
            {
                return false;
            }
        }

        public async Task<bool> RegisterUser(RegisterViewModel registerRequest)
        {
            try
            {
                bool isExist = await _dbContext.Users.AnyAsync(i => i.Username == registerRequest.Username);
                if (isExist)
                {
                    return false;   
                }

                User user = new User();
                user.Name = registerRequest.Name;
                user.Username = registerRequest.Username;
                user.Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
                user.IsTwoFactorEnabled = registerRequest.IsAuthenticate;

                var res = await _dbContext.Users.AddAsync(user);

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch(Exception ex) 
            {
                return false;
            }
        }

        public async Task<User> GetUser(Expression<Func<User, bool>> expression)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(expression);
            return user;
        }

        public async Task<(string secretKey, string qrCodeUrl)> GenerateTwoFactorInfo(string username)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();

            var secretKeyBuilder = new StringBuilder(16);
            for (int i = 0; i < 16; i++)
            {
                secretKeyBuilder.Append(validChars[random.Next(validChars.Length)]);
            }
            var secretKey = secretKeyBuilder.ToString();

            var encodedUsername = Uri.EscapeDataString(username);
            var qrCodeUrl = $"otpauth://totp/{encodedUsername}?secret={secretKey}&issuer=TwoFactorAuthApp";

            return (secretKey, qrCodeUrl);
        }

        public async Task<bool> VerifyTwoFactorAuthentication(TwoStepLoginViewModel model)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user != null && !string.IsNullOrEmpty(user.SecretKey) && !string.IsNullOrEmpty(model.Code))
                {
                    var totp = new Totp(Base32Encoding.ToBytes(user.SecretKey));
                    var verifed = totp.VerifyTotp(model.Code, out _, new VerificationWindow(2, 2)); // Adjust window size as needed
                    return verifed;
                }
                return false;
            }
            catch (Exception ex)
            {

                return false;
            }

        }

        public async Task<bool> EnableAutheticator(EnableAuthenticatorViewModel model)
        {
            try
            {
                var topt = new Totp(Base32Encoding.ToBytes(model.SecretKey));
                var verified = topt.VerifyTotp(model.Code, out _, new VerificationWindow(2, 2));
                if (verified)
                {
                    var userDb = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == model.Username);
                    if (userDb != null)
                    {
                        userDb.SecretKey = model.SecretKey;
                        userDb.IsTwoFactorEnabled = true;
                        var res = _dbContext.Users.Update(userDb);
                        await _dbContext.SaveChangesAsync();
                        return true;
                    }
                }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DisableAutheticator(string username)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    //user.SecretKey = string.Empty;
                    user.IsTwoFactorEnabled = false;
                    var res = _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
