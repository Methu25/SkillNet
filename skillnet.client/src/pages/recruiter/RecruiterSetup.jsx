import { useState } from 'react';
import { Link } from 'react-router-dom';
import { recruiterApi } from '../../api/recruiterApi';
import { useRecruiter } from '../../context/RecruiterContext';

const fieldLimits = {
    organizationName: 200,
    industry: 150,
    website: 255,
    logo: 255,
    address: 500
};

const toForm = (organization) => ({
    organizationName: organization?.organizationName || '',
    industry: organization?.industry || '',
    website: organization?.website || '',
    logo: organization?.logo || '',
    address: organization?.address || ''
});

const validate = (form) => {
    const errors = {};

    if (!form.organizationName.trim()) {
        errors.organizationName = 'Organization name is required.';
    }

    Object.entries(fieldLimits).forEach(([field, limit]) => {
        if (form[field].trim().length > limit) {
            errors[field] = `Must be ${limit} characters or fewer.`;
        }
    });

    return errors;
};

const toRequest = (form) => ({
    organizationName: form.organizationName.trim(),
    industry: form.industry.trim() || null,
    website: form.website.trim() || null,
    logo: form.logo.trim() || null,
    address: form.address.trim() || null
});

const RecruiterSetupEditor = ({ recruiterState }) => {
    const {
        organization,
        refreshOrganization,
        setOrganization
    } = recruiterState;
    const [form, setForm] = useState(() => toForm(organization));
    const [errors, setErrors] = useState({});
    const [requestError, setRequestError] = useState('');
    const [success, setSuccess] = useState('');
    const [saving, setSaving] = useState(false);
    const [submitting, setSubmitting] = useState(false);

    const status = organization?.approvalStatus || 'Draft';
    const isLocked = status === 'Pending' || status === 'Approved';
    const isBusy = saving || submitting;

    const handleChange = ({ target: { name, value } }) => {
        setForm((current) => ({ ...current, [name]: value }));
        setErrors((current) => ({ ...current, [name]: undefined }));
        setRequestError('');
        setSuccess('');
    };

    const persistDraft = async () => {
        const validationErrors = validate(form);
        setErrors(validationErrors);
        if (Object.keys(validationErrors).length) return null;

        const savedOrganization = await recruiterApi.updateOrganization(toRequest(form));
        setOrganization(savedOrganization);
        setForm(toForm(savedOrganization));
        await refreshOrganization();
        return savedOrganization;
    };

    const handleSave = async (event) => {
        event.preventDefault();
        if (isBusy || isLocked) return;

        setSaving(true);
        setRequestError('');
        setSuccess('');
        try {
            const saved = await persistDraft();
            if (saved) setSuccess('Your organization draft has been saved.');
        } catch (requestFailure) {
            setRequestError(requestFailure.message || 'The organization draft could not be saved.');
        } finally {
            setSaving(false);
        }
    };

    const handleSubmit = async () => {
        if (isBusy || isLocked) return;

        setSubmitting(true);
        setRequestError('');
        setSuccess('');
        try {
            const saved = await persistDraft();
            if (!saved) return;

            const submittedOrganization = await recruiterApi.submitOrganization();
            setOrganization(submittedOrganization);
            await refreshOrganization();
            setSuccess('Your organization has been submitted for approval.');
        } catch (requestFailure) {
            setRequestError(requestFailure.message || 'The organization could not be submitted.');
        } finally {
            setSubmitting(false);
        }
    };

    if (isLocked) {
        return (
            <section className="recruiter-setup">
                <div className="recruiter-page-heading">
                    <div><span className="recruiter-eyebrow">Organization</span><h2>Company setup</h2><p>Manage the organization connected to your recruiter account.</p></div>
                </div>
                {success && <div className="recruiter-setup-alert recruiter-setup-alert--success" role="status">{success}</div>}
                <div className="recruiter-setup-card recruiter-setup-locked">
                    <span className={`recruiter-status recruiter-status--${status.toLowerCase()}`}>{status}</span>
                    <h3>{status === 'Pending' ? 'Approval is in progress' : 'Your organization is approved'}</h3>
                    <p>{status === 'Pending' ? 'Your organization is currently being reviewed and cannot be edited.' : 'Your approved organization details are locked from this setup page.'}</p>
                    <Link className="recruiter-primary-action" to={status === 'Pending' ? '/recruiter/pending' : '/recruiter/dashboard'}>
                        {status === 'Pending' ? 'View approval status' : 'Go to dashboard'}
                    </Link>
                </div>
            </section>
        );
    }

    return (
        <section className="recruiter-setup">
            <div className="recruiter-page-heading">
                <div>
                    <span className="recruiter-eyebrow">Organization</span>
                    <h2>{organization ? 'Edit company details' : 'Set up your company'}</h2>
                    <p>Save your progress as a draft, then submit the completed organization for admin approval.</p>
                </div>
                <span className={`recruiter-status recruiter-status--${status.toLowerCase()}`}>{status}</span>
            </div>

            {status === 'Rejected' && (
                <div className="recruiter-setup-alert recruiter-setup-alert--rejected" role="alert">
                    <strong>Changes requested</strong>
                    <p>{organization?.rejectionReason || 'Your organization was rejected. Update the details and submit it again.'}</p>
                </div>
            )}

            {requestError && <div className="recruiter-setup-alert recruiter-setup-alert--error" role="alert">{requestError}</div>}
            {success && <div className="recruiter-setup-alert recruiter-setup-alert--success" role="status">{success}</div>}

            <form className="recruiter-setup-card" onSubmit={handleSave} noValidate>
                <div className="recruiter-setup-intro">
                    <div><h3>Organization information</h3><p>Fields marked with an asterisk are required.</p></div>
                </div>

                <div className="recruiter-form-grid">
                    <label className="recruiter-form-field recruiter-form-field--wide">
                        <span>Organization name *</span>
                        <input name="organizationName" value={form.organizationName} onChange={handleChange} maxLength={fieldLimits.organizationName} aria-invalid={Boolean(errors.organizationName)} disabled={isBusy} />
                        {errors.organizationName && <small className="recruiter-field-error">{errors.organizationName}</small>}
                    </label>
                    <label className="recruiter-form-field">
                        <span>Industry</span>
                        <input name="industry" value={form.industry} onChange={handleChange} maxLength={fieldLimits.industry} disabled={isBusy} />
                        {errors.industry && <small className="recruiter-field-error">{errors.industry}</small>}
                    </label>
                    <label className="recruiter-form-field">
                        <span>Website</span>
                        <input name="website" value={form.website} onChange={handleChange} maxLength={fieldLimits.website} placeholder="https://example.com" disabled={isBusy} />
                        {errors.website && <small className="recruiter-field-error">{errors.website}</small>}
                    </label>
                    <label className="recruiter-form-field recruiter-form-field--wide">
                        <span>Logo URL</span>
                        <input name="logo" value={form.logo} onChange={handleChange} maxLength={fieldLimits.logo} placeholder="https://example.com/logo.png" disabled={isBusy} />
                        {errors.logo && <small className="recruiter-field-error">{errors.logo}</small>}
                    </label>
                    <label className="recruiter-form-field recruiter-form-field--wide">
                        <span>Address</span>
                        <textarea name="address" value={form.address} onChange={handleChange} maxLength={fieldLimits.address} rows="4" disabled={isBusy} />
                        {errors.address && <small className="recruiter-field-error">{errors.address}</small>}
                    </label>
                </div>

                <div className="recruiter-setup-actions">
                    <button className="recruiter-secondary-button" type="submit" disabled={isBusy}>
                        {saving ? 'Saving...' : 'Save draft'}
                    </button>
                    <button className="recruiter-submit-button" type="button" onClick={handleSubmit} disabled={isBusy}>
                        {submitting ? 'Submitting...' : status === 'Rejected' ? 'Resubmit for approval' : 'Submit for approval'}
                    </button>
                </div>
            </form>
        </section>
    );
};

const RecruiterSetup = () => {
    const recruiterState = useRecruiter();
    const { organization, loading, error, refreshOrganization } = recruiterState;

    if (loading && !organization) {
        return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading organization details...</div>;
    }

    if (error && !organization) {
        return (
            <div className="recruiter-route-state recruiter-route-state--error">
                <strong>Organization details could not be loaded.</strong>
                <span>{error}</span>
                <button type="button" onClick={refreshOrganization}>Try again</button>
            </div>
        );
    }

    return <RecruiterSetupEditor recruiterState={recruiterState} />;
};

export default RecruiterSetup;
