using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SafeScribe.Application.Services;
using SafeScribe.Domain.Interfaces;
using SafeScribe.Infrastructure.Data;
using SafeScribe.Infrastructure.Repositories;
using SafeScribe.Middleware;
using System.Text;
using SafeScribe.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);


// DATABASE CONFIGURATION
builder.Services.AddDbContext<SafeScribeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"))
);


// CONTROLLERS
builder.Services.AddControllers();


// JWT CONFIGURATION
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Valida se o token foi assinado utilizando a chave correta.
            ValidateIssuerSigningKey = true,

            // Define a chave secreta usada para validar a assinatura do token.
            IssuerSigningKey = key,

            // Habilita a validação do issuer do token.
            ValidateIssuer = true,

            // Define o emissor esperado do token (valor do campo "iss").
            ValidIssuer = jwt["Issuer"],

            // Habilita a validação do audience do token.
            ValidateAudience = true,

            // Define o público-alvo esperado .
            ValidAudience = jwt["Audience"],

            // Define a margem de tolerância para expiração do token.
            // Quando definido como zero, o token expira exatamente no horário indicado.
            ClockSkew = TimeSpan.Zero,

            // Habilita a verificação do tempo de vida do token (expiração).
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// DEPENDENCY INJECTION

// Repositórios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();

// Serviços de aplicação
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NoteService>();

// Serviços relacionados a autenticação
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<ITokenBlacklistService, InMemoryTokenBlacklistService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(NoteProfile).Assembly);
});

// SWAGGER CONFIGURATION (com JWT)

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SafeScribe API",
        Version = "v1",
        Description = "API de autenticação e autorização com JWT"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Insira o token JWT no formato: Bearer {seu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
