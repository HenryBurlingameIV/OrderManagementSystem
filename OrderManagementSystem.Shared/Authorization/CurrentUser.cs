using Microsoft.AspNetCore.Http;
using OrderManagementSystem.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Authorization
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(userId, out var result) ? result : Guid.Empty;
            }
        }
        public string Email => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? "";

        public List<string> GetClaims(string claimType)
        {
            return _httpContextAccessor?.HttpContext?.User?
                .FindAll(claimType)
                .Select(c => c.Value)
                .ToList() ?? new List<string>();
        }

        public bool HasClaim(string claimType, string claimValue)
        {
            return GetClaims(claimType).Contains(claimValue);
        }

    }
}
