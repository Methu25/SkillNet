import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { hiringApi } from '../api/hiringApi';
import './HiringDashboard.css';

const formatDateTime = value => value ? new Date(value).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' }) : 'Not scheduled';

function HiringDashboard() {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const [interviews, setInterviews] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const initials = user
        ? (`${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`.toUpperCase() || user.email?.[0]?.toUpperCase() || 'HM')
        : '';

    const handleLogout = async () => {
        await logout();
        navigate('/login');
    };

    useEffect(() => {
        hiringApi.getAssignedInterviews()
            .then(data => setInterviews(Array.isArray(data) ? data : []))
            .catch(requestError => setError(requestError.message || 'Assigned interviews could not be loaded.'))
            .finally(() => setLoading(false));
    }, []);

    const stats = useMemo(() => ({
        total: interviews.length,
        awaitingEvaluation: interviews.filter(item => !item.hasEvaluation && (item.applicationStatus === 'Interviewing' || item.status === 'Scheduled' || item.status === 'Interviewing')).length,
        evaluated: interviews.filter(item => item.hasEvaluation).length
    }), [interviews]);

    return (
        <main className="hiring-dashboard-page">
            <header className="dashboard-header">
                <div className="header-left">
                    <span className="eyebrow">Hiring Workspace</span>
                    <h1>Assigned Interviews</h1>
                    <p>Evaluate only the candidates assigned to you.</p>
                </div>
                <div className="header-right">
                    {user ? (
                        <div className="user-profile-badge">
                            <div className="user-avatar">{initials}</div>
                            <div className="user-info">
                                <strong className="user-name">
                                    {user.firstName ? `${user.firstName} ${user.lastName || ''}`.trim() : (user.email || 'Hiring Manager')}
                                </strong>
                                <span className="user-email">{user.email || 'Hiring Manager'}</span>
                            </div>
                            <button type="button" className="logout-button" onClick={handleLogout}>Log out</button>
                        </div>
                    ) : (
                        <Link to="/login" className="login-button">Log in</Link>
                    )}
                </div>
            </header>

            <section className="stats-grid" aria-label="Interview summary">
                <article className="stat-card">
                    <span className="stat-label">Assigned</span>
                    <strong className="stat-value">{stats.total}</strong>
                </article>
                <article className="stat-card">
                    <span className="stat-label">Awaiting evaluation</span>
                    <strong className="stat-value accent">{stats.awaitingEvaluation}</strong>
                </article>
                <article className="stat-card">
                    <span className="stat-label">Evaluated</span>
                    <strong className="stat-value success">{stats.evaluated}</strong>
                </article>
            </section>

            {loading ? (
                <section className="dashboard-state loading-state">
                    <div className="spinner"></div>
                    <p>Loading assigned interviews...</p>
                </section>
            ) : error ? (
                <section className="dashboard-state error-state" role="alert">
                    <div className="state-icon">⚠️</div>
                    <h3>Error Loading Interviews</h3>
                    <p>{error}</p>
                </section>
            ) : interviews.length === 0 ? (
                <section className="dashboard-state empty-state">
                    <div className="state-icon">📋</div>
                    <h2>No assigned interviews</h2>
                    <p>New assignments will appear here once candidate interviews are assigned to you.</p>
                </section>
            ) : (
                <section className="interviews-grid" aria-label="Assigned interviews">
                    {interviews.map(interview => (
                        <article className="interview-card" key={interview.interviewId}>
                            <div className="card-header">
                                <span className={`status-badge ${(interview.applicationStatus || interview.status || '').toLowerCase()}`}>
                                    {interview.applicationStatus || interview.status || 'Scheduled'}
                                </span>
                                <h2>{interview.candidateName || 'Candidate'}</h2>
                                <p className="job-title">{interview.jobTitle || 'Job title unavailable'}</p>
                            </div>
                            <dl className="interview-details">
                                <div>
                                    <dt>Scheduled</dt>
                                    <dd>{formatDateTime(interview.scheduledDate)}</dd>
                                </div>
                                <div>
                                    <dt>Type</dt>
                                    <dd>{interview.interviewType || 'Standard'}</dd>
                                </div>
                                <div>
                                    <dt>Evaluation</dt>
                                    <dd className={interview.hasEvaluation ? 'status-submitted' : 'status-pending'}>
                                        {interview.hasEvaluation ? 'Submitted' : 'Pending'}
                                    </dd>
                                </div>
                            </dl>
                            <Link className="primary-button" to={`/interviews/${interview.interviewId}`}>
                                {interview.hasEvaluation ? 'View evaluation' : 'View / Evaluate'}
                            </Link>
                        </article>
                    ))}
                </section>
            )}
        </main>
    );
}

export default HiringDashboard;
