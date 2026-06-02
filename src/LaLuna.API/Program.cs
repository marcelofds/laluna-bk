using LaLuna.Application;
using LaLuna.Infrastructure;
using LaLuna.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------
// Application & Infrastructure layers
// -------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// -------------------------------------------------
// Keycloak JWT Authentication
// -------------------------------------------------
var keycloak = builder.Configuration.GetSection("Keycloak");
var authority = $"{keycloak["ServerUrl"]}/realms/{keycloak["Realm"]}";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/laluna";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "preferred_username"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity)
                    return Task.CompletedTask;

                var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                if (string.IsNullOrWhiteSpace(realmAccess))
                    return Task.CompletedTask;

                using var document = JsonDocument.Parse(realmAccess);
                if (!document.RootElement.TryGetProperty("roles", out var roles))
                    return Task.CompletedTask;

                foreach (var role in roles.EnumerateArray())
                {
                    var value = role.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                        identity.AddClaim(new Claim(ClaimTypes.Role, value));
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT ERROR:");
                Console.WriteLine(context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});

// -------------------------------------------------
// CORS — allow Angular dev server and production URL
// -------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        var origins = builder.Configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:4200"];

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// -------------------------------------------------
// Controllers & Swagger
// -------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "La Luna Store API",
        Version = "v1",
        Description = "Backend API for La Luna Love Store e-commerce"
    });

    // Add JWT Bearer button in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the Keycloak JWT token here."
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

// -------------------------------------------------
// Build & pipeline
// -------------------------------------------------
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "La Luna Store API v1");
        c.OAuthClientId(keycloak["ClientId"]);
        c.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
