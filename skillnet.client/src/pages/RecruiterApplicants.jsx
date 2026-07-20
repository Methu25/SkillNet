import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { applicationApi } from '../api/applicationApi';
import { jobApi } from '../api/jobApi';
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
    const { jobId, applicationId } = useParams();
    const [job, setJob] = useState(null);
    const [applications, setApplications] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [search, setSearch] = useState('');
    const [status, setStatus] = useState('');
    const [applicationDetail, setApplicationDetail] = useState(null);
    const [detailLoading, setDetailLoading] = useState(Boolean(applicationId));
    const [detailError, setDetailError] = useState('');
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
        // The route job identifier initializes the ownership-checked applicant request.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadApplicants();
    }, [loadApplicants]);

    useEffect(() => {
        if (applicationId) {
            // The detail route intentionally initializes its ownership-checked request.
            // eslint-disable-next-line react-hooks/set-state-in-effect
            loadApplicationDetail();
        }
    }, [applicationId, loadApplicationDetail]);

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

    return (
        <div className="recruiter-applicants-shell">
            <header className="recruiter-applicants-nav"><button className="recruiter-brand" onClick={() => navigate('/recruiter-dashboard')}>Skill<span>Net</span></button><button className="recruiter-nav-button" onClick={() => navigate('/recruiter-dashboard')}>Recruiter Dashboard</button></header>
            <main className="recruiter-applicants-page">
                <header className="recruiter-applicants-heading"><div><button className="recruiter-back" onClick={() => navigate('/recruiter-dashboard')}>← Back to jobs</button><span className="recruiter-eyebrow">Applicant pipeline</span><h1>{job?.title || 'Job applicants'}</h1><p>Review candidates who applied for this role. Status changes and notes are intentionally not available in this view.</p></div>{!loading && !error && <div className="recruiter-applicant-total"><strong>{applications.length}</strong><span>Total applicants</span></div>}</header>
                {!loading && !error && applications.length > 0 && <section className="recruiter-applicant-filters" aria-label="Applicant filters"><label><span>Search applicants</span><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Candidate name or email" /></label><label><span>Status</span><select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">All statuses</option>{statuses.map(value => <option key={value} value={value}>{value}</option>)}</select></label><button className="recruiter-button recruiter-button--ghost" onClick={clearFilters} disabled={!search && !status}>Clear filters</button></section>}
                {loading ? <div className="recruiter-applicant-list" aria-label="Loading applicants"><div className="recruiter-applicant-card recruiter-skeleton" /><div className="recruiter-applicant-card recruiter-skeleton" /></div>
                    : error ? <section className="recruiter-state recruiter-state--error" role="alert"><span>!</span><h2>Unable to load applicants</h2><p>{error}</p><button className="recruiter-button recruiter-button--primary" onClick={loadApplicants}>Retry</button></section>
                    : applications.length === 0 ? <section className="recruiter-state"><span>◇</span><h2>No applicants yet</h2><p>Applications submitted for {job?.title || 'this job'} will appear here.</p></section>
                    : filtered.length === 0 ? <section className="recruiter-state"><span>⌕</span><h2>No matching applicants</h2><p>Try a different name, email, or status.</p><button className="recruiter-button recruiter-button--ghost" onClick={clearFilters}>Clear filters</button></section>
                    : <section className="recruiter-applicant-list" aria-label="Applicants">{filtered.map(application => <ApplicantCard key={application.applicationId} application={application} onOpen={() => navigate(`${listRoute}/${application.applicationId}`)} />)}</section>}
            </main>
            {applicationId && <div className="recruiter-detail-backdrop"><aside className="recruiter-application-detail" aria-label="Recruiter application details"><button className="recruiter-detail-close" onClick={() => navigate(listRoute)} aria-label="Back to applicants list">×</button>{detailLoading ? <div className="recruiter-detail-loading recruiter-skeleton" /> : detailError ? <DetailError message={detailError} onRetry={validJobId && validApplicationId ? loadApplicationDetail : null} onBack={() => navigate(listRoute)} /> : applicationDetail && <ApplicationDetail application={applicationDetail} onBack={() => navigate(listRoute)} />}</aside></div>}
        </div>
    );
};

