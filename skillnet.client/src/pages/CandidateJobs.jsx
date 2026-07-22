import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { applicationApi } from '../api/applicationApi';
import { jobApi } from '../api/jobApi';
import { resumeApi } from '../api/resumeApi';
import CandidateNavigation from '../components/candidate/CandidateNavigation';
import DashboardCard from '../components/candidate/DashboardCard';
import ApplyJobDialog from '../components/candidate/jobs/ApplyJobDialog';
import MatchAnalysisPanel from '../components/MatchAnalysisPanel';
import { matchAnalysisApi } from '../api/matchAnalysisApi';
import './CandidateDashboard.css';
import './CandidateJobs.css';

const formatDate = (value) => value
    ? new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(value))
    : 'No deadline';

const formatSalary = (minimum, maximum) => {
    if (minimum == null && maximum == null) return 'Salary not listed';
    const currency = new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
    if (minimum != null && maximum != null) return `${currency.format(minimum)} – ${currency.format(maximum)}`;
    return minimum != null ? `From ${currency.format(minimum)}` : `Up to ${currency.format(maximum)}`;
};

const isApplicable = (job) => {
    if (!job || job.status?.toLowerCase() !== 'published') return false;
    return !job.applicationDeadline || new Date(job.applicationDeadline) >= new Date();
};

const getSafeApplyError = (error) => {
    if (error.status === 401) return 'Your session has expired. Please sign in again.';
    if (error.status === 404) return 'This job or selected resume is no longer available.';
    return error.message || 'Your application could not be submitted.';
};

