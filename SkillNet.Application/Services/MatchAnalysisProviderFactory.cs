using Microsoft.Extensions.Configuration;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services;

public class MatchAnalysisProviderFactory(
    IConfiguration configuration,
    GeminiMatchAnalysisProvider gemini,
    RequiredSkillCoverageFallbackProvider fallback) : IMatchAnalysisProviderFactory
{
    public IMatchAnalysisProvider CreateProvider()
    {
        var enabled = configuration.GetValue<bool>("Gemini:Enabled");
        var apiKey = configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        return enabled && !string.IsNullOrWhiteSpace(apiKey) ? gemini : fallback;
    }
}
