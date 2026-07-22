import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { applicationApi } from '../api/applicationApi';
import { interviewApi } from '../api/interviewApi';
import './RecruiterApplicants.css';

const initialForm = {
    interviewType: 'Online',
    scheduledDate: '',
    duration: 60,
    location: '',
    meetingLink: '',
    notes: '',
    interviewerIds: []
};

export default function ScheduleInterview() {
    const { jobId, applicationId } = useParams();
    const navigate = useNavigate();
    const [application, setApplication] = useState(null);
    const [interviewers, setInterviewers] = useState([]);
    const [form, setForm] = useState(initialForm);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState('');
    const backRoute = `/recruiter/jobs/${jobId}/applicants/${applicationId}`;

    useEffect(() => {
        let active = true;
        Promise.all([
            applicationApi.getRecruiterApplication(applicationId),
            interviewApi.getEligibleInterviewers()
        ]).then(([applicationResult, interviewerResult]) => {
            if (!active) return;
            if (Number(applicationResult.jobId) !== Number(jobId)) {
                throw new Error('This application does not belong to the selected job.');
            }
            setApplication(applicationResult);
            setInterviewers(Array.isArray(interviewerResult) ? interviewerResult : []);
            if (applicationResult.currentStatus !== 'Shortlisted') {
                setError('Only Shortlisted applications can be scheduled for an interview.');
            }
        }).catch(requestError => {
            if (active) setError(requestError.message || 'Interview scheduling details could not be loaded.');
        }).finally(() => {
            if (active) setLoading(false);
        });
        return () => { active = false; };
    }, [applicationId, jobId]);

    const updateField = ({ target: { name, value } }) => {
        setForm(current => ({ ...current, [name]: value }));
        setError('');
    };

    const toggleInterviewer = (interviewerId) => {
        setForm(current => ({
            ...current,
            interviewerIds: current.interviewerIds.includes(interviewerId)
                ? current.interviewerIds.filter(id => id !== interviewerId)
                : [...current.interviewerIds, interviewerId]
        }));
        setError('');
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        if (submitting || application?.currentStatus !== 'Shortlisted') return;
        if (!form.scheduledDate) {
            setError('Choose an interview date and time.');
            return;
        }

        setSubmitting(true);
        setError('');
        try {
            const created = await interviewApi.create({
                applicationId: Number(applicationId),
                interviewType: form.interviewType,
                interviewRound: 1,
                scheduledDate: new Date(form.scheduledDate).toISOString(),
                duration: Number(form.duration),
                location: form.location || null,
                meetingLink: form.meetingLink || null,
                notes: form.notes || null,
                interviewerIds: form.interviewerIds
            });
            navigate(backRoute, {
                replace: true,
                state: { interviewScheduled: true, interviewId: created.interviewId }
            });
        } catch (requestError) {
            setError(requestError.message || 'The interview could not be scheduled.');
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) return <div className="recruiter-route-state">Loading interview scheduling details...</div>;

    return (
        <main className="recruiter-applicants-page" style={{ maxWidth: 820 }}>
            <header className="recruiter-applicants-heading">
                <div>
                    <button className="recruiter-back" onClick={() => navigate(backRoute)}>Back to applicant</button>
                    <span className="recruiter-eyebrow">Applicant pipeline</span>
                    <h1>Schedule interview</h1>
                    <p>{application?.candidateName || 'Candidate'} for {application?.jobTitle || 'the selected job'}</p>
                </div>
                {application && <span className={`recruiter-status recruiter-status--${application.currentStatus.toLowerCase()}`}>{application.currentStatus}</span>}
            </header>

            {error && <div className="recruiter-applicants-notice recruiter-applicants-notice--error" role="alert">{error}</div>}

            <form className="recruiter-detail-card" onSubmit={handleSubmit} noValidate>
                <div className="recruiter-form-grid">
                    <label className="recruiter-form-field">
                        <span>Interview type *</span>
                        <select name="interviewType" value={form.interviewType} onChange={updateField} disabled={submitting}>
                            <option value="Online">Online</option>
                            <option value="In-Person">In-Person</option>
                            <option value="Phone">Phone</option>
                        </select>
                    </label>
                    <label className="recruiter-form-field">
                        <span>Date and time *</span>
                        <input type="datetime-local" name="scheduledDate" value={form.scheduledDate} onChange={updateField} disabled={submitting} required />
                    </label>
                    <label className="recruiter-form-field">
                        <span>Duration (minutes) *</span>
                        <input type="number" name="duration" min="15" max="480" step="15" value={form.duration} onChange={updateField} disabled={submitting} />
                    </label>
                    {form.interviewType === 'In-Person' && <label className="recruiter-form-field"><span>Location *</span><input name="location" value={form.location} onChange={updateField} maxLength="255" disabled={submitting} required /></label>}
                    {form.interviewType === 'Online' && <label className="recruiter-form-field"><span>HTTPS meeting link *</span><input type="url" name="meetingLink" value={form.meetingLink} onChange={updateField} placeholder="https://meet.example.com/interview" disabled={submitting} required /></label>}
                    <label className="recruiter-form-field recruiter-form-field--wide"><span>Notes</span><textarea name="notes" value={form.notes} onChange={updateField} maxLength="2000" rows="4" disabled={submitting} /></label>
                </div>

                <fieldset style={{ border: 0, padding: 0, margin: '1.5rem 0' }}>
                    <legend style={{ fontWeight: 700, marginBottom: '0.75rem' }}>Interviewers *</legend>
                    {interviewers.length === 0 ? <p>No active Hiring Managers are available.</p> : interviewers.map(interviewer => (
                        <label key={interviewer.interviewerId} style={{ display: 'flex', gap: '0.75rem', alignItems: 'center', marginBottom: '0.75rem' }}>
                            <input type="checkbox" checked={form.interviewerIds.includes(interviewer.interviewerId)} onChange={() => toggleInterviewer(interviewer.interviewerId)} disabled={submitting} />
                            <span><strong>{interviewer.name}</strong>{interviewer.position ? ` - ${interviewer.position}` : ''}</span>
                        </label>
                    ))}
                </fieldset>

                <div className="recruiter-setup-actions">
                    <button type="button" className="recruiter-button recruiter-button--ghost" onClick={() => navigate(backRoute)} disabled={submitting}>Cancel</button>
                    <button type="submit" className="recruiter-button recruiter-button--primary" disabled={submitting || application?.currentStatus !== 'Shortlisted' || interviewers.length === 0}>
                        {submitting ? 'Scheduling...' : 'Schedule interview'}
                    </button>
                </div>
            </form>
        </main>
    );
}
