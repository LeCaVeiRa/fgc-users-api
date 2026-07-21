using Fgc.Users.Application.Helpers;
using Fgc.Users.Domain.ValueObjects;
using Fgc.Users.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Fgc.Users.API.Controllers
{
    [ApiController]
    [Route("setup")]
    public class SetupController : ControllerBase
    {
        private readonly UsersDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SetupController(UsersDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Cria o primeiro usuário admin para facilitar os testes iniciais. Apenas para ambiente de desenvolvimento.
        /// </summary>
        [HttpPost("first-admin")]
        public IActionResult CreateFirstAdmin([FromBody] SetupAdminRequest request)
        {
            if (!_env.IsDevelopment())
                return NotFound(); // Em produção, ninguém descobre que existe.

            if (_context.Users.Any(u => u.Role == "Admin"))
                return BadRequest(new{error = "An admin user already exists."});

            var user = Domain.Entities.User.Create(
                request.Name,
                Email.Create(request.Email),
                PasswordHasher.Hash(request.Password),
                "Admin"
                );

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Admin user created successfully." });
        }
    }

    public record SetupAdminRequest(string Name, string Email, string Password);
}
