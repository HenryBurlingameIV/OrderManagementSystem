using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTO
{
    public record CreateUserRequest(string Name, string Email, string Password, List<string> Roles);

}
