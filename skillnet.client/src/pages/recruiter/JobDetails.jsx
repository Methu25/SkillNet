import { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';
import { recruiterApi } from '../../api/recruiterApi';

const formatDate = (value) => value
    ? new Date(value).toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' })
    : null;

const formatSalary = (minimum, maximum) => {
    const min = minimum == null ? null : Number(minimum).toLocaleString();
    const max = maximum == null ? null : Number(maximum).toLocaleString();
    if (min && max) return `${min} – ${max}`;
    if (min) return `From ${min}`;
    if (max) return `Up to ${max}`;
    return null;
};

const JobDetails = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const location = useLocation();
    const [job, setJob] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(location.state?.success || '');
    const [processing, setProcessing] = useState('');
    const [reloadKey, setReloadKey] = useState(0);
    const [duplicateId, setDuplicateId] = useState(null);

    useEffect(() => {
        let active = true;
        recruiterApi.getJob(id)
            .then((response) => {
                if (!active) return;
                setJob(response || null);
                setError('');
            })
            .catch((requestError) => {
                if (active) setError(requestError.message || 'The job could not be loaded.');
            })
            .finally(() => {
                if (active) setLoading(false);
            });
        return () => { active = false; };
    }, [id, reloadKey]);

    const refreshJob = async () => {
        const refreshed = await recruiterApi.getJob(id);
        setJob(refreshed || null);
    };

    const runAction = async (action) => {
        if (!job || processing) return;
        const confirmations = {
            publish: `Publish “${job.title}”?`,
            close: `Close “${job.title}”?`,
            delete: `Delete “${job.title}”? This action cannot be undone.`
        };
        if (confirmations[action] && !window.confirm(confirmations[action])) return;

        const operations = {
            publish: () => recruiterApi.publishJob(job.jobId),
            close: () => recruiterApi.closeJob(job.jobId),
            duplicate: () => recruiterApi.duplicateJob(job.jobId),
            delete: () => recruiterApi.deleteJob(job.jobId)
        };

        setProcessing(action);
        setError('');
        setSuccess('');
        setDuplicateId(null);
        try {
            const result = await operations[action]();
            if (action === 'delete') {
                navigate('/recruiter/jobs', { replace: true, state: { success: 'Job deleted successfully.' } });
                return;
            }

            await refreshJob();
            if (action === 'duplicate') {
                setDuplicateId(result?.jobId || null);
                setSuccess('Job duplicated successfully as a new draft.');
            } else {
                setSuccess(action === 'publish' ? 'Job published successfully.' : 'Job closed successfully.');
            }
        } catch (requestError) {
            setError(requestError.message || `The ${action} action could not be completed.`);
        } finally {
            setProcessing('');
        }
    };

    const retry = () => {
        if (loading) return;
        setLoading(true);
        setError('');
        setReloadKey((value) => value + 1);
    };

    if (loading) return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading job details...</div>;
    if (error && !job) {
        return <div className="recruiter-route-state recruiter-route-state--error"><strong>Job details could not be loaded.</strong><span>{error}</span><button type="button" onClick={retry}>Try again</button></div>;
    }
    if (!job) {
        return <div className="recruiter-route-state"><strong>Job not found</strong><span>This job is unavailable or no longer exists.</span><Link className="recruiter-primary-action" to="/recruiter/jobs">Back to jobs</Link></div>;
    }

    const salary = formatSalary(job.salaryMin, job.salaryMax);
    const busy = Boolean(processing);

    return (
        <section className="recruiter-job-details-page">
            <div className="recruiter-job-details-topbar">
                <Link to="/recruiter/jobs">← Back to jobs</Link>
                <div className="recruiter-job-details-actions">
                    {busy
                        ? <span className="is-disabled" aria-disabled="true">Edit</span>
                        : <Link to={`/recruiter/jobs/${job.jobId}/edit`}>Edit</Link>}
                    <button type="button" onClick={() => runAction('publish')} disabled={busy || job.status === 'Published'}>{processing === 'publish' ? 'Publishing...' : 'Publish'}</button>
                    <button type="button" onClick={() => runAction('close')} disabled={busy || job.status === 'Closed'}>{processing === 'close' ? 'Closing...' : 'Close'}</button>
                    <button type="button" onClick={() => runAction('duplicate')} disabled={busy}>{processing === 'duplicate' ? 'Duplicating...' : 'Duplicate'}</button>
                    <button className="recruiter-job-detail-danger" type="button" onClick={() => runAction('delete')} disabled={busy}>{processing === 'delete' ? 'Deleting...' : 'Delete'}</button>
                </div>
            </div>

            {error && <div className="recruiter-setup-alert recruiter-setup-alert--error" role="alert">{error}</div>}
            {success && <div className="recruiter-setup-alert recruiter-setup-alert--success" role="status">{success}{duplicateId && <> <Link to={`/recruiter/jobs/${duplicateId}`}>View duplicated job</Link></>}</div>}

            <article className="recruiter-job-preview">
                <header className="recruiter-job-preview-header">
                    <div>
                        <span className={`recruiter-job-status recruiter-job-status--${String(job.status).toLowerCase()}`}>{job.status}</span>
                        <h1>{job.title}</h1>
                        <p>{[job.organizationName, job.location, job.workMode].filter(Boolean).join(' · ')}</p>
                    </div>
                    <div className="recruiter-job-preview-id"><span>Job ID</span><strong>#{job.jobId}</strong></div>
                </header>

                <div className="recruiter-job-preview-body">
                    <main>
                        <section className="recruiter-job-preview-section"><h2>Job description</h2><div className="recruiter-job-description">{job.description}</div></section>
                        {job.skills?.length > 0 && <section className="recruiter-job-preview-section"><h2>Skills</h2><div className="recruiter-job-skill-list">{job.skills.map((skill) => <span key={skill}>{skill}</span>)}</div></section>}
                    </main>

                    <aside className="recruiter-job-facts">
                        <h2>Job overview</h2>
                        <dl>
                            <div><dt>Category</dt><dd>{job.categoryName}<small>Category ID: {job.categoryId}</small></dd></div>
                            <div><dt>Employment type</dt><dd>{job.employmentType}</dd></div>
                            <div><dt>Work mode</dt><dd>{job.workMode}</dd></div>
                            {job.location && <div><dt>Location</dt><dd>{job.location}</dd></div>}
                            {salary && <div><dt>Salary range</dt><dd>{salary}</dd></div>}
                            {job.experienceLevel && <div><dt>Experience level</dt><dd>{job.experienceLevel}</dd></div>}
                            {job.applicationDeadline && <div><dt>Application deadline</dt><dd>{formatDate(job.applicationDeadline)}</dd></div>}
                            <div><dt>Created</dt><dd>{formatDate(job.createdAt)}</dd></div>
                            {job.recruiterName && <div><dt>Recruiter</dt><dd>{job.recruiterName}<small>Recruiter ID: {job.recruiterId}</small></dd></div>}
                            {job.organizationName && <div><dt>Organization</dt><dd>{job.organizationName}</dd></div>}
                        </dl>
                    </aside>
                </div>
            </article>
        </section>
    );
};

export default JobDetails;
