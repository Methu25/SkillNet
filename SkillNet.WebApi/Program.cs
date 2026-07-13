using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

using SkillNet.Application.Services;
using SkillNet.Application.Interfaces;
using SkillNet.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Resolve LocalDB connection string dynamically if running on a machine with LocalDB version compat issues
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
{
    var resolvedConnectionString = ResolveLocalDbConnectionString(connectionString);
    if (resolvedConnectionString != connectionString)
    {
        builder.Configuration["ConnectionStrings:DefaultConnection"] = resolvedConnectionString;
        Console.WriteLine($"Resolved LocalDB connection string: {resolvedConnectionString}");
    }
}

// ==========================================
// 1. CORS POLICY (React Frontend)
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Register Auth Services (Application Layer)
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Register Admin Module Services (Application Layer)
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Register Interview Module (Infrastructure repositories + Application services)
// Note: JobCategoryService uses its own Singleton pattern internally (not registered here)
builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
builder.Services.AddScoped<IInterviewService, InterviewService>();

// Register Job & Recruiter Module
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IRecruiterService, RecruiterService>();

// ==========================================
// 3. CONFIGURE SWAGGER (WITH JWT SUPPORT)
// ==========================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SkillNet Recruitment API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: Bearer 12345abcdef",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SkillNet API v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();

static string ResolveLocalDbConnectionString(string connectionString)
{
    try
    {
        var match = Regex.Match(connectionString, @"(?:Server|Data Source)=\(localdb\)\\([^;]+)", RegexOptions.IgnoreCase);
        if (!match.Success) return connectionString;

        string instanceName = match.Groups[1].Value.Trim();

        // 1. Ensure LocalDB instance is started
        var startInfo = new ProcessStartInfo
        {
            FileName = "sqllocaldb",
            Arguments = $"start \"{instanceName}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (var process = Process.Start(startInfo))
        {
            process?.WaitForExit();
        }

        // 2. Query the instance info to get its active named pipe path
        var infoInfo = new ProcessStartInfo
        {
            FileName = "sqllocaldb",
            Arguments = $"info \"{instanceName}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        string output = string.Empty;
        using (var process = Process.Start(infoInfo))
        {
            if (process != null)
            {
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
        }

        // 3. Parse the pipe name from the output
        var pipeMatch = Regex.Match(output, @"Instance pipe name:\s*(np:\\\\[^\s\r\n]+)", RegexOptions.IgnoreCase);
        if (pipeMatch.Success)
        {
            string pipeName = pipeMatch.Groups[1].Value;
            string resolved = Regex.Replace(connectionString, @"(?:Server|Data Source)=\(localdb\)\\[^;]+", $"Server={pipeName}", RegexOptions.IgnoreCase);
            return resolved;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[LocalDB Resolver] Failed to resolve named pipe for LocalDB: {ex.Message}");
    }

    return connectionString;
}
