using Fgc.Users.Application.DTOS.Users;
using Fgc.Users.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fgc.Users.Api.Controllers
{
    [ApiController]
    [Route("users")]
    [Tags("Users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> UpdateUser(Guid id,UpdateUserRequest request)
        {
            var user = await _userService.UpdateUserAsync(
                id,
                request.Email,
                request.Password
            );

            return Ok(new
            {
                user.Id,
                user.Name,
                Email = user.Email.Value,
                Password = "********"
            });
        }
    }
}
