using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces;

public interface IMatchAnalysisProvider
{
    Task<MatchAnalysisResult> AnalyzeAsync(MatchAnalysisRequestData request, CancellationToken cancellationToken);
}

public interface IMatchAnalysisProviderFactory
{
    IMatchAnalysisProvider CreateProvider();
}

public class MatchAnalysisProviderException : Exception
{
    public MatchAnalysisProviderException(string message, Exception? innerException = null) : base(message, innerException) { }
}
