using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTO
{
    public record GetPaginatedUserDetailsRequest(
        int PageNumber,
        int PageSize,
        string? Search = null,          
        List<string>? Roles = null,     
        bool? IsActive = null,          
        string? SortBy = "Email",
        bool Descending = false
    );

}
