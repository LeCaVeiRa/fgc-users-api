using Fgc.Users.Application.Interfaces;
using Fgc.Users.Application.Services;
using Fgc.Users.Infrastructure.Persistence;
using Fgc.Users.Infrastructure.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// 1. CONFIGURAÇÕES DO MICROSSERVIÇO DE USUÁRIOS
// ============================================================

// A. Banco de Dados (Entity Framework Core)
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("UserDb"))
);

// B. Injeção de Dependências (Repositories)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();

// C. Injeção de Dependências (Services)
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<AuthService>();

// Adicionando o gerador de token aqui junto com os serviços de autenticação.
builder.Services.AddScoped<Fgc.Users.API.Security.JwtTokenGenerator>();

// D. Adicionando o middleware de autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });


builder.Services.AddAuthorization();

// Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// E. MassTransit / RabbitMQ
if (!builder.Services.Any(s => s.ServiceType == typeof(MassTransit.IBus)))
{
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
            {
                h.Username("admin");
                h.Password("admin");
            });
        });
    });
}
// ============================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ============================================================
// 2. MIDDLEWARES DE SEGURANÇA
// ============================================================
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok());

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
public partial class Program { } // Para testes de integração