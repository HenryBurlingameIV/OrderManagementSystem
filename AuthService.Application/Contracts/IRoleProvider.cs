using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Contracts
{
    public interface IRoleProvider
    {
        Task<Role> GetClientRoleAsync(CancellationToken ct);
        Task<Role> GetAdminRoleAsync(CancellationToken ct);
        Task<Role> GetManagerRoleAsync(CancellationToken ct);
        Task<IList<Role>> GetRolesByNamesAsync(List<string> roleNames, CancellationToken ct);
    }
}
