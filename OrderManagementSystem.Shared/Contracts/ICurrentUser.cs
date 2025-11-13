using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Contracts
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        string Email {  get; }

        List<string> GetClaims(string claimType);
        bool HasClaim(string claimType, string claimValue);
    }
}
