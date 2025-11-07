using AuthService.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagmentService _userManagmentService;

        public UsersController(IUserManagmentService userManagmentService)
        {
            _userManagmentService = userManagmentService;
        }

        [HttpPatch("{userId:Guid}/activate")]
        public async Task<ActionResult> ActivateUser([FromRoute]Guid userId, CancellationToken ct)
        {
            await _userManagmentService.ActivateUserAsync(userId, ct);
            return NoContent();
        }

        [HttpPatch("{userId:Guid}/deactivate")]
        public async Task<ActionResult> DeactivateUser([FromRoute]Guid userId, CancellationToken ct)
        {
            await _userManagmentService.DeactivateUserAsync(userId, ct);
            return NoContent();
        }
    }
}
