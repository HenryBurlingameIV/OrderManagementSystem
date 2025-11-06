using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTO
{
    public record UserViewModel(Guid Id, string Name, string Email, List<string> Roles);

}
