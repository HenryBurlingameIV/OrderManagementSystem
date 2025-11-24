
using AuthService.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Services
{

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password);

        public bool VerifyPassword(string password, string hash) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password, hash);


    }
}
