using Fgc.Users.Application.DTOS.Users;
using Fgc.Users.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fgc.Users.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("admin/users")]
    [Tags("AdminUsers")]
    public class AdminUserController : ControllerBase
    {
        private readonly AdminUserService _userService;

        public AdminUserController(AdminUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllAsync();

            var response = users.Select(u => new
            {
                u.Id,
                u.Name,
                Email = u.Email.Value,
                u.Role
            });

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user is null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Name,
                Email = user.Email.Value,
                user.Role
            });
        }

        [HttpPut("{id:guid}/role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> UpdateUserByAdmin(Guid id, UpdateUserByAdminRequest request)
        {

                var user = await _userService.UpdateByAdminAsync(
                    id,
                    request.Role
                );

                return Ok(new
                {
                    user.Id,
                    user.Name,
                    Email = user.Email.Value,
                    user.Role
                });
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> DeleteUser(Guid id)
        {
                await _userService.DeleteUserAsync(id);
                return NoContent();
        }
    }
}