const CandidateJobs = () => {
    const navigate = useNavigate();
    const { jobId } = useParams();
    const [jobs, setJobs] = useState([]);
    const [keyword, setKeyword] = useState('');
    const [searchTerm, setSearchTerm] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [selectedJob, setSelectedJob] = useState(null);
    const [detailLoading, setDetailLoading] = useState(false);
    const [detailError, setDetailError] = useState('');
    const [applyJob, setApplyJob] = useState(null);
    const [resumes, setResumes] = useState([]);
    const [resumeLoading, setResumeLoading] = useState(false);
    const [resumeError, setResumeError] = useState('');
    const [submitting, setSubmitting] = useState(false);
    const [submitError, setSubmitError] = useState('');
    const [applicationSuccess, setApplicationSuccess] = useState(false);
    const [unavailableJobs, setUnavailableJobs] = useState(() => new Set());

    const loadJobs = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            const result = await jobApi.search({ keyword: searchTerm, sortBy: 'newest', page: 1, pageSize: 20 });
            setJobs(Array.isArray(result) ? result : []);
        } catch (requestError) {
            setError(requestError.message || 'Available jobs could not be loaded.');
        } finally {
            setLoading(false);
        }
    }, [searchTerm]);

    const loadJob = useCallback(async (id) => {
        setDetailLoading(true);
        setDetailError('');
        try {
            setSelectedJob(await jobApi.getById(id));
        } catch (requestError) {
            setSelectedJob(null);
            setDetailError(requestError.status === 404 ? 'This job was not found.' : requestError.message || 'Job details could not be loaded.');
        } finally {
            setDetailLoading(false);
        }
    }, []);

    useEffect(() => {
        // The job search request initializes and refreshes the listing.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadJobs();
    }, [loadJobs]);

    useEffect(() => {
        // Route changes intentionally load the selected job detail.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        if (jobId) loadJob(jobId);
        else { setSelectedJob(null); setDetailError(''); }
    }, [jobId, loadJob]);

    const openApplyDialog = async (job) => {
        setApplyJob(job);
        setResumes([]);
        setResumeError('');
        setSubmitError('');
        setApplicationSuccess(false);
        setResumeLoading(true);
        try {
            const result = await resumeApi.getAll();
            setResumes(Array.isArray(result) ? result : []);
        } catch (requestError) {
            setResumeError(requestError.status === 401 ? 'Your session has expired. Please sign in again.' : requestError.message || 'Your resumes could not be loaded.');
        } finally {
            setResumeLoading(false);
        }
    };

    const submitApplication = async (request) => {
        setSubmitting(true);
        setSubmitError('');
        try {
            await applicationApi.apply(request);
            setUnavailableJobs(current => new Set(current).add(request.jobId));
            setApplicationSuccess(true);
        } catch (requestError) {
            const message = getSafeApplyError(requestError);
            if (message.toLowerCase().includes('already applied')) {
                setUnavailableJobs(current => new Set(current).add(request.jobId));
            }
            setSubmitError(message);
        } finally {
            setSubmitting(false);
        }
    };

    const search = (event) => {
        event.preventDefault();
        setSearchTerm(keyword.trim());
    };

    return (
        <div className="candidate-dashboard-shell">
            <CandidateNavigation />
            <main className="candidate-dashboard candidate-jobs-page">
                <header className="jobs-page-heading"><div><span className="candidate-eyebrow">Explore opportunities</span><h1>Find your next role</h1><p>Browse currently published SkillNet opportunities and apply with one of your saved resumes.</p></div></header>
                <form className="job-search" onSubmit={search}><label htmlFor="job-keyword">Search jobs</label><div><input id="job-keyword" value={keyword} onChange={(event) => setKeyword(event.target.value)} placeholder="Job title, skill, or keyword" /><button className="candidate-button candidate-button--primary">Search</button></div></form>

                {loading ? <div className="job-grid"><div className="job-card job-card--skeleton skeleton" /><div className="job-card job-card--skeleton skeleton" /></div>
                    : error ? <DashboardCard className="dashboard-error"><span className="dashboard-error__icon">!</span><h2>Unable to load jobs</h2><p>{error}</p><button className="candidate-button candidate-button--primary" onClick={loadJobs}>Retry</button></DashboardCard>
                    : jobs.length === 0 ? <DashboardCard className="jobs-empty"><span>◇</span><h2>No matching jobs</h2><p>Try another keyword or check back for new opportunities.</p>{searchTerm && <button className="candidate-button candidate-button--secondary" onClick={() => { setKeyword(''); setSearchTerm(''); }}>Clear search</button>}</DashboardCard>
                    : <section className="job-grid" aria-label="Available jobs">{jobs.map(job => <JobCard key={job.jobId} job={job} onOpen={() => navigate(`/candidate/jobs/${job.jobId}`)} />)}</section>}
            </main>

            {jobId && <div className="job-detail-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) navigate('/candidate/jobs'); }}><aside className="job-detail" aria-label="Job details"><button className="job-detail__close" onClick={() => navigate('/candidate/jobs')} aria-label="Close job details">×</button>{detailLoading ? <div className="job-detail__loading skeleton" /> : detailError ? <div className="job-detail__error"><span>!</span><h2>Unable to open job</h2><p>{detailError}</p><button className="candidate-button candidate-button--primary" onClick={() => loadJob(jobId)}>Retry</button></div> : selectedJob && <JobDetails job={selectedJob} unavailable={unavailableJobs.has(selectedJob.jobId)} onApply={() => openApplyDialog(selectedJob)} />}</aside></div>}

            {applyJob && <ApplyJobDialog job={applyJob} resumes={resumes} loadingResumes={resumeLoading} loadError={resumeError} submitting={submitting} submitError={submitError} success={applicationSuccess} onClose={() => { if (!submitting) setApplyJob(null); }} onSubmit={submitApplication} onManageResumes={() => navigate('/candidate/resumes')} onViewApplications={() => navigate('/candidate/applications')} />}
        </div>
    );
};

