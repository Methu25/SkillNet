var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// builder.Services.AddOpenApi();

builder.Services.AddScoped<SkillNet.Server.Interfaces.IInterviewRepository, SkillNet.Server.Repositories.InterviewRepository>();
builder.Services.AddScoped<SkillNet.Server.Interfaces.IInterviewService, SkillNet.Server.Services.InterviewService>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();