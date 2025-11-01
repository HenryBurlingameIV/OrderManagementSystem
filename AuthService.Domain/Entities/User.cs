using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }

        public List<Role> Roles { get; set; } = new();

        public List<string> GetAllPermissions()
        {
            return Roles?
                .SelectMany(r => r.Permissions)
                .Select(p => p.Name)
                .Distinct()
                .ToList() ?? new List<string>();
        }
    }
}
