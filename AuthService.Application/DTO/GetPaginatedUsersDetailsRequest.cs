using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTO
{
    public record GetPaginatedUsersDetailsRequest(
        int PageNumber,
        int PageSize,
        string? Search = null,          
        string? Role = null,     
        bool? IsActive = null,          
        string? SortBy = "Email",
        bool Descending = false
    );

}
