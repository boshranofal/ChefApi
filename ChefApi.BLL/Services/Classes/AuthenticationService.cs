using ChefApi.BLL.Services.Interfaces;
using ChefApi.DAL.DTO.Request;
using ChefApi.DAL.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefApi.BLL.Services.Classes
{
    public class AuthenticationService : IAuthenticationService
    {
        public Task<UserResponse> Login(LoginRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse> Register(RegisterRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
