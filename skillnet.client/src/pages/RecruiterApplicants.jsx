import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { applicationApi } from '../api/applicationApi';
import { jobApi } from '../api/jobApi';
import './RecruiterApplicants.css';

const formatDate = (value) => value
    ? new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(value))
    : 'Not available';

const statusClass = (status) => String(status || 'unknown').toLowerCase().replace(/[^a-z0-9]+/g, '-');

const RecruiterApplicants = () => {
    const navigate = useNavigate();
    const { jobId, applicationId } = useParams();
    const [job, setJob] = useState(null);
    const [applications, setApplications] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [search, setSearch] = useState('');
    const [status, setStatus] = useState('');

    const loadApplicants = useCallback(async () => {
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
                    const detail = await applicationApi.getRecruiterApplication(summary.applicationId);
                    return { ...summary, ...detail };
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
    }, [jobId]);

    useEffect(() => {
        // The route job identifier initializes the ownership-checked applicant request.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadApplicants();
    }, [loadApplicants]);

    const statuses = useMemo(() => [...new Set(applications.map(item => item.currentStatus).filter(Boolean))].sort(), [applications]);
    const filtered = useMemo(() => {
        const term = search.trim().toLowerCase();
        return applications.filter(application => {
            const matchesSearch = !term || application.candidateName?.toLowerCase().includes(term) || application.candidateEmail?.toLowerCase().includes(term);
            const matchesStatus = !status || application.currentStatus === status;
            return matchesSearch && matchesStatus;
        });
    }, [applications, search, status]);

    const selectedApplication = applications.find(item => String(item.applicationId) === applicationId);
    const filtersActive = Boolean(search || status);
    const clearFilters = () => { setSearch(''); setStatus(''); };

    return (
        <div className="recruiter-applicants-shell">
            <header className="recruiter-applicants-nav">
                <button className="recruiter-brand" onClick={() => navigate('/recruiter-dashboard')}>Skill<span>Net</span></button>
                <button className="recruiter-nav-button" onClick={() => navigate('/recruiter-dashboard')}>Recruiter Dashboard</button>
            </header>

            <main className="recruiter-applicants-page">
                <header className="recruiter-applicants-heading">
                    <div><button className="recruiter-back" onClick={() => navigate('/recruiter-dashboard')}>← Back to jobs</button><span className="recruiter-eyebrow">Applicant pipeline</span><h1>{job?.title || 'Job applicants'}</h1><p>Review candidates who applied for this role. Status changes and notes are intentionally not available in this view.</p></div>
                    {!loading && !error && <div className="recruiter-applicant-total"><strong>{applications.length}</strong><span>Total applicants</span></div>}
                </header>

                {!loading && !error && applications.length > 0 && <section className="recruiter-applicant-filters" aria-label="Applicant filters">
                    <label><span>Search applicants</span><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Candidate name or email" /></label>
                    <label><span>Status</span><select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">All statuses</option>{statuses.map(value => <option key={value} value={value}>{value}</option>)}</select></label>
                    <button className="recruiter-button recruiter-button--ghost" onClick={clearFilters} disabled={!filtersActive}>Clear filters</button>
                </section>}

                {loading ? <div className="recruiter-applicant-list" aria-label="Loading applicants"><div className="recruiter-applicant-card recruiter-skeleton" /><div className="recruiter-applicant-card recruiter-skeleton" /></div>
                    : error ? <section className="recruiter-state recruiter-state--error" role="alert"><span>!</span><h2>Unable to load applicants</h2><p>{error}</p><button className="recruiter-button recruiter-button--primary" onClick={loadApplicants}>Retry</button></section>
                    : applications.length === 0 ? <section className="recruiter-state"><span>◇</span><h2>No applicants yet</h2><p>Applications submitted for {job?.title || 'this job'} will appear here.</p></section>
                    : filtered.length === 0 ? <section className="recruiter-state"><span>⌕</span><h2>No matching applicants</h2><p>Try a different name, email, or status.</p><button className="recruiter-button recruiter-button--ghost" onClick={clearFilters}>Clear filters</button></section>
                    : <section className="recruiter-applicant-list" aria-label="Applicants">{filtered.map(application => <ApplicantCard key={application.applicationId} application={application} onOpen={() => navigate(`/recruiter/jobs/${jobId}/applicants/${application.applicationId}`)} />)}</section>}
            </main>

            {applicationId && <div className="recruiter-placeholder-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) navigate(`/recruiter/jobs/${jobId}/applicants`); }}><aside className="recruiter-placeholder"><button onClick={() => navigate(`/recruiter/jobs/${jobId}/applicants`)} aria-label="Close applicant preview">×</button><span className="recruiter-eyebrow">Applicant details</span>{selectedApplication ? <><div className="recruiter-placeholder-avatar">{selectedApplication.candidateName?.charAt(0) || 'C'}</div><h2>{selectedApplication.candidateName || 'Candidate'}</h2>{selectedApplication.candidateEmail && <p>{selectedApplication.candidateEmail}</p>}<span className={`recruiter-status recruiter-status--${statusClass(selectedApplication.currentStatus)}`}>{selectedApplication.currentStatus}</span><div className="recruiter-placeholder-message"><strong>Detailed review is the next step</strong><p>Status updates, recruiter notes, and interview actions are outside this applicants-list implementation.</p></div></> : <><h2>Applicant not found</h2><p>Return to the list and select an available applicant.</p></>}</aside></div>}
        </div>
    );
};

const ApplicantCard = ({ application, onOpen }) => <article className="recruiter-applicant-card"><div className="recruiter-applicant-avatar">{application.candidateName?.charAt(0)?.toUpperCase() || 'C'}</div><div className="recruiter-applicant-main"><div className="recruiter-applicant-title"><div><h2>{application.candidateName || 'Candidate'}</h2>{application.candidateEmail && <a href={`mailto:${application.candidateEmail}`}>{application.candidateEmail}</a>}</div><span className={`recruiter-status recruiter-status--${statusClass(application.currentStatus)}`}>{application.currentStatus || 'Unknown'}</span></div><dl><div><dt>Applied</dt><dd>{formatDate(application.appliedDate)}</dd></div><div><dt>Resume</dt><dd>{application.resumeFileName || (application.resumeId ? `Resume #${application.resumeId}` : 'Not available')}</dd></div><div><dt>Cover letter</dt><dd>{application.coverLetter ? 'Included' : 'Not included'}</dd></div></dl></div><button className="recruiter-button recruiter-button--secondary" onClick={onOpen}>View details</button></article>;

export default RecruiterApplicants;
