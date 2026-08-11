import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { applicationApi } from '../api/applicationApi';
import CandidateNavigation from '../components/candidate/CandidateNavigation';
import DashboardCard from '../components/candidate/DashboardCard';
import WithdrawApplicationDialog from '../components/candidate/applications/WithdrawApplicationDialog';
import './CandidateDashboard.css';
import './CandidateApplications.css';

const TERMINAL_STATUSES = new Set(['hired', 'rejected', 'withdrawn']);

const formatDate = (value, includeTime = false) => {
    if (!value) return 'Not available';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return 'Not available';
    return new Intl.DateTimeFormat(undefined, includeTime
        ? { dateStyle: 'medium', timeStyle: 'short' }
        : { dateStyle: 'medium' }).format(date);
};

const statusClass = (status) => String(status || 'unknown').toLowerCase().replace(/[^a-z0-9]+/g, '-');
const canWithdraw = (status) => status && !TERMINAL_STATUSES.has(status.toLowerCase());

const CandidateApplications = () => {
    const navigate = useNavigate();
    const { applicationId } = useParams();
    const [applications, setApplications] = useState([]);
    const [selected, setSelected] = useState(null);
    const [loading, setLoading] = useState(true);
    const [detailLoading, setDetailLoading] = useState(false);
    const [error, setError] = useState('');
    const [detailError, setDetailError] = useState('');
    const [notice, setNotice] = useState(null);
    const [withdrawTarget, setWithdrawTarget] = useState(null);
    const [withdrawError, setWithdrawError] = useState('');
    const [withdrawing, setWithdrawing] = useState(false);

    const loadApplications = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            const result = await applicationApi.getMine();
            setApplications(Array.isArray(result) ? result : []);
        } catch (requestError) {
            setError(requestError.message || 'Your applications could not be loaded.');
        } finally {
            setLoading(false);
        }
    }, []);

    const loadDetail = useCallback(async (id) => {
        setDetailLoading(true);
        setDetailError('');
        try {
            setSelected(await applicationApi.getMineById(id));
        } catch (requestError) {
            setSelected(null);
            setDetailError(requestError.status === 404
                ? 'This application was not found.'
                : requestError.message || 'The application details could not be loaded.');
        } finally {
            setDetailLoading(false);
        }
    }, []);

    useEffect(() => {
        // The applications request initializes page state on mount.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadApplications();
    }, [loadApplications]);

    useEffect(() => {
        // Route changes intentionally load the selected application detail.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        if (applicationId) loadDetail(applicationId);
        else {
            setSelected(null);
            setDetailError('');
        }
    }, [applicationId, loadDetail]);

    const openApplication = (id) => navigate(`/candidate/applications/${id}`);
    const closeDetail = () => navigate('/candidate/applications');

    const confirmWithdrawal = async (reason) => {
        setWithdrawing(true);
        setWithdrawError('');
        try {
            await applicationApi.withdraw(withdrawTarget.applicationId, reason);
            setWithdrawTarget(null);
            setNotice({ type: 'success', message: 'Your application was withdrawn successfully.' });
            await Promise.all([
                loadApplications(),
                applicationId ? loadDetail(applicationId) : Promise.resolve()
            ]);
        } catch (requestError) {
            setWithdrawError(requestError.message || 'The application could not be withdrawn.');
        } finally {
            setWithdrawing(false);
        }
    };

    return (
        <div className="candidate-dashboard-shell">
            <CandidateNavigation />
            <main className="candidate-dashboard candidate-applications-page">
                <header className="applications-page-heading">
                    <div>
                        <span className="candidate-eyebrow">Opportunity tracker</span>
                        <h1>My applications</h1>
                        <p>Follow every application and review its latest progress in one place.</p>
                    </div>
                    <div className="applications-total"><strong>{applications.length}</strong><span>Total applications</span></div>
                </header>

                {notice && <div className={`applications-notice applications-notice--${notice.type}`} role="status">{notice.message}<button onClick={() => setNotice(null)} aria-label="Dismiss message">×</button></div>}

                {loading ? (
                    <div className="application-list" aria-label="Loading applications">
                        <div className="application-row application-row--skeleton skeleton" />
                        <div className="application-row application-row--skeleton skeleton" />
                        <div className="application-row application-row--skeleton skeleton" />
                    </div>
                ) : error ? (
                    <DashboardCard className="dashboard-error">
                        <span className="dashboard-error__icon">!</span>
                        <h2>Unable to load applications</h2>
                        <p>{error}</p>
                        <button className="candidate-button candidate-button--primary" onClick={loadApplications}>Retry</button>
                    </DashboardCard>
                ) : applications.length === 0 ? (
                    <DashboardCard className="applications-empty">
                        <div className="applications-empty__icon">✓</div>
                        <h2>No applications yet</h2>
                        <p>Applications you submit through SkillNet will appear here.</p>
                    </DashboardCard>
                ) : (
                    <section className="application-list" aria-label="Your job applications">
                        {applications.map(application => (
                            <article className="application-row" key={application.applicationId}>
                                <div className="application-row__mark">{(application.jobTitle || 'J').charAt(0).toUpperCase()}</div>
                                <div className="application-row__main">
                                    <div className="application-row__title">
                                        <div><h2>{application.jobTitle || 'Untitled job'}</h2><span>Application #{application.applicationId}</span></div>
                                        <span className={`application-status application-status--${statusClass(application.currentStatus)}`}>{application.currentStatus || 'Unknown'}</span>
                                    </div>
                                    <dl className="application-row__facts">
                                        <div><dt>Applied</dt><dd>{formatDate(application.appliedDate)}</dd></div>
                                        <div><dt>Last updated</dt><dd>{formatDate(application.lastUpdated)}</dd></div>
                                        <div><dt>Resume</dt><dd>Resume #{application.resumeId}</dd></div>
                                    </dl>
                                </div>
                                <button className="candidate-button candidate-button--secondary" onClick={() => openApplication(application.applicationId)}>View details</button>
                            </article>
                        ))}
                    </section>
                )}
            </main>

            {applicationId && (
                <div className="application-detail-backdrop" role="presentation" onMouseDown={(event) => { if (event.target === event.currentTarget) closeDetail(); }}>
                    <aside className="application-detail" aria-label="Application details">
                        <button className="application-detail__close" onClick={closeDetail} aria-label="Close application details">×</button>
                        {detailLoading ? <div className="application-detail__loading skeleton" />
                            : detailError ? <div className="application-detail__error" role="alert"><span>!</span><h2>Unable to open application</h2><p>{detailError}</p><button className="candidate-button candidate-button--primary" onClick={() => loadDetail(applicationId)}>Retry</button></div>
                            : selected && <ApplicationDetails application={selected} onWithdraw={() => { setWithdrawError(''); setWithdrawTarget(selected); }} />}
                    </aside>
                </div>
            )}

            {withdrawTarget && <WithdrawApplicationDialog application={withdrawTarget} withdrawing={withdrawing} error={withdrawError} onClose={() => { if (!withdrawing) setWithdrawTarget(null); }} onConfirm={confirmWithdrawal} />}
        </div>
    );
};

