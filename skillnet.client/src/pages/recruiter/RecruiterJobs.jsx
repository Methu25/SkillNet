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
    const [status, setStatus] = useState('All');
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

    const statuses = useMemo(() => [...new Set(jobs.map((job) => job.status).filter(Boolean))], [jobs]);
    const filteredJobs = useMemo(() => {
        const searchTerm = search.trim().toLowerCase();
        return jobs.filter((job) => {
            const matchesTitle = !searchTerm || job.title.toLowerCase().includes(searchTerm);
            const matchesStatus = status === 'All' || job.status === status;
            return matchesTitle && matchesStatus;
        });
    }, [jobs, search, status]);

    const refreshJobs = async () => {
        const response = await recruiterApi.getDashboard();
        setJobs(Array.isArray(response?.jobs) ? response.jobs : []);
    };

    const runAction = async (job, action) => {
        const confirmations = {
            publish: `Publish “${job.title}”?`,
            close: `Close “${job.title}”?`,
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

    return (
        <section className="recruiter-jobs-page">
            <div className="recruiter-page-heading">
                <div><span className="recruiter-eyebrow">Job management</span><h2>Your jobs</h2><p>Create, review, publish, and close your organization’s job posts.</p></div>
                <Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create job</Link>
            </div>

            {error && <div className="recruiter-setup-alert recruiter-setup-alert--error" role="alert">{error}</div>}
            {success && <div className="recruiter-setup-alert recruiter-setup-alert--success" role="status">{success}</div>}

            <div className="recruiter-jobs-card">
                <div className="recruiter-jobs-toolbar">
                    <label className="recruiter-job-search">
                        <span className="recruiter-visually-hidden">Search jobs by title</span>
                        <input type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search by job title" />
                    </label>
                    <label className="recruiter-job-filter">
                        <span>Status</span>
                        <select value={status} onChange={(event) => setStatus(event.target.value)}>
                            <option value="All">All statuses</option>
                            {statuses.map((jobStatus) => <option value={jobStatus} key={jobStatus}>{jobStatus}</option>)}
                        </select>
                    </label>
                </div>

                {jobs.length === 0 ? (
                    <div className="recruiter-dashboard-empty">
                        <span>JOB</span><h4>No jobs yet</h4><p>Create your first draft job to start recruiting.</p>
                        <Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create job</Link>
                    </div>
                ) : filteredJobs.length === 0 ? (
                    <div className="recruiter-jobs-no-results"><strong>No matching jobs</strong><span>Try a different title or status filter.</span></div>
                ) : (
                    <div className="recruiter-jobs-table-wrap">
                        <table className="recruiter-jobs-table">
                            <caption className="recruiter-visually-hidden">Recruiter-owned jobs</caption>
                            <thead><tr><th>Job</th><th>Status</th><th>Category</th><th>Location</th><th>Created</th><th>Actions</th></tr></thead>
                            <tbody>
                                {filteredJobs.map((job) => (
                                    <tr key={job.jobId}>
                                        <td data-label="Job"><Link className="recruiter-job-title" to={`/recruiter/jobs/${job.jobId}`}>{job.title}</Link></td>
                                        <td data-label="Status"><span className={`recruiter-job-status recruiter-job-status--${String(job.status).toLowerCase()}`}>{job.status}</span></td>
                                        <td data-label="Category">{job.categoryName || 'Not specified'}</td>
                                        <td data-label="Location">{job.location || 'Not specified'}</td>
                                        <td data-label="Created"><time dateTime={job.createdAt}>{formatDate(job.createdAt)}</time></td>
                                        <td data-label="Actions">
                                            <div className="recruiter-job-actions">
                                                <Link to={`/recruiter/jobs/${job.jobId}`}>View</Link>
                                                <Link to={`/recruiter/jobs/${job.jobId}/edit`}>Edit</Link>
                                                <button type="button" onClick={() => runAction(job, 'publish')} disabled={isProcessing || job.status === 'Published'}>Publish</button>
                                                <button type="button" onClick={() => runAction(job, 'close')} disabled={isProcessing || job.status === 'Closed'}>Close</button>
                                                <button type="button" onClick={() => runAction(job, 'duplicate')} disabled={isProcessing}>{processing === `duplicate-${job.jobId}` ? 'Duplicating...' : 'Duplicate'}</button>
                                                <button className="recruiter-job-action--danger" type="button" onClick={() => runAction(job, 'delete')} disabled={isProcessing}>{processing === `delete-${job.jobId}` ? 'Deleting...' : 'Delete'}</button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </section>
    );
};

export default RecruiterJobs;
