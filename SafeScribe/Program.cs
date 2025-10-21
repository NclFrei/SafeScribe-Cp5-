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

// =====================================
// DATABASE CONFIGURATION
// =====================================
builder.Services.AddDbContext<SafeScribeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"))
);

// =====================================
// CONTROLLERS
// =====================================
builder.Services.AddControllers();

// =====================================
// JWT CONFIGURATION
// =====================================
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,

            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],

            // Se quiser validar o Audience, pode deixar "true".
            // Se quiser simplificar os testes, "false" é aceitável.
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            // 🔥 ESSENCIAL: mapeia os claims corretos (role e nome)
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        // 🔍 Ativa logging detalhado de falhas (útil pra debug de 401)
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[JWT ERROR] {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"[JWT OK] Usuário autenticado: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// =====================================
// DEPENDENCY INJECTION
// =====================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NoteService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<ITokenBlacklistService, InMemoryTokenBlacklistService>();

// =====================================
// AUTOMAPPER CONFIGURATION
// =====================================
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(NoteProfile).Assembly);
});

// =====================================
// SWAGGER CONFIGURATION (com JWT)
// =====================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SafeScribe", Version = "v1" });

    var jwtSecurityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Description = "Insira o token JWT no campo abaixo.",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// =====================================
// BUILD APP
// =====================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Ordem obrigatória
app.UseAuthentication();                     // 1º - valida token
app.UseMiddleware<JwtBlacklistMiddleware>(); // 2º - checa blacklist
app.UseAuthorization();                      // 3º - valida roles

app.MapControllers();

app.Run();
