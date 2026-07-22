import { useCallback, useEffect, useMemo, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { applicationApi } from '../api/applicationApi';
import { jobApi } from '../api/jobApi';
import { matchAnalysisApi } from '../api/matchAnalysisApi';
import MatchAnalysisPanel from '../components/MatchAnalysisPanel';
import './RecruiterApplicants.css';

const formatDate = (value) => value
    ? new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(value))
    : 'Not available';

const formatDateTime = (value) => {
    if (!value) return 'Not available';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return 'Not available';
    return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(date);
};

const formatFileSize = (value) => {
    if (!Number.isFinite(value) || value <= 0) return 'Not available';
    if (value < 1024 * 1024) return `${Math.ceil(value / 1024)} KB`;
    return `${(value / (1024 * 1024)).toFixed(2)} MB`;
};

const statusClass = (status) => String(status || 'unknown').toLowerCase().replace(/[^a-z0-9]+/g, '-');
const isPositiveInteger = (value) => Number.isInteger(Number(value)) && Number(value) > 0;

const RecruiterApplicants = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { jobId, applicationId } = useParams();
    const [job, setJob] = useState(null);
    const [applications, setApplications] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [search, setSearch] = useState('');
    const [status, setStatus] = useState('');
    const [notice, setNotice] = useState(() => location.state?.interviewScheduled
        ? { type: 'success', message: `Interview #${location.state.interviewId} was scheduled successfully.` }
        : null);
    const [shortlisting, setShortlisting] = useState({});
    const [applicationDetail, setApplicationDetail] = useState(null);
    const [detailLoading, setDetailLoading] = useState(Boolean(applicationId));
    const [detailError, setDetailError] = useState('');
    const [statusSubmitting, setStatusSubmitting] = useState(false);
    const [statusFeedback, setStatusFeedback] = useState(null);
    const [noteSubmitting, setNoteSubmitting] = useState(false);
    const [noteFeedback, setNoteFeedback] = useState(null);
    const validJobId = isPositiveInteger(jobId);
    const validApplicationId = !applicationId || isPositiveInteger(applicationId);

    const loadApplicants = useCallback(async () => {
        if (!validJobId) {
            setLoading(false);
            setError('The job route parameter is invalid.');
            return;
        }

        setLoading(true);
        setError('');
        try {
            const [jobResult, summaries] = await Promise.all([
                jobApi.getById(jobId),
                applicationApi.getForJob(jobId, { pageNumber: 1, pageSize: 100 })
            ]);
            const summaryList = Array.isArray(summaries) ? summaries : [];
            const details = await Promise.all(summaryList.map(async summary => {
                try {
                    return { ...summary, ...await applicationApi.getRecruiterApplication(summary.applicationId) };
                } catch {
                    return summary;
                }
            }));
            setJob(jobResult);
            setApplications(details);
        } catch (requestError) {
            setError(requestError.status === 404
                ? 'This job was not found or is not available to your recruiter profile.'
                : requestError.message || 'Applicants could not be loaded.');
        } finally {
            setLoading(false);
        }
    }, [jobId, validJobId]);

    const loadApplicationDetail = useCallback(async () => {
        if (!applicationId) return;
        if (!validJobId || !validApplicationId) {
            setApplicationDetail(null);
            setDetailLoading(false);
            setDetailError('The job or application route parameter is invalid.');
            return;
        }

        setDetailLoading(true);
        setDetailError('');
        try {
            const result = await applicationApi.getRecruiterApplication(applicationId);
            if (Number(result.jobId) !== Number(jobId)) {
                setApplicationDetail(null);
                setDetailError('This application does not belong to the selected job.');
            } else {
                setApplicationDetail(result);
            }
        } catch (requestError) {
            setApplicationDetail(null);
            if (requestError.status === 401 || requestError.status === 403) {
                setDetailError('You are not authorized to view this application.');
            } else if (requestError.status === 404) {
                setDetailError('This application was not found or is not available to your recruiter profile.');
            } else {
                setDetailError(requestError.message || 'Application details could not be loaded.');
            }
        } finally {
            setDetailLoading(false);
        }
    }, [applicationId, jobId, validApplicationId, validJobId]);

    useEffect(() => {
        loadApplicants();
    }, [loadApplicants]);

    useEffect(() => {
        if (applicationId) {
            loadApplicationDetail();
        }
    }, [applicationId, loadApplicationDetail]);

    useEffect(() => {
        if (location.state?.interviewScheduled) {
            window.history.replaceState({}, document.title);
        }
    }, [location.state]);

    const statuses = useMemo(() => [...new Set(applications.map(item => item.currentStatus).filter(Boolean))].sort(), [applications]);
    const filtered = useMemo(() => {
        const term = search.trim().toLowerCase();
        return applications.filter(application => {
            const matchesSearch = !term || application.candidateName?.toLowerCase().includes(term) || application.candidateEmail?.toLowerCase().includes(term);
            return matchesSearch && (!status || application.currentStatus === status);
        });
    }, [applications, search, status]);

    const clearFilters = () => { setSearch(''); setStatus(''); };
    const listRoute = `/recruiter/jobs/${jobId}/applicants`;

    const updateApplicationStatus = async (nextStatus, comment) => {
        if (!applicationDetail || statusSubmitting) return;
        if (!window.confirm(`Change this application from ${applicationDetail.currentStatus} to ${nextStatus}?`)) return;

        setStatusSubmitting(true);
        setStatusFeedback(null);
        try {
            await applicationApi.updateRecruiterStatus(applicationDetail.applicationId, nextStatus, comment);
            await Promise.all([loadApplicationDetail(), loadApplicants()]);
            setStatusFeedback({ type: 'success', message: `Application status changed to ${nextStatus}.` });
        } catch (requestError) {
            const message = requestError.status === 401 || requestError.status === 403
                ? 'You are not authorized to update this application.'
                : requestError.status === 404
                    ? 'This application is no longer available to your recruiter profile.'
                    : requestError.status === 409 || requestError.status === 400
                        ? requestError.message
                        : 'The status could not be updated. Please try again.';
            setStatusFeedback({ type: 'error', message });
        } finally {
            setStatusSubmitting(false);
        }
    };

    const addRecruiterNote = async (comment) => {
        if (!applicationDetail || noteSubmitting) return false;

        setNoteSubmitting(true);
        setNoteFeedback(null);
        try {
            await applicationApi.addRecruiterNote(applicationDetail.applicationId, comment);
            await loadApplicationDetail();
            setNoteFeedback({ type: 'success', message: 'Recruiter note added.' });
            return true;
        } catch (requestError) {
            const message = requestError.status === 401 || requestError.status === 403
                ? 'You are not authorized to add notes to this application.'
                : requestError.status === 404
                    ? 'This application is no longer available to your recruiter profile.'
                    : requestError.status === 400
                        ? requestError.message
                        : 'The note could not be saved. Please try again.';
            setNoteFeedback({ type: 'error', message });
            return false;
        } finally {
            setNoteSubmitting(false);
        }
    };

    const handleShortlist = async (appId) => {
        setShortlisting(prev => ({ ...prev, [appId]: true }));
        setNotice(null);
        try {
            const updated = await applicationApi.updateStatus(appId, 'Shortlisted');
            setApplications(prev =>
                prev.map(app =>
                    app.applicationId === appId
                        ? { ...app, currentStatus: updated.currentStatus }
                        : app
                )
            );
            if (applicationDetail?.applicationId === appId) {
                setApplicationDetail(prev => ({ ...prev, currentStatus: updated.currentStatus }));
            }
            setNotice({ type: 'success', message: `${updated.candidateName || 'Applicant'} was shortlisted successfully.` });
        } catch (requestError) {
            setNotice({ type: 'error', message: requestError.message || 'The applicant could not be shortlisted.' });
        } finally {
            setShortlisting(prev => { const next = { ...prev }; delete next[appId]; return next; });
        }
    };

    return (
        <div className="recruiter-applicants-shell">
            <header className="recruiter-applicants-nav"><button className="recruiter-brand" onClick={() => navigate('/recruiter-dashboard')}>Skill<span>Net</span></button><button className="recruiter-nav-button" onClick={() => navigate('/recruiter-dashboard')}>Recruiter Dashboard</button></header>
            <main className="recruiter-applicants-page">
                <header className="recruiter-applicants-heading"><div><button className="recruiter-back" onClick={() => navigate('/recruiter-dashboard')}>← Back to jobs</button><span className="recruiter-eyebrow">Applicant pipeline</span><h1>{job?.title || 'Job applicants'}</h1><p>Review candidates who applied for this role and manage each application through its valid next steps.</p></div>{!loading && !error && <div className="recruiter-applicant-total"><strong>{applications.length}</strong><span>Total applicants</span></div>}</header>
                {notice && (
                    <div className={`recruiter-applicants-notice recruiter-applicants-notice--${notice.type}`} role={notice.type === 'error' ? 'alert' : 'status'}>
                        {notice.message}
                        <button onClick={() => setNotice(null)} aria-label="Dismiss message">×</button>
                    </div>
                )}
                {!loading && !error && applications.length > 0 && (
                    <div className="recruiter-ranking-disclaimer">
                        <span className="disclaimer-icon">ℹ</span>
                        <p>Applicants are ordered by required skill coverage. This score supports recruiter review and does not replace human assessment.</p>
                    </div>
                )}
                {!loading && !error && applications.length > 0 && <section className="recruiter-applicant-filters" aria-label="Applicant filters"><label><span>Search applicants</span><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Candidate name or email" /></label><label><span>Status</span><select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">All statuses</option>{statuses.map(value => <option key={value} value={value}>{value}</option>)}</select></label><button className="recruiter-button recruiter-button--ghost" onClick={clearFilters} disabled={!search && !status}>Clear filters</button></section>}
                {loading ? <div className="recruiter-applicant-list" aria-label="Loading applicants"><div className="recruiter-applicant-card recruiter-skeleton" /><div className="recruiter-applicant-card recruiter-skeleton" /></div>
                    : error ? <section className="recruiter-state recruiter-state--error" role="alert"><span>!</span><h2>Unable to load applicants</h2><p>{error}</p><button className="recruiter-button recruiter-button--primary" onClick={loadApplicants}>Retry</button></section>
                    : applications.length === 0 ? <section className="recruiter-state"><span>◇</span><h2>No applicants yet</h2><p>Applications submitted for {job?.title || 'this job'} will appear here.</p></section>
                    : filtered.length === 0 ? <section className="recruiter-state"><span>⌕</span><h2>No matching applicants</h2><p>Try a different name, email, or status.</p><button className="recruiter-button recruiter-button--ghost" onClick={clearFilters}>Clear filters</button></section>
                    : <section className="recruiter-applicant-list" aria-label="Applicants">{filtered.map(application => <ApplicantCard key={application.applicationId} application={application} onOpen={() => navigate(`${listRoute}/${application.applicationId}`)} onSchedule={() => navigate(`${listRoute}/${application.applicationId}/schedule`)} onShortlist={handleShortlist} isShortlisting={!!shortlisting[application.applicationId]} />)}</section>}
            </main>
            {applicationId && <div className="recruiter-detail-backdrop"><aside className="recruiter-application-detail" aria-label="Recruiter application details"><button className="recruiter-detail-close" onClick={() => navigate(listRoute)} aria-label="Back to applicants list" disabled={statusSubmitting || noteSubmitting}>×</button>{detailLoading ? <div className="recruiter-detail-loading recruiter-skeleton" /> : detailError ? <DetailError message={detailError} onRetry={validJobId && validApplicationId ? loadApplicationDetail : null} onBack={() => navigate(listRoute)} /> : applicationDetail && <ApplicationDetail key={`${applicationDetail.applicationId}-${applicationDetail.currentStatus}`} application={applicationDetail} onBack={() => navigate(listRoute)} onStatusChange={updateApplicationStatus} submitting={statusSubmitting} feedback={statusFeedback} onAddNote={addRecruiterNote} noteSubmitting={noteSubmitting} noteFeedback={noteFeedback} onSchedule={() => navigate(`${listRoute}/${applicationDetail.applicationId}/schedule`)} onShortlist={handleShortlist} isShortlisting={!!shortlisting[applicationDetail?.applicationId]} />}</aside></div>}
        </div>
    );
};

