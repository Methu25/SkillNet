import { useState } from 'react';

export default function MatchAnalysisPanel({ loadAnalysis, buttonLabel = 'Get AI Analysis' }) {
    const [result, setResult] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const analyze = async () => {
        if (loading) return;
        setLoading(true); setError('');
        try { setResult(await loadAnalysis()); }
        catch (requestError) { setError(requestError.message || 'Analysis could not be completed.'); }
        finally { setLoading(false); }
    };
    return <section className="recruiter-detail-card" aria-live="polite">
        <h2>Candidate–job compatibility</h2>
        {!result && <button type="button" className="recruiter-button recruiter-button--primary" onClick={analyze} disabled={loading}>{loading ? 'Analyzing...' : buttonLabel}</button>}
        {error && <p role="alert">{error}</p>}
        {result && <div>
            <p><strong>{result.isFallback ? 'Skill-based fallback analysis' : 'AI-assisted analysis'}</strong></p>
            <p><strong>Compatibility score:</strong> {result.aiScore}/100</p>
            <p><strong>Recommended action:</strong> {result.recommendedAction}</p>
            <p>{result.conciseExplanation}</p>
            <div><strong>Strengths</strong>{result.strengths?.length ? <ul>{result.strengths.map(item => <li key={item}>{item}</li>)}</ul> : <p>None identified.</p>}</div>
            <div><strong>Skill gaps</strong>{result.skillGaps?.length ? <ul>{result.skillGaps.map(item => <li key={item}>{item}</li>)}</ul> : <p>None identified.</p>}</div>
            <small>Provider: {result.provider} ({result.model})</small>
        </div>}
        <p><small>This analysis supports review and does not replace human judgment.</small></p>
    </section>;
}
