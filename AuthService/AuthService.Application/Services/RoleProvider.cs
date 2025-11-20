using AuthService.Application.Contracts;
using AuthService.Domain.Entities;
using OrderManagementSystem.Shared.Authorization;
using OrderManagementSystem.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class RoleProvider : IRoleProvider
    {
        private readonly IEFRepository<Role, int> _rolesRepository;

        public RoleProvider(IEFRepository<Role, int> rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }
        public async Task<Role> GetAdminRoleAsync(CancellationToken ct) =>
            await _rolesRepository.GetFirstOrDefaultAsync(
                filter: r => r.Name == Roles.Admin,
                asNoTraсking: false,
                ct: ct) ?? throw new InvalidOperationException("Admin role not found.");


        public async Task<Role> GetClientRoleAsync(CancellationToken ct) =>        
            await _rolesRepository.GetFirstOrDefaultAsync(
                filter: r => r.Name == Roles.Client,
                asNoTraсking: false,
                ct: ct) ?? throw new InvalidOperationException("Client role not found.");
        

        public async Task<Role> GetManagerRoleAsync(CancellationToken ct) =>
            await _rolesRepository.GetFirstOrDefaultAsync(
                filter: r => r.Name == Roles.Manager,
                asNoTraсking: false,
                ct: ct) ?? throw new InvalidOperationException("Admin role not found.");

        public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct)
        {
            var normalizeName = roleName.Trim().ToLowerInvariant();
            return await _rolesRepository.GetFirstOrDefaultAsync(
                filter: r => r.Name.ToLower() == normalizeName,
                ct: ct);
        }

        public async Task<IList<Role>> GetRolesByNamesAsync(List<string> roleNames, CancellationToken ct)
        {
            if (roleNames == null || !roleNames.Any())
                return new List<Role>();

            var normilizeNames = roleNames.Select(n => n.Trim().ToLowerInvariant());

            return await _rolesRepository.GetAllAsync(
                filter: r => normilizeNames.Contains(r.Name.ToLower()),
                asNoTraсking: false,
                ct: ct);
        }
            
    }
}
