using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TicketSystem.API.Data;
using TicketSystem.API.Middleware;
using TicketSystem.API.Repositories;
using TicketSystem.API.Services;
using TicketSystem.API.Settings;

var builder = WebApplication.CreateBuilder(args);


var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()
    ?? throw new Exception("MongoDbSettings not configured");
builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ITicketRepository, TicketRepository>();
builder.Services.AddSingleton<IKnowledgeArticleRepository, KnowledgeArticleRepository>();


builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<SmartRoutingService>();
builder.Services.AddSingleton<KnowledgeBaseService>();
builder.Services.AddSingleton<TicketService>();
builder.Services.AddSingleton<AdminService>();
builder.Services.AddSingleton<DataSeeder>();
builder.Services.AddHttpClient<AIService>();


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


// Browsers send Origin without a trailing slash; WithOrigins must match exactly.
static string[] NormalizeOrigins(params string[] origins) =>
    origins
        .Where(static o => !string.IsNullOrWhiteSpace(o))
        .Select(static o => o.Trim().TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

var configuredOrigins = builder.Configuration["AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();

var corsOrigins = NormalizeOrigins(
    configuredOrigins.Concat(new[]
    {
        "http://localhost:4200",
        "http://localhost:5043",
        "https://localhost:7234",
        "https://ticketai.netlify.app"
    }).ToArray());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();