const ApplicationDetails = ({ application, onWithdraw }) => {
    const history = Array.isArray(application.statusHistory) ? application.statusHistory : [];

    return (
        <div className="application-detail__content">
            <span className="candidate-eyebrow">Application #{application.applicationId}</span>
            <div className="application-detail__heading">
                <div><h1>{application.jobTitle || 'Untitled job'}</h1><p>Job #{application.jobId}</p></div>
                <span className={`application-status application-status--${statusClass(application.currentStatus)}`}>{application.currentStatus || 'Unknown'}</span>
            </div>

            <dl className="application-detail__facts">
                <div><dt>Applied</dt><dd>{formatDate(application.appliedDate, true)}</dd></div>
                <div><dt>Last updated</dt><dd>{formatDate(application.lastUpdated, true)}</dd></div>
                {application.source && <div><dt>Source</dt><dd>{application.source}</dd></div>}
            </dl>

            {application.scheduledInterview && (
                <section className="application-detail__section candidate-interview-card">
                    <h2>Scheduled interview</h2>
                    <div className="candidate-interview-card__box">
                        <div className="candidate-interview-card__badge">
                            <span>📅 {application.scheduledInterview.interviewType || 'Interview'}</span>
                            <span className="candidate-interview-round">Round {application.scheduledInterview.interviewRound || 1}</span>
                        </div>
                        <dl className="candidate-interview-card__grid">
                            <div>
                                <dt>Date & time</dt>
                                <dd>{formatDate(application.scheduledInterview.scheduledDate, true)}</dd>
                            </div>
                            <div>
                                <dt>Duration</dt>
                                <dd>{application.scheduledInterview.duration} minutes</dd>
                            </div>
                            {application.scheduledInterview.location && (
                                <div>
                                    <dt>Location</dt>
                                    <dd>{application.scheduledInterview.location}</dd>
                                </div>
                            )}
                        </dl>
                        {application.scheduledInterview.meetingLink && (
                            <div className="candidate-interview-card__action">
                                <a
                                    href={application.scheduledInterview.meetingLink}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="candidate-button candidate-button--primary candidate-meeting-link-btn"
                                >
                                    <span>🔗</span> Join Online Interview
                                </a>
                            </div>
                        )}
                    </div>
                </section>
            )}

            <section className="application-detail__section">
                <h2>Resume used</h2>
                <div className="application-resume">
                    <span>PDF</span>
                    <div><strong>{application.resumeFileName || `Resume #${application.resumeId}`}</strong>{application.resumeUploadedDate && <small>Uploaded {formatDate(application.resumeUploadedDate)}</small>}</div>
                </div>
            </section>

            {application.coverLetter && <section className="application-detail__section"><h2>Cover letter</h2><p className="application-cover-letter">{application.coverLetter}</p></section>}

            <section className="application-detail__section">
                <h2>Status history</h2>
                {history.length === 0 ? <p className="application-muted">No status history is available.</p> : <ol className="application-timeline">
                    {history.map(item => <li key={item.statusHistoryId}><span /><div><strong>{item.newStatus}</strong><time>{formatDate(item.changedAt, true)}</time>{item.comment && <p>{item.comment}</p>}</div></li>)}
                </ol>}
            </section>

            {canWithdraw(application.currentStatus) && <div className="application-detail__footer"><button className="candidate-button application-withdraw-button" onClick={onWithdraw}>Withdraw application</button></div>}
        </div>
    );
};

export default CandidateApplications;