const ApplicantCard = ({ application, onOpen, onSchedule, onShortlist, isShortlisting }) => (
    <article className="recruiter-applicant-card">
        <div className="recruiter-applicant-avatar">{application.candidateName?.charAt(0)?.toUpperCase() || 'C'}</div>
        <div className="recruiter-applicant-main">
            <div className="recruiter-applicant-title">
                <div>
                    <h2>{application.candidateName || 'Candidate'}</h2>
                    {application.candidateEmail && <a href={`mailto:${application.candidateEmail}`}>{application.candidateEmail}</a>}
                </div>
                <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                    {application.matchScore !== null && application.matchScore !== undefined && (
                        <span className="recruiter-match-badge">
                            <span className="sr-only">Skill Match Score: </span>
                            <strong>{application.matchScore}%</strong> Match
                        </span>
                    )}
                    <span className={`recruiter-status recruiter-status--${statusClass(application.currentStatus)}`}>{application.currentStatus || 'Unknown'}</span>
                </div>
            </div>
            <dl>
                <div><dt>Applied</dt><dd>{formatDate(application.appliedDate)}</dd></div>
                <div><dt>Resume</dt><dd>{application.resumeFileName || (application.resumeId ? `Resume #${application.resumeId}` : 'Not available')}</dd></div>
                <div><dt>Cover letter</dt><dd>{application.coverLetter ? 'Included' : 'Not included'}</dd></div>
            </dl>
        </div>
        <div className="recruiter-card-actions">
            {application.currentStatus === 'Shortlisted' && <button className="recruiter-button recruiter-button--primary" onClick={onSchedule}>Schedule interview</button>}
            {application.currentStatus === 'Applied' && (
                <button
                    id={`shortlist-btn-${application.applicationId}`}
                    className="recruiter-button recruiter-button--shortlist"
                    onClick={() => onShortlist(application.applicationId)}
                    disabled={isShortlisting}
                    aria-label={`Shortlist ${application.candidateName || 'applicant'}`}
                >
                    {isShortlisting ? 'Shortlisting…' : '✓ Shortlist'}
                </button>
            )}
            <button className="recruiter-button recruiter-button--secondary" onClick={onOpen}>View details</button>
        </div>
    </article>
);

