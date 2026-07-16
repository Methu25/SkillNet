import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { candidateApi } from '../api/candidateApi';
import { useAuth } from '../context/AuthContext';
import CandidateDashboardSkeleton from '../components/candidate/CandidateDashboardSkeleton';
import DashboardCard from '../components/candidate/DashboardCard';
import ProfileCompletionCard from '../components/candidate/ProfileCompletionCard';
import ProfileAvatar from '../components/candidate/profile-image/ProfileAvatar';
import './CandidateDashboard.css';

const formatDate = (value) => value
    ? new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(value))
    : 'Not available';

const CandidateDashboard = () => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const [dashboard, setDashboard] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const loadDashboard = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            setDashboard(await candidateApi.getDashboard());
        } catch (requestError) {
            setError(requestError.message || 'We could not load your dashboard.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        // The dashboard request intentionally initializes page state on mount.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadDashboard();
    }, [loadDashboard]);

    if (loading) return <CandidateDashboardSkeleton />;

    if (error) {
        return (
            <main className="candidate-dashboard candidate-dashboard--centered">
                <DashboardCard className="dashboard-error">
                    <span className="dashboard-error__icon">!</span>
                    <h1>Something went wrong</h1>
                    <p>{error}</p>
                    <button className="candidate-button candidate-button--primary" onClick={loadDashboard}>Retry</button>
                </DashboardCard>
            </main>
        );
    }

    const profile = dashboard?.profile || {};
    const completion = dashboard?.profileCompletion || {};
    const fullName = profile.fullName || `${user?.firstName || ''} ${user?.lastName || ''}`.trim() || 'SkillNet Candidate';

    return (
        <div className="candidate-dashboard-shell">
            <header className="candidate-topbar">
                <button className="candidate-brand" onClick={() => navigate('/candidate/dashboard')}>Skill<span>Net</span></button>
                <nav className="candidate-topbar__actions" aria-label="Account actions">
                    <button className="icon-button" aria-label="Notifications" title="Notifications coming soon">♡</button>
                    <button className="icon-button" aria-label="Settings" title="Settings coming soon">⚙</button>
                    <button className="candidate-button candidate-button--ghost" onClick={logout}>Logout</button>
                </nav>
            </header>

            <main className="candidate-dashboard">
                <section className="candidate-hero">
                    <ProfileAvatar imagePath={profile.profileImagePath} name={fullName} large />
                    <div className="candidate-hero__copy">
                        <span className="candidate-eyebrow">Candidate workspace</span>
                        <h1>{fullName}</h1>
                        <p className="candidate-hero__title">{profile.professionalTitle || 'Build your professional presence'}</p>
                        <p>{profile.degree || profile.education || 'Add your education to help recruiters understand your background.'}</p>
                        {profile.professionalSummary && <p className="candidate-hero__summary">{profile.professionalSummary}</p>}
                    </div>
                </section>

                {!dashboard?.hasProfile ? (
                    <div className="first-time-layout">
                        <DashboardCard className="welcome-card">
                            <span className="welcome-card__mark">✦</span>
                            <span className="candidate-eyebrow">Welcome aboard</span>
                            <h2>Welcome to SkillNet</h2>
                            <p>{dashboard?.welcomeMessage || 'Start your professional journey by creating your profile.'}</p>
                            <button className="candidate-button candidate-button--primary candidate-button--large" onClick={() => navigate('/candidate/profile/create')}>
                                Create Profile
                            </button>
                        </DashboardCard>
                        <ProfileCompletionCard completion={completion} />
                    </div>
                ) : (
                    <>
                        <ProfileCompletionCard completion={completion} />

                        <div className="candidate-grid candidate-grid--summary">
                            <DashboardCard title="Profile summary" className="profile-summary-card">
                                <div className="profile-summary-card__identity">
                                    <ProfileAvatar imagePath={profile.profileImagePath} name={fullName} />
                                    <div>
                                        <strong>{profile.professionalTitle || 'Professional title not added'}</strong>
                                        <span>{profile.degree || profile.education || 'Education not added'}</span>
                                    </div>
                                </div>
                                <dl className="profile-facts">
                                    <div><dt>Experience</dt><dd>{profile.experienceYears ?? 0} years</dd></div>
                                    <div><dt>Location</dt><dd>{profile.location || 'Not added'}</dd></div>
                                    <div><dt>Completion</dt><dd>{completion.completionPercentage || 0}%</dd></div>
                                </dl>
                                <div className="card-actions"><button className="candidate-button candidate-button--secondary" onClick={() => navigate('/candidate/profile')}>{profile.profileImagePath ? 'Change Picture' : 'Add Picture'}</button></div>
                            </DashboardCard>

                            <DashboardCard title="Resume" className="resume-card">
                                <div className="card-statline">
                                    <strong>{dashboard.totalResumes || 0}</strong>
                                    <span>{dashboard.hasActiveResume ? 'Active resume ready' : 'No active resume'}</span>
                                </div>
                                <p className="muted-copy">
                                    {dashboard.activeResume?.fileName || 'Upload a resume to start applying confidently.'}
                                </p>
                                {dashboard.latestResume && <small>Latest upload: {formatDate(dashboard.latestResume.uploadedDate)}</small>}
                                <div className="card-actions">
                                    <button className="candidate-button candidate-button--primary" onClick={() => navigate('/candidate/resumes')}>Upload Resume</button>
                                    <button className="candidate-button candidate-button--secondary" onClick={() => navigate('/candidate/resumes')}>Manage Resume</button>
                                </div>
                            </DashboardCard>

                            <DashboardCard title="Skills" className="skills-card">
                                <div className="card-statline">
                                    <strong>{dashboard.totalSkills || 0}</strong>
                                    <span>skills added</span>
                                </div>
                                <div className="skill-list">
                                    {(dashboard.skills || []).slice(0, 6).map(skill => <span key={skill.skillId}>{skill.skillName}</span>)}
                                    {(dashboard.skills || []).length === 0 && <p className="muted-copy">Add skills to improve your profile visibility.</p>}
                                </div>
                                <button className="candidate-button candidate-button--secondary" onClick={() => navigate('/candidate/skills')}>Manage Skills</button>
                            </DashboardCard>
                        </div>

                        <DashboardCard title="Applications" className="applications-card">
                            {(dashboard.totalApplications || 0) === 0 ? (
                                <div className="empty-state"><span>⌁</span><p>No applications yet. Your next opportunity is waiting.</p></div>
                            ) : (
                                <div className="application-stats">
                                    <div><strong>{dashboard.appliedApplications || 0}</strong><span>Applied</span></div>
                                    <div><strong>{dashboard.shortlistedApplications || 0}</strong><span>Shortlisted</span></div>
                                    <div><strong>{dashboard.interviewScheduledApplications || 0}</strong><span>Interview scheduled</span></div>
                                    <div><strong>{dashboard.acceptedApplications || 0}</strong><span>Accepted</span></div>
                                    <div><strong>{dashboard.rejectedApplications || 0}</strong><span>Rejected</span></div>
                                </div>
                            )}
                        </DashboardCard>

                        <div className="candidate-grid candidate-grid--feed">
                            <DashboardCard title="Upcoming interviews">
                                {(dashboard.interviews || []).length === 0
                                    ? <div className="empty-state"><span>◷</span><p>No upcoming interviews.</p></div>
                                    : <div className="dashboard-list">{dashboard.interviews.map(interview => (
                                        <article key={interview.interviewId}>
                                            <strong>{interview.interviewType || 'Interview'}</strong>
                                            <span>{formatDate(interview.scheduledDate)}</span>
                                        </article>
                                    ))}</div>}
                            </DashboardCard>

                            <DashboardCard title="Latest jobs">
                                {(dashboard.recommendedJobs || []).length === 0
                                    ? <div className="empty-state"><span>◇</span><p>No job recommendations yet.</p></div>
                                    : <div className="dashboard-list">{dashboard.recommendedJobs.map(job => (
                                        <article key={job.jobId}>
                                            <strong>{job.title}</strong>
                                            <span>{job.organizationName || job.location || 'SkillNet opportunity'}</span>
                                        </article>
                                    ))}</div>}
                            </DashboardCard>
                        </div>

                        <DashboardCard title="Quick actions" className="quick-actions-card">
                            <div className="quick-actions">
                                {!completion.isComplete && <button onClick={() => navigate('/candidate/profile')}>Complete Profile <span>→</span></button>}
                                <button onClick={() => navigate('/candidate/resumes')}>Upload Resume <span>→</span></button>
                                <button onClick={() => navigate('/candidate/skills')}>Manage Skills <span>→</span></button>
                                <button onClick={() => navigate('/jobs')}>Browse Jobs <span>→</span></button>
                            </div>
                        </DashboardCard>
                    </>
                )}
            </main>
        </div>
    );
};

export default CandidateDashboard;
