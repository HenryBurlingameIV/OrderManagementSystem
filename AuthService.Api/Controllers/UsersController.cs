using AuthService.Application.Contracts;
using AuthService.Application.DTO;
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

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            var result = await _userManagmentService.CreateUserAsync(request, ct);
            return Ok(result);
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

        [HttpPost("{userId:Guid}/roles/{roleName}")]
        public async Task<ActionResult> AssignRole([FromRoute] Guid userId, [FromRoute] string roleName, CancellationToken ct)
        {
            await _userManagmentService.AssignRoleAsync(userId, roleName, ct);
            return NoContent();
        }

        [HttpDelete("{userId:Guid}/roles/{roleName}")]
        public async Task<ActionResult> RemoveRole([FromRoute] Guid userId, [FromRoute] string roleName, CancellationToken ct)
        {
            await _userManagmentService.RemoveRoleAsync(userId, roleName, ct);
            return NoContent();
        }
    }
}
