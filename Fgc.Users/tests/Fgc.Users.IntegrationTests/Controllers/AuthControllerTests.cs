using System.Net;
using System.Net.Http.Json;

namespace Fgc.Users.IntegrationTests.Controllers
{
    // O IClassFixture injeta a fábrica customizada na classe de testes.
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            // Cria um cliente HTTP para enviar requisições à API.
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task Register_ShouldReturnSuccessStatusCode_WhenInputIsValid()
        {
            // Arrange
            var request = new
            {
                Name = "Integration Test User",
                Email = "integration@test.com",
                Password = "Passw0rd!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/auth/register", request);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"A casa pegou fogo! Motivo: {errorMessage}");
            }
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new
            {
                Name = "Ronnam",
                Email = "invalid-email-sem-arroba",
                Password = "Passw0rd!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/auth/register", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithEmptyName_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new
            {
                Name = "",
                Email = "ronnam@test.com",
                Password = "Passw0rd!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/auth/register", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var request = new
            {
                Name = "Ronnam",
                Email = "duplicate.email@test.com",
                Password = "Passw0rd!"
            };

            // Seed: Registrar o usuário pela primeira vez para criar a duplicação.
            await _client.PostAsJsonAsync("/auth/register", request);

            // Act: Tentar registrar o mesmo usuário novamente.
            var response = await _client.PostAsJsonAsync("/auth/register", request);

            
            // Assert: Verificar se a resposta é BadRequest devido ao email duplicado.
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}
