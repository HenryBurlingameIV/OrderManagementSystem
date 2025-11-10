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
        Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken ct);
        Task ActivateUserAsync(Guid userId, CancellationToken ct);
        Task DeactivateUserAsync(Guid userId, CancellationToken ct);

        Task<UserViewModel> GetUserAsync(Guid userId, CancellationToken ct);

        Task AssignRoleAsync(Guid userId, string role, CancellationToken ct);
        Task RemoveRoleAsync(Guid userId, string role, CancellationToken ct);
    }
}
