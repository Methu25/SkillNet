using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;

namespace SkillNet.Tests;

public class MatchAnalysisProviderTests
{
    [Theory]
    [InlineData(false, "key", false)]
    [InlineData(true, "", false)]
    [InlineData(true, "key", true)]
    public void FactorySelectsConfiguredProvider(bool enabled, string key, bool expectsGemini)
    {
        var configuration = Configuration(new Dictionary<string, string?> { ["Gemini:Enabled"] = enabled.ToString(), ["Gemini:ApiKey"] = key });
        var fallback = new RequiredSkillCoverageFallbackProvider(new SpyMatchingStrategy());
        var gemini = new GeminiMatchAnalysisProvider(new FakeHttpClientFactory(new HttpClient(new StubHandler(_ => JsonResponse(50)))), configuration);
        var selected = new MatchAnalysisProviderFactory(configuration, gemini, fallback).CreateProvider();
        Assert.Equal(expectsGemini, selected is GeminiMatchAnalysisProvider);
    }

    [Theory]
    [InlineData(120, 100)]
    [InlineData(-5, 0)]
    [InlineData(84, 84)]
    public async Task GeminiMapsStructuredJsonAndClampsScore(int providerScore, int expected)
    {
        var provider = GeminiProvider(_ => JsonResponse(providerScore));
        var result = await provider.AnalyzeAsync(Request(), CancellationToken.None);
        Assert.Equal(expected, result.AiScore);
        Assert.Equal("Gemini", result.Provider);
        Assert.False(result.IsFallback);
        Assert.Equal("Strong Match", result.RecommendedAction);
    }

    [Fact]
    public async Task InvalidGeminiJsonRaisesControlledProviderFailure()
    {
        var provider = GeminiProvider(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("not-json") });
        await Assert.ThrowsAsync<MatchAnalysisProviderException>(() => provider.AnalyzeAsync(Request(), CancellationToken.None));
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ProviderHttpFailureRaisesControlledException(HttpStatusCode status)
    {
        var provider = GeminiProvider(_ => new HttpResponseMessage(status));
        await Assert.ThrowsAsync<MatchAnalysisProviderException>(() => provider.AnalyzeAsync(Request(), CancellationToken.None));
    }

    [Fact]
    public async Task ProviderTimeoutRaisesControlledException()
    {
        var provider = GeminiProvider(_ => throw new TaskCanceledException());
        await Assert.ThrowsAsync<MatchAnalysisProviderException>(() => provider.AnalyzeAsync(Request(), CancellationToken.None));
    }

    [Fact]
    public async Task FallbackReusesCandidateJobMatchingStrategy()
    {
        var strategy = new SpyMatchingStrategy();
        var result = await new RequiredSkillCoverageFallbackProvider(strategy).AnalyzeAsync(Request(), CancellationToken.None);
        Assert.True(strategy.Called);
        Assert.True(result.IsFallback);
        Assert.Equal("RequiredSkillCoverage", result.Provider);
        Assert.Equal(75, result.AiScore);
    }

    private static GeminiMatchAnalysisProvider GeminiProvider(Func<HttpRequestMessage, HttpResponseMessage> response) =>
        new(new FakeHttpClientFactory(new HttpClient(new StubHandler(response)) { BaseAddress = new Uri("https://example.test") }),
            Configuration(new Dictionary<string, string?> { ["Gemini:ApiKey"] = "test-key", ["Gemini:Model"] = "test-model" }));

    private static MatchAnalysisRequestData Request() => new()
    {
        JobTitle = "Backend Engineer",
        JobDescription = "Build APIs",
        RequiredSkills = ["C#", "SQL"],
        CandidateSkills = ["C#"]
    };

    private static HttpResponseMessage JsonResponse(int score)
    {
        var structured = System.Text.Json.JsonSerializer.Serialize(new { aiScore = score, strengths = new[] { "C#" }, skillGaps = new[] { "Azure" }, conciseExplanation = "Relevant professional overlap.", recommendedAction = "Strong Match" });
        var envelope = System.Text.Json.JsonSerializer.Serialize(new { candidates = new[] { new { content = new { parts = new[] { new { text = structured } } } } } });
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(envelope, Encoding.UTF8, "application/json") };
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values) => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private sealed class FakeHttpClientFactory(HttpClient client) : IHttpClientFactory { public HttpClient CreateClient(string name) => client; }
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(response(request));
    }
    private sealed class SpyMatchingStrategy : ICandidateJobMatchingStrategy
    {
        public bool Called { get; private set; }
        public MatchingResult Match(MatchingInput input) { Called = true; return new MatchingResult { MatchScore = 75, MatchedSkills = ["C#"], MissingSkills = ["SQL"], MatchedRequiredSkillCount = 1, TotalRequiredSkills = 2 }; }
    }
}
