using AuthService.Application.Contracts;
using AuthService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    internal class UserManagmentService : IUserManagmentService
    {
        public Task AssignRoleAsync(Guid userId, List<string> roles, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task CreateUserAsync(CreateUserRequest request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task DeactivateUserAsync(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<UserViewModel> GetUserAsync(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task UnassignRoleAsync(Guid userId, string role, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
