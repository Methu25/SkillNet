import { Link } from 'react-router-dom';
import { buildAssetUrl } from '../../api/apiClient';
import { useRecruiter } from '../../context/RecruiterContext';

const formatDate = (value) => value
    ? new Date(value).toLocaleString(undefined, { year: 'numeric', month: 'long', day: 'numeric', hour: 'numeric', minute: '2-digit' })
    : null;

const getSafeWebsiteUrl = (value) => {
    if (!value) return null;
    try {
        const url = new URL(value);
        return url.protocol === 'http:' || url.protocol === 'https:' ? url.href : null;
    } catch {
        return null;
    }
};

const RecruiterCompany = () => {
    const { organization, approvalStatus, loading, error, refreshOrganization } = useRecruiter();

    if (loading && !organization) {
        return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading company profile...</div>;
    }

    if (error && !organization) {
        return (
            <div className="recruiter-route-state recruiter-route-state--error">
                <strong>Company profile could not be loaded.</strong>
                <span>{error}</span>
                <button type="button" onClick={refreshOrganization}>Try again</button>
            </div>
        );
    }

    if (!organization) {
        return (
            <div className="recruiter-route-state">
                <strong>No organization found</strong>
                <span>Complete organization setup to create your company profile.</span>
                <Link className="recruiter-primary-action" to="/recruiter/setup">Set up organization</Link>
            </div>
        );
    }

    const canEdit = approvalStatus === 'Draft' || approvalStatus === 'Rejected';
    const logoUrl = organization.logo ? buildAssetUrl(organization.logo) : null;
    const websiteUrl = getSafeWebsiteUrl(organization.website);

    return (
        <section className="recruiter-company-page">
            <div className="recruiter-page-heading">
                <div><span className="recruiter-eyebrow">Organization</span><h2>Company profile</h2><p>Review your organization information and its current verification state.</p></div>
                {canEdit && <Link className="recruiter-primary-action" to="/recruiter/setup">Edit organization</Link>}
            </div>

            {error && <div className="recruiter-setup-alert recruiter-setup-alert--error" role="alert">{error}</div>}
            {approvalStatus === 'Rejected' && organization.rejectionReason && (
                <div className="recruiter-setup-alert recruiter-setup-alert--rejected" role="alert"><strong>Organization changes requested</strong><p>{organization.rejectionReason}</p></div>
            )}

            <article className="recruiter-company-card">
                <header className="recruiter-company-hero">
                    <div className="recruiter-company-logo">
                        {logoUrl ? <img src={logoUrl} alt={`${organization.organizationName} logo`} /> : <span>{organization.organizationName.slice(0, 2).toUpperCase()}</span>}
                    </div>
                    <div className="recruiter-company-identity">
                        <span className={`recruiter-status recruiter-status--${String(approvalStatus || 'draft').toLowerCase()}`}>{approvalStatus || 'Draft'}</span>
                        <h3>{organization.organizationName}</h3>
                        {organization.industry && <p>{organization.industry}</p>}
                    </div>
                    <div className="recruiter-company-id"><span>Organization ID</span><strong>#{organization.organizationId}</strong></div>
                </header>

                <div className="recruiter-company-content">
                    <section>
                        <h4>Organization information</h4>
                        <dl className="recruiter-company-details">
                            <div><dt>Organization name</dt><dd>{organization.organizationName}</dd></div>
                            {organization.industry && <div><dt>Industry</dt><dd>{organization.industry}</dd></div>}
                            {organization.website && <div><dt>Website</dt><dd>{websiteUrl ? <a href={websiteUrl} target="_blank" rel="noreferrer">{organization.website}</a> : organization.website}</dd></div>}
                            {organization.logo && <div><dt>Logo</dt><dd className="recruiter-company-long-value">{organization.logo}</dd></div>}
                            {organization.address && <div><dt>Address</dt><dd>{organization.address}</dd></div>}
                            <div><dt>Created</dt><dd>{formatDate(organization.createdAt)}</dd></div>
                        </dl>
                    </section>

                    <aside className="recruiter-company-approval">
                        <h4>Approval information</h4>
                        <div className="recruiter-company-approval-status"><span>Current status</span><strong className={`recruiter-status recruiter-status--${String(approvalStatus || 'draft').toLowerCase()}`}>{approvalStatus || 'Draft'}</strong></div>
                        <dl>
                            {organization.submittedAt && <div><dt>Submitted</dt><dd>{formatDate(organization.submittedAt)}</dd></div>}
                            {organization.reviewedAt && <div><dt>Reviewed</dt><dd>{formatDate(organization.reviewedAt)}</dd></div>}
                            {organization.rejectionReason && <div><dt>Rejection reason</dt><dd>{organization.rejectionReason}</dd></div>}
                        </dl>
                        {canEdit && <p className="recruiter-company-edit-note">This organization can currently be edited from the setup page.</p>}
                    </aside>
                </div>
            </article>
        </section>
    );
};

export default RecruiterCompany;
