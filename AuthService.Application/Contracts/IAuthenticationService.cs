using AuthService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Contracts
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    }
}
