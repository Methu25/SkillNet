import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { buildAssetUrl } from '../../api/apiClient';
import { recruiterApi } from '../../api/recruiterApi';
import { useRecruiter } from '../../context/RecruiterContext';

const emptyDashboard = {
    stats: { totalJobs: 0, draftJobs: 0, publishedJobs: 0, closedJobs: 0 },
    jobs: []
};

const statCards = [
    { key: 'totalJobs', label: 'Total Jobs', mark: 'TJ' },
    { key: 'draftJobs', label: 'Draft Jobs', mark: 'DR' },
    { key: 'publishedJobs', label: 'Active Jobs', mark: 'AJ' }
];

const quickActions = [
    ['/recruiter/jobs/create', 'Create Job', 'Start a new job posting'],
    ['/recruiter/company', 'Organization Profile', 'Review company details'],
    ['/recruiter/jobs', 'Manage Jobs', 'View all job postings'],
    ['/recruiter/settings', 'Settings', 'Update recruiter details']
];

const formatDate = (value) => {
    if (!value) return 'Date unavailable';
    return new Date(value).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
};

const getOrganizationCompletion = (organization) => {
    const values = [
        organization?.organizationName,
        organization?.industry,
        organization?.website,
        organization?.address,
        organization?.logo
    ];
    const completed = values.filter((value) => Boolean(String(value || '').trim())).length;
    return Math.round((completed / values.length) * 100);
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

    if (loading) return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading dashboard...</div>;
    if (error) return <div className="recruiter-route-state recruiter-route-state--error"><strong>Dashboard data could not be loaded.</strong><span>{error}</span><button type="button" onClick={retry}>Try again</button></div>;

    const logoUrl = organization?.logo ? buildAssetUrl(organization.logo) : null;
    const organizationInitials = organization?.organizationName
        ? organization.organizationName.split(/\s+/).slice(0, 2).map((word) => word[0]).join('').toUpperCase()
        : 'SN';
    const organizationCompletion = getOrganizationCompletion(organization);

    return (
        <section className="recruiter-dashboard-page">
            <div className="recruiter-dashboard-welcome">
                <div><span className="recruiter-eyebrow">Dashboard</span><h2>Welcome to your recruiter workspace</h2><p>Manage your organization profile and job postings in one place.</p></div>
                <Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create Job</Link>
            </div>

            <article className={`recruiter-organization-hero${organization ? '' : ' is-incomplete'}`}>
                <div className="recruiter-organization-logo">
                    {logoUrl ? <img src={logoUrl} alt={`${organization.organizationName} logo`} /> : <span>{organizationInitials}</span>}
                </div>
                <div className="recruiter-organization-copy">
                    <span className="recruiter-card-kicker">Organization</span>
                    <h3>{organization?.organizationName || 'Complete Organization Profile'}</h3>
                    {organization ? (
                        <>
                            <p className="recruiter-organization-industry">{organization.industry || 'Industry not added'}</p>
                            <p className="recruiter-organization-location">{organization.address || 'Location not added'}</p>
                            <div className="recruiter-dashboard-completion">
                                <div><span>Profile completion</span><strong>{organizationCompletion}%</strong></div>
                                <div className="recruiter-dashboard-completion-track" role="progressbar" aria-label="Organization profile completion" aria-valuemin="0" aria-valuemax="100" aria-valuenow={organizationCompletion}><span style={{ width: `${organizationCompletion}%` }} /></div>
                            </div>
                        </>
                    ) : (
                        <p className="recruiter-organization-description">Your organization profile is optional. Complete it whenever you are ready—job posting remains available.</p>
                    )}
                </div>
                <Link className="recruiter-secondary-action" to="/recruiter/setup">{organization ? 'Edit Organization Profile' : 'Complete Organization Profile'}</Link>
            </article>

            <div className="recruiter-dashboard-grid">
                <div className="recruiter-dashboard-primary">
                    <div className="recruiter-stat-grid recruiter-stat-grid--three">
                        {statCards.map((card) => (
                            <article className="recruiter-stat-card" key={card.key}>
                                <span className="recruiter-stat-mark">{card.mark}</span>
                                <div><strong>{dashboard.stats[card.key] ?? 0}</strong><span>{card.label}</span></div>
                            </article>
                        ))}
                    </div>

                    <section className="recruiter-dashboard-section recruiter-recent-section">
                        <div className="recruiter-dashboard-section-heading"><div><h3>Recent Job Postings</h3><p>Your latest recruiter-owned job posts.</p></div>{recentJobs.length > 0 && <Link to="/recruiter/jobs">View all</Link>}</div>
                        {recentJobs.length === 0 ? (
                            <div className="recruiter-dashboard-empty"><span>JOB</span><h4>No job postings yet</h4><p>Create your first job posting when you are ready to start recruiting.</p><Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create Job</Link></div>
                        ) : (
                            <div className="recruiter-recent-jobs">
                                {recentJobs.map((job) => (
                                    <article className="recruiter-recent-job" key={job.jobId}>
                                        <div className="recruiter-recent-job-main"><strong>{job.title}</strong><span>{job.location || 'Location not specified'}</span></div>
                                        <div className="recruiter-recent-job-meta"><span className={`recruiter-job-status recruiter-job-status--${String(job.status || 'draft').toLowerCase()}`}>{job.status || 'Draft'}</span><time dateTime={job.createdAt}>{formatDate(job.createdAt)}</time></div>
                                        <div className="recruiter-recent-job-actions"><Link to={`/recruiter/jobs/${job.jobId}`}>View</Link><Link to={`/recruiter/jobs/${job.jobId}/edit`}>Edit</Link></div>
                                    </article>
                                ))}
                            </div>
                        )}
                    </section>
                </div>

                <aside className="recruiter-dashboard-rail">
                    <section className="recruiter-management-card"><span className="recruiter-card-kicker">Job management</span><h3>Manage your hiring activity</h3><p>Create a new role or review all existing job postings.</p><Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create Job</Link><Link className="recruiter-secondary-action" to="/recruiter/jobs">Manage Jobs</Link></section>
                    <section className="recruiter-quick-panel"><h3>Quick actions</h3><div className="recruiter-quick-list">{quickActions.map(([to, label, hint]) => <Link to={to} key={to}><span><strong>{label}</strong><small>{hint}</small></span><b aria-hidden="true">→</b></Link>)}</div></section>
                </aside>
            </div>
        </section>
    );
};

export default RecruiterDashboard;