const DetailError = ({ message, onRetry, onBack }) => <section className="recruiter-detail-state" role="alert"><span>!</span><h1>Unable to load application</h1><p>{message}</p><div>{onRetry && <button className="recruiter-button recruiter-button--primary" onClick={onRetry}>Retry</button>}<button className="recruiter-button recruiter-button--ghost" onClick={onBack}>Back to applicants</button></div></section>;

const ApplicationDetail = ({ application, onBack, onStatusChange, submitting, feedback, onAddNote, noteSubmitting, noteFeedback, onSchedule, onShortlist, isShortlisting }) => {
    const [selectedStatus, setSelectedStatus] = useState('');
    const [comment, setComment] = useState('');
    const [noteComment, setNoteComment] = useState('');
    const history = Array.isArray(application.statusHistory) ? [...application.statusHistory].sort((left, right) => new Date(left.changedAt) - new Date(right.changedAt)) : [];
    const validNextStatuses = Array.isArray(application.validNextStatuses) ? application.validNextStatuses : [];
    const canShortlist = validNextStatuses.includes('Shortlisted');
    const canReject = validNextStatuses.includes('Rejected');
    const notes = Array.isArray(application.recruiterNotes) ? application.recruiterNotes : [];
    const submitNote = async (event) => {
        event.preventDefault();
        const saved = await onAddNote(noteComment.trim());
        if (saved) setNoteComment('');
    };
    return <div className="recruiter-detail-content">
        <button className="recruiter-detail-back" onClick={onBack} disabled={submitting}>← Back to applicants</button><span className="recruiter-eyebrow">Application #{application.applicationId}</span>
        <div className="recruiter-detail-heading"><div><h1>{application.jobTitle || 'Job application'}</h1><p>Submitted {formatDateTime(application.appliedDate)}</p></div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', alignItems: 'flex-end' }}>
                <span className={`recruiter-status recruiter-status--${statusClass(application.currentStatus)}`}>{application.currentStatus || 'Unknown'}</span>
                {application.currentStatus === 'Applied' && (
                    <button
                        id={`shortlist-detail-btn-${application.applicationId}`}
                        className="recruiter-button recruiter-button--shortlist"
                        onClick={() => onShortlist(application.applicationId)}
                        disabled={isShortlisting}
                        aria-label={`Shortlist ${application.candidateName || 'applicant'}`}
                    >
                        {isShortlisting ? 'Shortlisting…' : '✓ Shortlist'}
                    </button>
                )}
                {application.currentStatus === 'Shortlisted' && <button className="recruiter-button recruiter-button--primary" onClick={onSchedule}>Schedule interview</button>}
            </div>
        </div>
        <dl className="recruiter-detail-facts"><div><dt>Application ID</dt><dd>#{application.applicationId}</dd></div><div><dt>Last updated</dt><dd>{formatDateTime(application.lastUpdated)}</dd></div><div><dt>Source</dt><dd>{application.source || 'Not specified'}</dd></div></dl>
        <MatchAnalysisPanel loadAnalysis={() => matchAnalysisApi.forRecruiter(application.jobId, application.candidateId)} buttonLabel="AI Analysis" />
        
        {application.matchScore !== null && application.matchScore !== undefined && (
            <section className="recruiter-detail-card">
                <span className="recruiter-eyebrow">Skill Match</span>
                <h2>Required Skill Coverage: {application.matchScore}%</h2>
                
                {application.totalRequiredSkills === 0 ? (
                    <p className="recruiter-detail-muted" style={{ margin: 0, fontSize: '0.78rem' }}>No skills are required for this job.</p>
                ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginTop: '10px' }}>
                        {application.matchedSkills && application.matchedSkills.length > 0 ? (
                            <div>
                                <h4 style={{ fontSize: '0.75rem', margin: '0 0 4px 0', color: '#68707C' }}>Matched Required Skills:</h4>
                                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                                    {application.matchedSkills.map(s => (
                                        <span className="recruiter-match-pill recruiter-match-pill--matched" key={s}>{s}</span>
                                    ))}
                                </div>
                            </div>
                        ) : (
                            <p className="recruiter-detail-muted" style={{ margin: 0, fontSize: '0.78rem' }}>No matching required skills.</p>
                        )}

                        {application.missingSkills && application.missingSkills.length > 0 ? (
                            <div>
                                <h4 style={{ fontSize: '0.75rem', margin: '0 0 4px 0', color: '#68707C' }}>Missing Required Skills:</h4>
                                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                                    {application.missingSkills.map(s => (
                                        <span className="recruiter-match-pill recruiter-match-pill--missing" key={s}>{s}</span>
                                    ))}
                                </div>
                            </div>
                        ) : null}
                    </div>
                )}
            </section>
        )}

        <section className="recruiter-detail-card recruiter-status-management"><span className="recruiter-eyebrow">Recruiter action</span><h2>Manage application status</h2><p className="recruiter-detail-muted">Current status: <strong>{application.currentStatus}</strong></p>{feedback && <div className={`recruiter-status-feedback recruiter-status-feedback--${feedback.type}`} role={feedback.type === 'error' ? 'alert' : 'status'}>{feedback.message}</div>}{validNextStatuses.length === 0 ? <p className="recruiter-status-terminal">No further recruiter status changes are available.</p> : <><div className="recruiter-quick-actions">{canShortlist && <button className="recruiter-button recruiter-button--secondary" onClick={() => onStatusChange('Shortlisted', comment)} disabled={submitting}>Shortlist</button>}{canReject && <button className="recruiter-button recruiter-button--danger" onClick={() => onStatusChange('Rejected', comment)} disabled={submitting}>Reject</button>}</div><label className="recruiter-status-field"><span>Next status</span><select value={selectedStatus} onChange={(event) => setSelectedStatus(event.target.value)} disabled={submitting}><option value="">Select a valid next status</option>{validNextStatuses.map(value => <option key={value} value={value}>{value}</option>)}</select></label><label className="recruiter-status-field"><span>Comment <small>(optional)</small></span><textarea value={comment} onChange={(event) => setComment(event.target.value)} maxLength={2000} rows={3} disabled={submitting} placeholder="Add context to the status history" /></label><button className="recruiter-button recruiter-button--primary recruiter-status-submit" onClick={() => onStatusChange(selectedStatus, comment)} disabled={!selectedStatus || submitting}>{submitting ? 'Updating…' : 'Update status'}</button></>}</section>
        <section className="recruiter-detail-card"><div className="recruiter-detail-section-heading"><span className="recruiter-detail-avatar">{application.candidateName?.charAt(0)?.toUpperCase() || 'C'}</span><div><span className="recruiter-eyebrow">Candidate</span><h2>{application.candidateName || 'Candidate name unavailable'}</h2>{application.candidateEmail && <a href={`mailto:${application.candidateEmail}`}>{application.candidateEmail}</a>}</div></div>{application.candidateProfessionalTitle ? <p className="recruiter-candidate-title">{application.candidateProfessionalTitle}</p> : <p className="recruiter-detail-muted">No professional title was provided.</p>}</section>
        <section className="recruiter-detail-card"><span className="recruiter-eyebrow">Submitted resume</span><div className="recruiter-resume-detail"><span>PDF</span><div><h2>{application.resumeFileName || `Resume #${application.resumeId}`}</h2><dl><div><dt>File type</dt><dd>{application.resumeFileType || 'Not available'}</dd></div><div><dt>File size</dt><dd>{formatFileSize(application.resumeFileSize)}</dd></div><div><dt>Uploaded</dt><dd>{formatDateTime(application.resumeUploadedDate)}</dd></div></dl></div></div><p className="recruiter-detail-muted">Resume download is unavailable because the existing endpoint is candidate-owned.</p></section>
        <section className="recruiter-detail-card"><span className="recruiter-eyebrow">Cover letter</span><h2>Candidate message</h2>{application.coverLetter ? <p className="recruiter-cover-letter">{application.coverLetter}</p> : <div className="recruiter-inline-empty"><span>—</span><p>No cover letter was provided with this application.</p></div>}</section>
        <section className="recruiter-detail-card recruiter-notes"><span className="recruiter-eyebrow">Private recruiter workspace</span><h2>Recruiter notes</h2><p className="recruiter-detail-muted">These notes are visible only to the recruiter who manages this application.</p><form className="recruiter-note-form" onSubmit={submitNote}><label className="recruiter-status-field"><span>New note</span><textarea value={noteComment} onChange={(event) => setNoteComment(event.target.value)} maxLength={2000} rows={4} disabled={noteSubmitting} placeholder="Add an internal note about this application" /></label><div className="recruiter-note-form-footer"><small>{noteComment.length}/2000</small><button className="recruiter-button recruiter-button--primary" disabled={!noteComment.trim() || noteSubmitting}>{noteSubmitting ? 'Saving…' : 'Add note'}</button></div></form>{noteFeedback && <div className={`recruiter-status-feedback recruiter-status-feedback--${noteFeedback.type}`} role={noteFeedback.type === 'error' ? 'alert' : 'status'}>{noteFeedback.message}</div>}<div className="recruiter-note-list">{notes.length === 0 ? <div className="recruiter-inline-empty"><span>◇</span><p>No recruiter notes have been added.</p></div> : notes.map(note => <article className="recruiter-note" key={note.noteId}><div><strong>{note.recruiterName || note.recruiterEmail || 'Recruiter'}</strong><time>{formatDateTime(note.createdAt)}</time></div><p>{note.comment}</p></article>)}</div></section>
        <section className="recruiter-detail-card"><span className="recruiter-eyebrow">Application progress</span><h2>Status history</h2>{history.length === 0 ? <div className="recruiter-inline-empty"><span>◇</span><p>No status history is available for this application.</p></div> : <ol className="recruiter-history">{history.map(item => <li key={item.statusHistoryId}><span className="recruiter-history-dot" /><div className="recruiter-history-entry"><div><strong>{item.oldStatus || 'Initial submission'}</strong><span>→</span><strong>{item.newStatus}</strong></div><time>{formatDateTime(item.changedAt)}</time>{(item.changedByName || item.changedByEmail) && <p className="recruiter-history-actor">Changed by {item.changedByName || item.changedByEmail}</p>}{item.comment && <p className="recruiter-history-comment">{item.comment}</p>}</div></li>)}</ol>}</section>
    </div>;
};

export default RecruiterApplicants;
