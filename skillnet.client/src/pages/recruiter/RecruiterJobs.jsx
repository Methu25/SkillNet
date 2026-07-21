import { useEffect, useMemo, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { recruiterApi } from '../../api/recruiterApi';

const formatDate = (value) => value
    ? new Date(value).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
    : 'Date unavailable';

const RecruiterJobs = () => {
    const location = useLocation();
    const [jobs, setJobs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(location.state?.success || '');
    const [search, setSearch] = useState('');
    const [processing, setProcessing] = useState(null);
    const [reloadKey, setReloadKey] = useState(0);

    useEffect(() => {
        let active = true;

        recruiterApi.getDashboard()
            .then((response) => {
                if (!active) return;
                setJobs(Array.isArray(response?.jobs) ? response.jobs : []);
                setError('');
            })
            .catch((requestError) => {
                if (active) setError(requestError.message || 'Your jobs could not be loaded.');
            })
            .finally(() => {
                if (active) setLoading(false);
            });

        return () => { active = false; };
    }, [reloadKey]);

    const [activeTab, setActiveTab] = useState('All');

    const filteredJobs = useMemo(() => {
        const searchTerm = search.trim().toLowerCase();
        return jobs.filter((job) => {
            const matchesTitle = !searchTerm || job.title.toLowerCase().includes(searchTerm);
            let matchesTab = true;
            if (activeTab === 'Active') matchesTab = job.status === 'Published';
            else if (activeTab === 'Drafts') matchesTab = job.status === 'Draft';
            else if (activeTab === 'Closed') matchesTab = job.status === 'Closed';
            return matchesTitle && matchesTab;
        });
    }, [jobs, search, activeTab]);

    const refreshJobs = async () => {
        const response = await recruiterApi.getDashboard();
        setJobs(Array.isArray(response?.jobs) ? response.jobs : []);
    };

    const runAction = async (job, action) => {
        const confirmations = {
            publish: `Publish “${job.title}”?`,
            close: `Close “${job.title}”? This will prevent any new applications.`,
            delete: `Delete “${job.title}”? This action cannot be undone.`
        };
        if (confirmations[action] && !window.confirm(confirmations[action])) return;
        if (processing) return;

        const operations = {
            publish: () => recruiterApi.publishJob(job.jobId),
            close: () => recruiterApi.closeJob(job.jobId),
            duplicate: () => recruiterApi.duplicateJob(job.jobId),
            delete: () => recruiterApi.deleteJob(job.jobId)
        };
        const successMessages = {
            publish: 'Job published successfully.',
            close: 'Job closed successfully.',
            duplicate: 'Job duplicated as a new draft.',
            delete: 'Job deleted successfully.'
        };

        setProcessing(`${action}-${job.jobId}`);
        setError('');
        setSuccess('');
        try {
            await operations[action]();
            await refreshJobs();
            setSuccess(successMessages[action]);
        } catch (requestError) {
            const fallbackMessages = {
                publish: 'The job could not be published.',
                close: 'The job could not be closed.',
                duplicate: 'The job could not be duplicated.',
                delete: 'The job could not be deleted.'
            };
            setError(requestError.message || fallbackMessages[action]);
        } finally {
            setProcessing(null);
        }
    };

    const retry = () => {
        if (loading) return;
        setLoading(true);
        setError('');
        setReloadKey((current) => current + 1);
    };

    if (loading) {
        return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading your jobs...</div>;
    }

    if (error && jobs.length === 0) {
        return (
            <div className="recruiter-route-state recruiter-route-state--error">
                <strong>Your jobs could not be loaded.</strong>
                <span>{error}</span>
                <button type="button" onClick={retry}>Try again</button>
            </div>
        );
    }

    const isProcessing = Boolean(processing);

    const formatSalary = (minimum, maximum) => {
        const min = minimum == null ? null : Number(minimum).toLocaleString();
        const max = maximum == null ? null : Number(maximum).toLocaleString();
        if (min && max) return `$${min} – $${max}`;
        if (min) return `From $${min}`;
        if (max) return `Up to $${max}`;
        return 'Not specified';
    };

    return (
        <section className="recruiter-jobs-page">
            <div className="recruiter-page-heading">
                <div>
                    <span className="recruiter-eyebrow">Job management</span>
                    <h2>Your jobs</h2>
                    <p>Create, review, publish, and close your organization’s job posts.</p>
                </div>
                <Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create job</Link>
            </div>

            {error && <div className="recruiter-setup-alert recruiter-setup-alert--error" role="alert">{error}</div>}
            {success && <div className="recruiter-setup-alert recruiter-setup-alert--success" role="status">{success}</div>}

            <div className="recruiter-jobs-toolbar-card">
                <div className="recruiter-jobs-toolbar">
                    <label className="recruiter-job-search">
                        <span className="recruiter-visually-hidden">Search jobs by title</span>
                        <input type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search by job title" />
                    </label>
                </div>

                {/* Status Tabs */}
                <div className="recruiter-jobs-tabs">
                    {['All', 'Active', 'Drafts', 'Closed'].map((tab) => (
                        <button
                            key={tab}
                            type="button"
                            className={`recruiter-jobs-tab-btn ${activeTab === tab ? 'is-active' : ''}`}
                            onClick={() => setActiveTab(tab)}
                        >
                            {tab}
                        </button>
                    ))}
                </div>
            </div>

            {jobs.length === 0 ? (
                <div className="recruiter-dashboard-empty mt-6">
                    <span>JOB</span>
                    <h4>No jobs yet</h4>
                    <p>Create your first draft job to start recruiting.</p>
                    <Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create job</Link>
                </div>
            ) : filteredJobs.length === 0 ? (
                <div className="recruiter-jobs-no-results mt-6">
                    <strong>No matching jobs</strong>
                    <span>Try a different title or select another status tab.</span>
                </div>
            ) : (
                <div className="recruiter-jobs-grid">
                    {filteredJobs.map((job) => {
                        const isJobDraft = job.status === 'Draft';
                        const isJobActive = job.status === 'Published';
                        const isJobClosed = job.status === 'Closed';

                        return (
                            <article className="recruiter-job-card" key={job.jobId}>
                                <header className="recruiter-job-card-header">
                                    <div>
                                        <span className={`recruiter-job-status recruiter-job-status--${String(job.status).toLowerCase()}`}>
                                            {job.status === 'Published' ? 'Active' : job.status}
                                        </span>
                                        <h3 className="recruiter-job-card-title">
                                            <Link to={`/recruiter/jobs/${job.jobId}`}>{job.title}</Link>
                                        </h3>
                                        <p className="recruiter-job-card-category">{job.categoryName || 'General Category'}</p>
                                    </div>
                                    <div className="recruiter-job-card-id">#{job.jobId}</div>
                                </header>

                                <div className="recruiter-job-card-body">
                                    <p className="recruiter-job-card-desc">
                                        {job.description ? (job.description.length > 120 ? `${job.description.slice(0, 120)}...` : job.description) : 'No description provided.'}
                                    </p>

                                    <div className="recruiter-job-card-meta-grid">
                                        <div><strong>Type:</strong> <span>{job.employmentType || 'N/A'}</span></div>
                                        <div><strong>Mode:</strong> <span>{job.workMode || 'N/A'}</span></div>
                                        <div><strong>Location:</strong> <span>{job.location || 'Remote'}</span></div>
                                        <div><strong>Salary:</strong> <span>{formatSalary(job.salaryMin, job.salaryMax)}</span></div>
                                        {job.applicationDeadline && (
                                            <div><strong>Deadline:</strong> <span>{formatDate(job.applicationDeadline)}</span></div>
                                        )}
                                        <div><strong>Updated:</strong> <span>{formatDate(job.createdAt)}</span></div>
                                    </div>
                                </div>

                                <footer className="recruiter-job-card-footer">
                                    <div className="recruiter-job-card-actions">
                                        {/* Common actions */}
                                        <Link to={`/recruiter/jobs/${job.jobId}`} className="recruiter-card-action">View</Link>

                                        {/* Status specific actions */}
                                        {isJobDraft && (
                                            <>
                                                <Link to={`/recruiter/jobs/${job.jobId}/edit`} className="recruiter-card-action">Edit</Link>
                                                <button
                                                    type="button"
                                                    className="recruiter-card-action"
                                                    onClick={() => runAction(job, 'publish')}
                                                    disabled={isProcessing}
                                                >
                                                    {processing === `publish-${job.jobId}` ? 'Publishing...' : 'Publish'}
                                                </button>
                                                <button
                                                    type="button"
                                                    className="recruiter-card-action recruiter-card-action--danger"
                                                    onClick={() => runAction(job, 'delete')}
                                                    disabled={isProcessing}
                                                >
                                                    {processing === `delete-${job.jobId}` ? 'Deleting...' : 'Delete'}
                                                </button>
                                            </>
                                        )}

                                        {isJobActive && (
                                            <>
                                                <Link to={`/recruiter/jobs/${job.jobId}/edit`} className="recruiter-card-action">Edit</Link>
                                                <button
                                                    type="button"
                                                    className="recruiter-card-action"
                                                    onClick={() => runAction(job, 'close')}
                                                    disabled={isProcessing}
                                                >
                                                    {processing === `close-${job.jobId}` ? 'Closing...' : 'Close'}
                                                </button>
                                                <Link to={`/recruiter/jobs/${job.jobId}/applicants`} className="recruiter-card-action">Applications</Link>
                                            </>
                                        )}

                                        {isJobClosed && (
                                            <Link to={`/recruiter/jobs/${job.jobId}/applicants`} className="recruiter-card-action">Applications</Link>
                                        )}
                                    </div>
                                </footer>
                            </article>
                        );
                    })}
                </div>
            )}
        </section>
    );
};

export default RecruiterJobs;
