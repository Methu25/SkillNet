import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { recruiterApi } from '../../api/recruiterApi';
import { useRecruiter } from '../../context/RecruiterContext';

const emptyDashboard = {
    stats: { totalJobs: 0, draftJobs: 0, publishedJobs: 0, closedJobs: 0 },
    jobs: []
};

const statCards = [
    { key: 'totalJobs', label: 'Total jobs', mark: 'TJ', tone: 'orange' },
    { key: 'draftJobs', label: 'Draft jobs', mark: 'DR', tone: 'slate' },
    { key: 'publishedJobs', label: 'Published jobs', mark: 'PB', tone: 'green' },
    { key: 'closedJobs', label: 'Closed jobs', mark: 'CL', tone: 'red' }
];

const formatDate = (value) => {
    if (!value) return 'Date unavailable';
    return new Date(value).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
};

const RecruiterDashboard = () => {
    const { organization } = useRecruiter();
    const [dashboard, setDashboard] = useState(emptyDashboard);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [reloadKey, setReloadKey] = useState(0);

    useEffect(() => {
        let active = true;

        recruiterApi.getDashboard()
            .then((response) => {
                if (!active) return;
                setDashboard({
                    stats: { ...emptyDashboard.stats, ...(response?.stats || {}) },
                    jobs: Array.isArray(response?.jobs) ? response.jobs : []
                });
                setError('');
            })
            .catch((requestError) => {
                if (active) setError(requestError.message || 'Dashboard data could not be loaded.');
            })
            .finally(() => {
                if (active) setLoading(false);
            });

        return () => { active = false; };
    }, [reloadKey]);

    const recentJobs = useMemo(() => [...dashboard.jobs]
        .sort((first, second) => new Date(second.createdAt || 0) - new Date(first.createdAt || 0))
        .slice(0, 5), [dashboard.jobs]);

    const retry = () => {
        if (loading) return;
        setLoading(true);
        setError('');
        setReloadKey((current) => current + 1);
    };

    if (loading) {
        return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading dashboard...</div>;
    }

    if (error) {
        return (
            <div className="recruiter-route-state recruiter-route-state--error">
                <strong>Dashboard data could not be loaded.</strong>
                <span>{error}</span>
                <button type="button" onClick={retry}>Try again</button>
            </div>
        );
    }

    return (
        <section className="recruiter-dashboard-page">
            <div className="recruiter-page-heading">
                <div>
                    <span className="recruiter-eyebrow">Overview</span>
                    <h2>Recruiter dashboard</h2>
                    <p>{organization?.organizationName ? `An overview of ${organization.organizationName}'s job activity.` : 'A clear view of your current job activity.'}</p>
                </div>
                <div className="recruiter-dashboard-quick-actions">
                    <Link className="recruiter-secondary-action" to="/recruiter/jobs">View jobs</Link>
                    <Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create job</Link>
                </div>
            </div>

            <div className="recruiter-stat-grid">
                {statCards.map((card) => (
                    <article className="recruiter-stat-card" key={card.key}>
                        <span className={`recruiter-stat-mark recruiter-stat-mark--${card.tone}`}>{card.mark}</span>
                        <div><strong>{dashboard.stats[card.key] ?? 0}</strong><span>{card.label}</span></div>
                    </article>
                ))}
            </div>

            <div className="recruiter-dashboard-section">
                <div className="recruiter-dashboard-section-heading">
                    <div><h3>Recent jobs</h3><p>Your five most recently created job posts.</p></div>
                    {recentJobs.length > 0 && <Link to="/recruiter/jobs">View all jobs</Link>}
                </div>

                {recentJobs.length === 0 ? (
                    <div className="recruiter-dashboard-empty">
                        <span>JOB</span>
                        <h4>No jobs yet</h4>
                        <p>Create your first draft job to start building your job pipeline.</p>
                        <Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create job</Link>
                    </div>
                ) : (
                    <div className="recruiter-recent-jobs">
                        {recentJobs.map((job) => (
                            <Link className="recruiter-recent-job" to={`/recruiter/jobs/${job.jobId}`} key={job.jobId}>
                                <div className="recruiter-recent-job-main">
                                    <strong>{job.title}</strong>
                                    <span>{[job.categoryName, job.employmentType, job.workMode].filter(Boolean).join(' · ')}</span>
                                </div>
                                <div className="recruiter-recent-job-meta">
                                    <span className={`recruiter-job-status recruiter-job-status--${String(job.status).toLowerCase()}`}>{job.status}</span>
                                    <time dateTime={job.createdAt}>{formatDate(job.createdAt)}</time>
                                </div>
                            </Link>
                        ))}
                    </div>
                )}
            </div>
        </section>
    );
};

export default RecruiterDashboard;
