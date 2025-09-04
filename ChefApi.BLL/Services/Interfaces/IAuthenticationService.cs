using ChefApi.DAL.DTO.Request;
using ChefApi.DAL.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefApi.BLL.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserResponse>Login(LoginRequest request);
        Task<UserResponse>Register(RegisterRequest request);
        Task<string> ConfirmEmail(string userId, string token);
        Task<string> ForgetPassword(ForgetPasswordRequest request);
        Task<string> VerifyResetCode(VerifyResetCodeRequest request);
        Task<string> ResetPassword(ResetPasswordRequest request);
    }
}
