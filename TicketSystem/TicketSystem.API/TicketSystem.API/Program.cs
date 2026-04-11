using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TicketSystem.API.Data;
using TicketSystem.API.Middleware;
using TicketSystem.API.Services;
using TicketSystem.API.Settings;

var builder = WebApplication.CreateBuilder(args);

// ── MongoDB Settings ──────────────────────────────────────────
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()
    ?? throw new Exception("MongoDbSettings not configured");
builder.Services.AddSingleton(mongoSettings);

// ── Services ──────────────────────────────────────────────────
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<TicketService>();
builder.Services.AddSingleton<AdminService>();
builder.Services.AddSingleton<DataSeeder>();
builder.Services.AddHttpClient<AIService>();

// ── JWT Authentication ────────────────────────────────────────
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new Exception("JWT secret not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"] ?? "http://localhost:4200"
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Seed Data ─────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();