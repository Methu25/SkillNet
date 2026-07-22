import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { hiringApi } from '../api/hiringApi';
import './InterviewDetails.css';

const emptyEvaluation = { technicalScore: '', communicationScore: '', problemSolvingScore: '', cultureFitScore: '', recommendation: 'Hire', comments: '' };
const formatDateTime = value => value ? new Date(value).toLocaleString() : 'Not scheduled';

function InterviewDetails() {
    const { id } = useParams();
    const [interview, setInterview] = useState(null);
    const [evaluation, setEvaluation] = useState(emptyEvaluation);
    const [savedEvaluation, setSavedEvaluation] = useState(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [deciding, setDeciding] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    useEffect(() => {
        let active = true;
        hiringApi.getAssignedInterview(id).then(async interviewResult => {
            if (!active) return;
            setInterview(interviewResult);
            if (interviewResult.hasEvaluation) setSavedEvaluation(await hiringApi.getEvaluation(id));
        }).catch(requestError => {
            if (active) setError(requestError.message || 'Interview details could not be loaded.');
        }).finally(() => { if (active) setLoading(false); });
        return () => { active = false; };
    }, [id]);

    const scorePreview = useMemo(() => {
        const values = ['technicalScore', 'communicationScore', 'problemSolvingScore', 'cultureFitScore'].map(key => Number(evaluation[key]));
        return values.every(value => value >= 1 && value <= 10) ? (values.reduce((sum, value) => sum + value, 0) / 4).toFixed(2) : '—';
    }, [evaluation]);

    const handleChange = ({ target: { name, value } }) => {
        setEvaluation(current => ({ ...current, [name]: value }));
        setError('');
        setSuccess('');
    };

    const submitEvaluation = async event => {
        event.preventDefault();
        if (submitting) return;
        setSubmitting(true);
        setError('');
        try {
            const result = await hiringApi.submitEvaluation(id, {
                technicalScore: Number(evaluation.technicalScore),
                communicationScore: Number(evaluation.communicationScore),
                problemSolvingScore: Number(evaluation.problemSolvingScore),
                cultureFitScore: Number(evaluation.cultureFitScore),
                recommendation: evaluation.recommendation,
                comments: evaluation.comments
            });
            setSavedEvaluation(result);
            setInterview(current => ({ ...current, hasEvaluation: true, applicationStatus: 'EvaluationSubmitted', status: 'EvaluationSubmitted' }));
            setSuccess('Evaluation submitted successfully. Confirm the final hiring decision below.');
        } catch (requestError) {
            setError(requestError.message || 'The evaluation could not be submitted.');
        } finally {
            setSubmitting(false);
        }
    };

    const recordDecision = async decision => {
        if (deciding || !window.confirm(`Confirm final decision: ${decision}?`)) return;
        setDeciding(true);
        setError('');
        try {
            await hiringApi.recordDecision(id, decision);
            setInterview(current => ({ ...current, applicationStatus: decision, status: decision }));
            setSuccess(`Final decision recorded: ${decision}.`);
        } catch (requestError) {
            setError(requestError.message || 'The final decision could not be recorded.');
        } finally {
            setDeciding(false);
        }
    };

    if (loading) return <main className="interview-details-page"><h2>Loading assigned interview...</h2></main>;
    if (error && !interview) return <main className="interview-details-page"><div className="error-message" role="alert">{error}</div><Link className="back-button" to="/hiring-dashboard">Back to dashboard</Link></main>;
    if (!interview) return null;
    const terminal = ['Hired', 'Rejected'].includes(interview.applicationStatus);

    return (
        <main className="interview-details-page">
            <header className="details-header"><div><h1>{interview.candidateName || 'Interview details'}</h1><p>{interview.jobTitle}</p></div><Link className="back-button" to="/hiring-dashboard">Back to dashboard</Link></header>
            {error && <div className="error-message" role="alert">{error}</div>}
            {success && <div className="success-message" role="status">{success}</div>}
            <div className="details-grid">
                <section className="details-card"><h2>Candidate</h2><p><strong>Name:</strong> {interview.candidateName}</p><p><strong>Email:</strong> {interview.candidateEmail}</p><p><strong>Summary:</strong> {interview.candidateSummary || 'Not provided'}</p></section>
                <section className="details-card"><h2>Interview</h2><p><strong>Scheduled:</strong> {formatDateTime(interview.scheduledDate)}</p><p><strong>Type:</strong> {interview.interviewType}</p><p><strong>Duration:</strong> {interview.duration} minutes</p><p><strong>Application status:</strong> {interview.applicationStatus}</p></section>
            </div>

            {!savedEvaluation && interview.applicationStatus === 'Interviewing' && <section className="details-card full-width"><h2>Submit evaluation</h2><form onSubmit={submitEvaluation}><div className="score-grid">{[
                ['technicalScore', 'Technical'], ['communicationScore', 'Communication'], ['problemSolvingScore', 'Problem solving'], ['cultureFitScore', 'Culture fit']
            ].map(([name, label]) => <label key={name}>{label} score (1–10)<input name={name} type="number" min="1" max="10" value={evaluation[name]} onChange={handleChange} required disabled={submitting} /></label>)}<label>Overall score preview<input readOnly value={scorePreview} aria-label="Overall score preview" /></label><label>Recommendation<select name="recommendation" value={evaluation.recommendation} onChange={handleChange} disabled={submitting}><option value="Hire">Hire</option><option value="Reject">Reject</option></select></label></div><label>Comments *<textarea name="comments" value={evaluation.comments} onChange={handleChange} maxLength="2000" required disabled={submitting} /></label><button className="submit-button" disabled={submitting}>{submitting ? 'Submitting...' : 'Submit evaluation'}</button></form></section>}

            {savedEvaluation && <section className="details-card full-width"><h2>Saved evaluation</h2><div className="score-grid"><p><strong>Technical:</strong> {savedEvaluation.technicalScore}/10</p><p><strong>Communication:</strong> {savedEvaluation.communicationScore}/10</p><p><strong>Problem solving:</strong> {savedEvaluation.problemSolvingScore}/10</p><p><strong>Culture fit:</strong> {savedEvaluation.cultureFitScore}/10</p><p><strong>Overall:</strong> {Number(savedEvaluation.overallScore).toFixed(2)}/10</p><p><strong>Recommendation:</strong> {savedEvaluation.recommendation}</p></div><p><strong>Comments:</strong> {savedEvaluation.comments}</p></section>}

            {savedEvaluation && interview.applicationStatus === 'EvaluationSubmitted' && <section className="details-card full-width"><h2>Final hiring decision</h2><p>The evaluation recommendation is advisory. Confirm the final application decision explicitly.</p><div className="decision-actions"><button className="submit-button" onClick={() => recordDecision('Hired')} disabled={deciding}>Hire candidate</button><button className="back-button" onClick={() => recordDecision('Rejected')} disabled={deciding}>Reject candidate</button></div></section>}
            {terminal && <section className={`details-card full-width ${interview.applicationStatus === 'Hired' ? 'success-message' : 'error-message'}`} role="status"><h2>Final decision: {interview.applicationStatus}</h2><p>This application is in a terminal state and cannot be changed.</p></section>}
        </main>
    );
}

export default InterviewDetails;
