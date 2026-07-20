import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRecruiter } from '../../context/RecruiterContext';

const displayValue = (value) => value || 'Not provided';

const RecruiterPending = () => {
    const navigate = useNavigate();
    const { organization, approvalStatus, loading, error, refreshOrganization } = useRecruiter();
    const [checking, setChecking] = useState(false);
    const [checkError, setCheckError] = useState('');
    const [lastChecked, setLastChecked] = useState(null);

    useEffect(() => {
        if (approvalStatus === 'Approved') {
            navigate('/recruiter/dashboard', { replace: true });
        } else if (approvalStatus === 'Rejected') {
            navigate('/recruiter/setup', { replace: true });
        }
    }, [approvalStatus, navigate]);

    const checkStatus = async () => {
        if (checking) return;

        setChecking(true);
        setCheckError('');
        try {
            const refreshedOrganization = await refreshOrganization();
            if (refreshedOrganization === undefined) return;
            if (refreshedOrganization?.approvalStatus === 'Approved') {
                navigate('/recruiter/dashboard', { replace: true });
                return;
            }
            if (refreshedOrganization?.approvalStatus === 'Rejected') {
                navigate('/recruiter/setup', { replace: true });
                return;
            }
            setLastChecked(new Date());
        } catch (requestError) {
            setCheckError(requestError.message || 'The approval status could not be refreshed.');
        } finally {
            setChecking(false);
        }
    };

    if (loading && !organization) {
        return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading approval status...</div>;
    }

    const currentError = checkError || error;

    return (
        <section className="recruiter-pending-page">
            <div className="recruiter-page-heading">
                <div>
                    <span className="recruiter-eyebrow">Organization verification</span>
                    <h2>Approval pending</h2>
                    <p>Your organization is waiting for an administrator review. You can check here when its status changes.</p>
                </div>
                <span className="recruiter-status recruiter-status--pending">{approvalStatus || 'Pending'}</span>
            </div>

            {currentError && (
                <div className="recruiter-setup-alert recruiter-setup-alert--error" role="alert">
                    <strong>Status could not be checked</strong>
                    <p>{currentError}</p>
                </div>
            )}

            <div className="recruiter-pending-card">
                <div className="recruiter-pending-hero">
                    <span className="recruiter-pending-icon" aria-hidden="true">⌛</span>
                    <div>
                        <span className="recruiter-status recruiter-status--pending">Pending</span>
                        <h3>Review in progress</h3>
                        <p>Job publishing will become available after your organization is approved.</p>
                    </div>
                </div>

                <dl className="recruiter-organization-summary">
                    <div><dt>Organization name</dt><dd>{displayValue(organization?.organizationName)}</dd></div>
                    <div><dt>Industry</dt><dd>{displayValue(organization?.industry)}</dd></div>
                    <div><dt>Website</dt><dd>{displayValue(organization?.website)}</dd></div>
                    <div><dt>Address</dt><dd>{displayValue(organization?.address)}</dd></div>
                    <div><dt>Submitted</dt><dd>{organization?.submittedAt ? new Date(organization.submittedAt).toLocaleString() : 'Not available'}</dd></div>
                </dl>

                <div className="recruiter-pending-actions">
                    <div aria-live="polite">
                        {lastChecked && !currentError && `Last checked ${lastChecked.toLocaleTimeString()}`}
                    </div>
                    <button className="recruiter-submit-button" type="button" onClick={checkStatus} disabled={checking || loading}>
                        {checking || loading ? 'Checking...' : 'Check status'}
                    </button>
                </div>
            </div>
        </section>
    );
};

export default RecruiterPending;