const JobCard = ({ job, onOpen }) => (
    <article className="job-card">
        <div className="job-card__top">
            <span className="job-card__logo">{job.title.charAt(0).toUpperCase()}</span>
            <div style={{ display: 'flex', gap: '6px', alignItems: 'center' }}>
                {job.matchScore !== null && job.matchScore !== undefined && (
                    <span className="skill-match-badge">
                        <strong>{job.matchScore}%</strong> Match
                    </span>
                )}
                <span className="job-card__status">{job.status}</span>
            </div>
        </div>
        <h2>{job.title}</h2>
        <p className="job-card__organization">{job.organizationName || 'SkillNet opportunity'}</p>
        <div className="job-card__tags">
            {job.employmentType && <span>{job.employmentType}</span>}
            {job.workMode && <span>{job.workMode}</span>}
            {job.location && <span>{job.location}</span>}
        </div>
        <p className="job-card__salary">{formatSalary(job.salaryMin, job.salaryMax)}</p>
        <footer>
            <span>Deadline: {formatDate(job.applicationDeadline)}</span>
            <button className="candidate-button candidate-button--secondary" onClick={onOpen}>View Details</button>
        </footer>
    </article>
);

const JobDetails = ({ job, unavailable, onApply }) => {
    const applicable = isApplicable(job);
    return (
        <div className="job-detail__content">
            <span className="candidate-eyebrow">{job.categoryName || 'Opportunity'}</span>
            
            <div className="job-detail__heading">
                <div>
                    <h1>{job.title}</h1>
                    <p>{job.organizationName || job.recruiterName || 'SkillNet opportunity'}</p>
                </div>
                <span>{job.status}</span>
            </div>
            
            <div className="job-detail__tags">
                {job.employmentType && <span>{job.employmentType}</span>}
                {job.workMode && <span>{job.workMode}</span>}
                {job.location && <span>{job.location}</span>}
                {job.experienceLevel && <span>{job.experienceLevel}</span>}
            </div>

            {job.matchScore !== null && job.matchScore !== undefined && (
                <div style={{ marginTop: '16px', borderTop: '1px solid #EDE5DF', paddingTop: '16px' }}>
                    <h3 style={{ fontSize: '0.85rem', margin: '0 0 8px 0' }}>Required Skills Matched: {job.matchScore}%</h3>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                        {job.matchedSkills && job.matchedSkills.length > 0 && (
                            <div className="matched-skills-preview">
                                <small>Matched: </small>
                                {job.matchedSkills.map(s => <span className="match-pill match-pill--matched" key={s}>{s}</span>)}
                            </div>
                        )}
                        {job.missingSkills && job.missingSkills.length > 0 && (
                            <div className="missing-skills-preview">
                                <small>Missing: </small>
                                {job.missingSkills.map(s => <span className="match-pill match-pill--missing" key={s}>{s}</span>)}
                            </div>
                        )}
                    </div>
                </div>
            )}
            
            <dl className="job-detail__facts" style={{ marginTop: '20px' }}>
                <div><dt>Salary</dt><dd>{formatSalary(job.salaryMin, job.salaryMax)}</dd></div>
                <div><dt>Application deadline</dt><dd>{formatDate(job.applicationDeadline)}</dd></div>
                <div><dt>Posted</dt><dd>{formatDate(job.createdAt)}</dd></div>
            </dl>
            
            <section>
                <h2>About the role</h2>
                <p className="job-description">{job.description}</p>
            </section>
            
            {job.skills?.length > 0 && (
                <section>
                    <h2>Skills</h2>
                    <div className="job-detail__skills">
                        {job.skills.map(skill => <span key={skill}>{skill}</span>)}
                    </div>
                </section>
            )}
            <MatchAnalysisPanel loadAnalysis={() => matchAnalysisApi.forCandidate(job.jobId)} />
            
            <footer>
                {unavailable ? (
                    <span className="job-application-closed">You have already applied for this job.</span>
                ) : applicable ? (
                    <button className="candidate-button candidate-button--primary candidate-button--large" onClick={onApply}>Apply Now</button>
                ) : (
                    <span className="job-application-closed">Applications are not currently open for this job.</span>
                )}
            </footer>
        </div>
    );
};

export default CandidateJobs;
