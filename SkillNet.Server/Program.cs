using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ADD CORS POLICY
// ==========================================
// 1. ADD CORS POLICY
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        // 5173 is standard for Vite (the modern VS React template)
        // 3000 is standard if using older Create React App
        policy.WithOrigins("https://localhost:5173", "http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ==========================================
// 2. CONFIGURE JWT AUTHENTICATION
// ==========================================
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretHighlySecureKeyWithAtLeast32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ==========================================
// 3. ACTIVATE SECURITY MIDDLEWARE (ORDER MATTERS)
// ==========================================
app.UseCors("AllowAngularApp"); // CORS must come before Auth

app.UseAuthentication(); // MUST come before Authorization (Verifies WHO you are)
app.UseAuthorization();  // You already had this! (Verifies WHAT you can do)

app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();