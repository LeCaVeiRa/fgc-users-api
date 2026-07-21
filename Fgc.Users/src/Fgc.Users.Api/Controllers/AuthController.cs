using Fgc.Users.Application.DTOS.Users;
using Fgc.Users.API.Security;
using Fgc.Users.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Fgc.Users.Domain.Exceptions;
using MassTransit;
using Fgc.MessageContracts.Events;

namespace Fgc.Users.Api.Controllers;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthController(
        UserService userService,
        AuthService authService,
        JwtTokenGenerator jwtTokenGenerator,
        IPublishEndpoint publishEndpoint)
    {
        _userService = userService;
        _authService = authService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _publishEndpoint = publishEndpoint;
    }

    // POST /auth/register
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        try
        {
            var user = await _userService.RegisterAsync(
            request.Name,
            request.Email,
            request.Password
            );

            await _publishEndpoint.Publish(new UserCreatedEvent(
                user.Id,
                user.Name,
                user.Email.Value,
                DateTime.UtcNow
                
            ));

            return CreatedAtAction(nameof(Register),
            new { id = user.Id },
            new
            {
                user.Id,
                user.Name,
                Email = user.Email.Value
            });
        }
        catch (ConflictException ex)
        {
            // Captura e-mail duplicado
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Captura o nome vazio ou e-mail inválido (Validação do Doínio)
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /auth/login
    [HttpPost("login")]

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> Login(LoginRequest request)
    {

            var user = await _authService.AuthenticateAsync(
                request.Email,
                request.Password
            );

            var token = _jwtTokenGenerator.GenerateToken(user);

            return Ok(new { token });
        
    }
}
