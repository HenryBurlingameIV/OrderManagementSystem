using AuthService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Contracts
{
    public interface IUserManagmentService
    {
        Task CreateUserAsync(CreateUserRequest request, CancellationToken ct);
        Task DeactivateUserAsync(Guid userId, CancellationToken ct);

        Task<UserViewModel> GetUserAsync(Guid userId, CancellationToken ct);

        Task AssignRoleAsync(Guid userId, List<string> roles, CancellationToken ct);
        Task UnassignRoleAsync(Guid userId, string role, CancellationToken ct);
    }
}
