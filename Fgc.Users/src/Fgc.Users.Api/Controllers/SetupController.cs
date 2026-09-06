using Fgc.Users.API.Security;
using Fgc.Users.Application.Services;
using Fgc.Users.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Fgc.Users.API.Controllers
{
    [ApiController]
    [Route("setup")]
    [Tags("Setup")]
    public class SetupController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IWebHostEnvironment _env;

        public SetupController(UserService userService, JwtTokenGenerator jwtTokenGenerator, IWebHostEnvironment env)
        {
            _userService = userService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _env = env;
        }

        /// <summary>
        /// Cria o primeiro usuário admin para facilitar os testes iniciais. Apenas para ambiente de desenvolvimento.
        /// </summary>
        [HttpPost("first-admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateFirstAdmin([FromBody] SetupAdminRequest request)
        {
            if (!_env.IsDevelopment())
                return NotFound(); // Em produção, ninguém descobre que existe.

            try
            {
                var user = await _userService.CreateFirstAdminAsync(request.Name, request.Email, request.Password);
                var token = _jwtTokenGenerator.GenerateToken(user);

                return Ok(new
                {
                    user.Id,
                    user.Name,
                    Email = user.Email.Value,
                    Token = token
                });
            }
            catch (ConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public record SetupAdminRequest(string Name, string Email, string Password);
}