const ApplicantCard = ({ application, onOpen }) => <article className="recruiter-applicant-card"><div className="recruiter-applicant-avatar">{application.candidateName?.charAt(0)?.toUpperCase() || 'C'}</div><div className="recruiter-applicant-main"><div className="recruiter-applicant-title"><div><h2>{application.candidateName || 'Candidate'}</h2>{application.candidateEmail && <a href={`mailto:${application.candidateEmail}`}>{application.candidateEmail}</a>}</div><span className={`recruiter-status recruiter-status--${statusClass(application.currentStatus)}`}>{application.currentStatus || 'Unknown'}</span></div><dl><div><dt>Applied</dt><dd>{formatDate(application.appliedDate)}</dd></div><div><dt>Resume</dt><dd>{application.resumeFileName || (application.resumeId ? `Resume #${application.resumeId}` : 'Not available')}</dd></div><div><dt>Cover letter</dt><dd>{application.coverLetter ? 'Included' : 'Not included'}</dd></div></dl></div><button className="recruiter-button recruiter-button--secondary" onClick={onOpen}>View details</button></article>;

const DetailError = ({ message, onRetry, onBack }) => <section className="recruiter-detail-state" role="alert"><span>!</span><h1>Unable to load application</h1><p>{message}</p><div>{onRetry && <button className="recruiter-button recruiter-button--primary" onClick={onRetry}>Retry</button>}<button className="recruiter-button recruiter-button--ghost" onClick={onBack}>Back to applicants</button></div></section>;

const ApplicationDetail = ({ application, onBack }) => {
    const history = Array.isArray(application.statusHistory) ? [...application.statusHistory].sort((left, right) => new Date(left.changedAt) - new Date(right.changedAt)) : [];
    return <div className="recruiter-detail-content">
        <button className="recruiter-detail-back" onClick={onBack}>← Back to applicants</button><span className="recruiter-eyebrow">Application #{application.applicationId}</span>
        <div className="recruiter-detail-heading"><div><h1>{application.jobTitle || 'Job application'}</h1><p>Submitted {formatDateTime(application.appliedDate)}</p></div><span className={`recruiter-status recruiter-status--${statusClass(application.currentStatus)}`}>{application.currentStatus || 'Unknown'}</span></div>
        <dl className="recruiter-detail-facts"><div><dt>Application ID</dt><dd>#{application.applicationId}</dd></div><div><dt>Last updated</dt><dd>{formatDateTime(application.lastUpdated)}</dd></div><div><dt>Source</dt><dd>{application.source || 'Not specified'}</dd></div></dl>
        <section className="recruiter-detail-card"><div className="recruiter-detail-section-heading"><span className="recruiter-detail-avatar">{application.candidateName?.charAt(0)?.toUpperCase() || 'C'}</span><div><span className="recruiter-eyebrow">Candidate</span><h2>{application.candidateName || 'Candidate name unavailable'}</h2>{application.candidateEmail && <a href={`mailto:${application.candidateEmail}`}>{application.candidateEmail}</a>}</div></div>{application.candidateProfessionalTitle ? <p className="recruiter-candidate-title">{application.candidateProfessionalTitle}</p> : <p className="recruiter-detail-muted">No professional title was provided.</p>}</section>
        <section className="recruiter-detail-card"><span className="recruiter-eyebrow">Submitted resume</span><div className="recruiter-resume-detail"><span>PDF</span><div><h2>{application.resumeFileName || `Resume #${application.resumeId}`}</h2><dl><div><dt>File type</dt><dd>{application.resumeFileType || 'Not available'}</dd></div><div><dt>File size</dt><dd>{formatFileSize(application.resumeFileSize)}</dd></div><div><dt>Uploaded</dt><dd>{formatDateTime(application.resumeUploadedDate)}</dd></div></dl></div></div><p className="recruiter-detail-muted">Resume download is unavailable because the existing endpoint is candidate-owned.</p></section>
        <section className="recruiter-detail-card"><span className="recruiter-eyebrow">Cover letter</span><h2>Candidate message</h2>{application.coverLetter ? <p className="recruiter-cover-letter">{application.coverLetter}</p> : <div className="recruiter-inline-empty"><span>—</span><p>No cover letter was provided with this application.</p></div>}</section>
        <section className="recruiter-detail-card"><span className="recruiter-eyebrow">Application progress</span><h2>Status history</h2>{history.length === 0 ? <div className="recruiter-inline-empty"><span>◇</span><p>No status history is available for this application.</p></div> : <ol className="recruiter-history">{history.map(item => <li key={item.statusHistoryId}><span className="recruiter-history-dot" /><div className="recruiter-history-entry"><div><strong>{item.oldStatus || 'Initial submission'}</strong><span>→</span><strong>{item.newStatus}</strong></div><time>{formatDateTime(item.changedAt)}</time>{(item.changedByName || item.changedByEmail) && <p className="recruiter-history-actor">Changed by {item.changedByName || item.changedByEmail}</p>}{item.comment && <p className="recruiter-history-comment">{item.comment}</p>}</div></li>)}</ol>}</section>
    </div>;
};

export default RecruiterApplicants;
