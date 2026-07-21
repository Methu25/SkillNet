import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { buildAssetUrl } from '../../api/apiClient';
import { recruiterApi } from '../../api/recruiterApi';
import { useRecruiter } from '../../context/RecruiterContext';

const fieldLimits = { organizationName: 200, industry: 100, website: 255, address: 500, description: 2000, companySize: 50, contactEmail: 254, contactPhone: 30, linkedInUrl: 255, city: 100, country: 100 };
const supportedLogoTypes = ['image/jpeg', 'image/png', 'image/webp'];
const supportedLogoExtensions = ['.jpg', '.jpeg', '.png', '.webp'];
const maximumLogoSize = 5 * 1024 * 1024;

const toForm = (organization) => ({
    organizationName: organization?.organizationName || '',
    industry: organization?.industry || '',
    website: organization?.website || '',
    address: organization?.address || '',
    description: organization?.description || '',
    companySize: organization?.companySize || '',
    foundedYear: organization?.foundedYear || '',
    contactEmail: organization?.contactEmail || '',
    contactPhone: organization?.contactPhone || '',
    linkedInUrl: organization?.linkedInUrl || '',
    city: organization?.city || '',
    country: organization?.country || ''
});

const getInitials = (name) => name
    ? name.trim().split(/\s+/).slice(0, 2).map((part) => part[0]).join('').toUpperCase()
    : 'SN';

const RecruiterCompanyEditor = ({ recruiterState }) => {
    const { organization, setOrganization } = recruiterState;
    const [form, setForm] = useState(() => toForm(organization));
    const [savedForm, setSavedForm] = useState(() => toForm(organization));
    const [errors, setErrors] = useState({});
    const [apiError, setApiError] = useState('');
    const [success, setSuccess] = useState('');
    const [saving, setSaving] = useState(false);
    const [logoBusy, setLogoBusy] = useState(false);
    const [previewUrl, setPreviewUrl] = useState('');
    const fileInputRef = useRef(null);

    useEffect(() => () => {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
    }, [previewUrl]);

    const completionItems = [
        ['Organization name', organization?.organizationName],
        ['Industry', organization?.industry],
        ['Logo', organization?.logo],
        ['Description', organization?.description],
        ['Website', organization?.website],
        ['Contact email', organization?.contactEmail],
        ['City', organization?.city],
        ['Country', organization?.country]
    ];
    const completedItems = completionItems.filter(([, value]) => Boolean(String(value || '').trim()));
    const missingItems = completionItems.filter(([, value]) => !String(value || '').trim());
    const completionPercentage = Math.round((completedItems.length / completionItems.length) * 100);
    const serverLogoUrl = organization?.logo ? buildAssetUrl(organization.logo) : '';
    const displayedLogo = previewUrl || serverLogoUrl;
    const isBusy = saving || logoBusy;

    const clearMessages = () => { setApiError(''); setSuccess(''); };

    const handleChange = ({ target: { name, value } }) => {
        setForm((current) => ({ ...current, [name]: value }));
        setErrors((current) => ({ ...current, [name]: undefined }));
        clearMessages();
    };

    const validateForm = () => {
        const nextErrors = {};
        if (!form.organizationName.trim()) nextErrors.organizationName = 'Organization name is required.';
        Object.entries(fieldLimits).forEach(([field, limit]) => {
            if (form[field] && form[field].trim().length > limit) nextErrors[field] = `Must be ${limit} characters or fewer.`;
        });
        if (form.website.trim()) {
            try {
                const url = new URL(form.website.trim());
                if (!['http:', 'https:'].includes(url.protocol)) nextErrors.website = 'Enter a valid HTTP or HTTPS URL.';
            } catch {
                nextErrors.website = 'Enter a valid website URL.';
            }
        }
        if (form.linkedInUrl.trim()) {
            try {
                const url = new URL(form.linkedInUrl.trim());
                if (!['http:', 'https:'].includes(url.protocol) || !url.hostname.includes('linkedin.com')) {
                    nextErrors.linkedInUrl = 'Enter a valid LinkedIn URL.';
                }
            } catch {
                nextErrors.linkedInUrl = 'Enter a valid LinkedIn URL.';
            }
        }
        if (form.contactEmail.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.contactEmail.trim())) {
            nextErrors.contactEmail = 'Enter a valid email address.';
        }
        if (form.foundedYear.toString().trim()) {
            const year = parseInt(form.foundedYear, 10);
            const currentYear = new Date().getFullYear();
            if (isNaN(year) || year < 1800 || year > currentYear) {
                nextErrors.foundedYear = `Enter a valid year between 1800 and ${currentYear}.`;
            }
        }
        return nextErrors;
    };

    const handleSave = async (event) => {
        event.preventDefault();
        if (isBusy) return;
        const nextErrors = validateForm();
        setErrors(nextErrors);
        if (Object.keys(nextErrors).length) return;

        setSaving(true); clearMessages();
        try {
            const request = {
                organizationName: form.organizationName.trim(),
                industry: form.industry.trim() || null,
                website: form.website.trim() || null,
                address: form.address.trim() || null,
                logo: organization?.logo || null,
                description: form.description.trim() || null,
                companySize: form.companySize.trim() || null,
                foundedYear: form.foundedYear.toString().trim() ? parseInt(form.foundedYear, 10) : null,
                contactEmail: form.contactEmail.trim() || null,
                contactPhone: form.contactPhone.trim() || null,
                linkedInUrl: form.linkedInUrl.trim() || null,
                city: form.city.trim() || null,
                country: form.country.trim() || null
            };
            const saved = await recruiterApi.updateOrganization(request);
            const nextForm = toForm(saved);
            setOrganization(saved);
            setForm(nextForm);
            setSavedForm(nextForm);
            setSuccess(organization ? 'Company profile saved successfully.' : 'Company profile created successfully. You can now upload a logo.');
        } catch (requestError) {
            setApiError(requestError.message || 'The company profile could not be saved.');
        } finally {
            setSaving(false);
        }
    };

    const validateLogo = (file) => {
        if (!file || file.size <= 0) return 'Choose a non-empty image file.';
        if (file.size > maximumLogoSize) return 'Logo size cannot exceed 5 MB.';
        const extension = `.${file.name.split('.').pop()?.toLowerCase() || ''}`;
        if (!supportedLogoTypes.includes(file.type) || !supportedLogoExtensions.includes(extension)) {
            return 'Choose a JPG, JPEG, PNG, or WebP image.';
        }
        return '';
    };

    const handleLogoSelected = async (event) => {
        const file = event.target.files?.[0];
        event.target.value = '';
        const validationError = validateLogo(file);
        if (validationError) { setApiError(validationError); setSuccess(''); return; }

        const localPreview = URL.createObjectURL(file);
        setPreviewUrl(localPreview);
        setLogoBusy(true); clearMessages();
        try {
            const updated = await recruiterApi.uploadOrganizationLogo(file);
            setOrganization(updated);
            setPreviewUrl('');
            setSuccess(organization?.logo ? 'Company logo changed successfully.' : 'Company logo uploaded successfully.');
        } catch (requestError) {
            setPreviewUrl('');
            setApiError(requestError.message || 'The company logo could not be uploaded.');
        } finally {
            setLogoBusy(false);
        }
    };

    const handleRemoveLogo = async () => {
        if (logoBusy || !organization?.logo) return;
        setLogoBusy(true); clearMessages();
        try {
            const updated = await recruiterApi.deleteOrganizationLogo();
            setOrganization(updated);
            setPreviewUrl('');
            setSuccess('Company logo removed successfully.');
        } catch (requestError) {
            setApiError(requestError.message || 'The company logo could not be removed.');
        } finally {
            setLogoBusy(false);
        }
    };

    const handleCancel = () => {
        setForm(savedForm);
        setErrors({});
        clearMessages();
    };

    return (
        <section className="recruiter-company-profile-page">
            <header className="recruiter-company-profile-heading">
                <div><span className="recruiter-eyebrow">Company profile</span><h1>{organization ? 'Manage your company profile' : 'Create your company profile'}</h1><p>Keep your organization details accurate and present a polished employer profile.</p></div>
                <Link className="recruiter-secondary-action" to="/recruiter/dashboard">Return to Dashboard</Link>
            </header>

            {apiError && <div className="recruiter-profile-notice recruiter-profile-notice--error" role="alert">{apiError}</div>}
            {success && <div className="recruiter-profile-notice recruiter-profile-notice--success" role="status">{success}</div>}

            <div className="recruiter-company-profile-layout">
                <aside className="recruiter-company-profile-sidebar">
                    <section className="recruiter-company-identity-card">
                        <div className="recruiter-company-profile-logo">
                            {displayedLogo ? <img src={displayedLogo} alt={`${organization?.organizationName || form.organizationName || 'Organization'} logo preview`} /> : <span>{getInitials(organization?.organizationName || form.organizationName)}</span>}
                            {logoBusy && <span className="recruiter-logo-overlay"><i className="recruiter-spinner" />Uploading</span>}
                        </div>
                        <input ref={fileInputRef} className="recruiter-visually-hidden" type="file" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" onChange={handleLogoSelected} disabled={!organization || isBusy} aria-label="Choose organization logo" />
                        <button className="recruiter-submit-button recruiter-logo-upload-button" type="button" onClick={() => fileInputRef.current?.click()} disabled={!organization || isBusy}>{organization?.logo ? 'Change Logo' : 'Upload Logo'}</button>
                        {organization?.logo && <button className="recruiter-logo-remove-button" type="button" onClick={handleRemoveLogo} disabled={isBusy}>Remove Logo</button>}
                        {!organization && <small className="recruiter-logo-help">Save the company profile before uploading a logo.</small>}
                        <h2>{organization?.organizationName || form.organizationName || 'Your organization'}</h2>
                        <p className="recruiter-company-profile-industry">{organization?.industry || form.industry || 'Industry not added'}</p>
                        <dl>
                            <div><dt>Location</dt><dd>{[organization?.city || form.city, organization?.country || form.country].filter(Boolean).join(', ') || organization?.address || form.address || 'Not added'}</dd></div>
                            <div><dt>Website</dt><dd>{organization?.website || form.website || 'Not added'}</dd></div>
                            <div><dt>LinkedIn</dt><dd>{organization?.linkedInUrl || form.linkedInUrl ? 'Added' : 'Not added'}</dd></div>
                        </dl>
                    </section>

                    <section className="recruiter-completion-card">
                        <div className="recruiter-completion-heading"><div><span>Profile completion</span><strong>{completionPercentage}%</strong></div><div className="recruiter-completion-track" aria-label={`${completionPercentage}% complete`}><span style={{ width: `${completionPercentage}%` }} /></div></div>
                        <div className="recruiter-completion-columns"><div><h3>Completed</h3>{completedItems.length ? <ul>{completedItems.map(([label]) => <li key={label}>{label}</li>)}</ul> : <p>Nothing completed yet.</p>}</div><div><h3>Missing</h3>{missingItems.length ? <ul>{missingItems.map(([label]) => <li key={label}>{label}</li>)}</ul> : <p>Your profile is complete.</p>}</div></div>
                        <small>Completion is optional and never blocks job posting.</small>
                    </section>
                </aside>

                <form className="recruiter-company-editor-card" onSubmit={handleSave} noValidate>
                    <section className="recruiter-company-form-section">
                        <header><h2>Company identity</h2><p>Add the core details candidates use to recognize your organization.</p></header>
                        <div className="recruiter-company-form-fields recruiter-form-grid-2">
                            <label className="recruiter-form-field recruiter-form-field--wide"><span>Organization name *</span><input name="organizationName" value={form.organizationName} onChange={handleChange} maxLength={fieldLimits.organizationName} disabled={isBusy} aria-invalid={Boolean(errors.organizationName)} />{errors.organizationName && <small className="recruiter-field-error">{errors.organizationName}</small>}</label>
                            <label className="recruiter-form-field"><span>Industry</span><input name="industry" value={form.industry} onChange={handleChange} maxLength={fieldLimits.industry} disabled={isBusy} />{errors.industry && <small className="recruiter-field-error">{errors.industry}</small>}</label>
                            <label className="recruiter-form-field recruiter-form-field--select"><span>Company size</span><select name="companySize" value={form.companySize} onChange={handleChange} disabled={isBusy}><option value="">Select size</option><option value="1-10">1-10 employees</option><option value="11-50">11-50 employees</option><option value="51-200">51-200 employees</option><option value="201-500">201-500 employees</option><option value="501-1000">501-1000 employees</option><option value="1001-5000">1001-5000 employees</option><option value="5001-10000">5001-10000 employees</option><option value="10001+">10001+ employees</option></select>{errors.companySize && <small className="recruiter-field-error">{errors.companySize}</small>}</label>
                            <label className="recruiter-form-field recruiter-form-field--number"><span>Founded year</span><input name="foundedYear" type="number" min="1800" max={new Date().getFullYear()} value={form.foundedYear} onChange={handleChange} disabled={isBusy} aria-invalid={Boolean(errors.foundedYear)} />{errors.foundedYear && <small className="recruiter-field-error">{errors.foundedYear}</small>}</label>
                        </div>
                    </section>
                    <section className="recruiter-company-form-section">
                        <header><h2>Company overview</h2><p>Provide a description or "About Us" section for candidates.</p></header>
                        <div className="recruiter-company-form-fields"><label className="recruiter-form-field recruiter-form-field--wide"><span>Description</span><textarea name="description" value={form.description} onChange={handleChange} maxLength={fieldLimits.description} rows="6" disabled={isBusy} />{errors.description && <small className="recruiter-field-error">{errors.description}</small>}</label></div>
                    </section>
                    <section className="recruiter-company-form-section">
                        <header><h2>Online presence</h2><p>Links to where candidates can learn more.</p></header>
                        <div className="recruiter-company-form-fields recruiter-form-grid-2">
                            <label className="recruiter-form-field"><span>Website</span><input name="website" type="url" value={form.website} onChange={handleChange} maxLength={fieldLimits.website} placeholder="https://example.com" disabled={isBusy} aria-invalid={Boolean(errors.website)} />{errors.website && <small className="recruiter-field-error">{errors.website}</small>}</label>
                            <label className="recruiter-form-field"><span>LinkedIn URL</span><input name="linkedInUrl" type="url" value={form.linkedInUrl} onChange={handleChange} maxLength={fieldLimits.linkedInUrl} placeholder="https://linkedin.com/company/example" disabled={isBusy} aria-invalid={Boolean(errors.linkedInUrl)} />{errors.linkedInUrl && <small className="recruiter-field-error">{errors.linkedInUrl}</small>}</label>
                        </div>
                    </section>
                    <section className="recruiter-company-form-section">
                        <header><h2>Contact</h2><p>How candidates or administrators can reach out.</p></header>
                        <div className="recruiter-company-form-fields recruiter-form-grid-2">
                            <label className="recruiter-form-field"><span>Contact email</span><input name="contactEmail" type="email" value={form.contactEmail} onChange={handleChange} maxLength={fieldLimits.contactEmail} disabled={isBusy} aria-invalid={Boolean(errors.contactEmail)} />{errors.contactEmail && <small className="recruiter-field-error">{errors.contactEmail}</small>}</label>
                            <label className="recruiter-form-field"><span>Contact phone</span><input name="contactPhone" type="tel" value={form.contactPhone} onChange={handleChange} maxLength={fieldLimits.contactPhone} disabled={isBusy} />{errors.contactPhone && <small className="recruiter-field-error">{errors.contactPhone}</small>}</label>
                        </div>
                    </section>
                    <section className="recruiter-company-form-section">
                        <header><h2>Location</h2><p>Where your organization is based.</p></header>
                        <div className="recruiter-company-form-fields recruiter-form-grid-2">
                            <label className="recruiter-form-field recruiter-form-field--wide"><span>Address</span><textarea name="address" value={form.address} onChange={handleChange} maxLength={fieldLimits.address} rows="3" disabled={isBusy} />{errors.address && <small className="recruiter-field-error">{errors.address}</small>}</label>
                            <label className="recruiter-form-field"><span>City</span><input name="city" value={form.city} onChange={handleChange} maxLength={fieldLimits.city} disabled={isBusy} />{errors.city && <small className="recruiter-field-error">{errors.city}</small>}</label>
                            <label className="recruiter-form-field"><span>Country</span><input name="country" value={form.country} onChange={handleChange} maxLength={fieldLimits.country} disabled={isBusy} />{errors.country && <small className="recruiter-field-error">{errors.country}</small>}</label>
                        </div>
                    </section>
                    <div className="recruiter-company-form-actions"><button className="recruiter-secondary-button" type="button" onClick={handleCancel} disabled={isBusy}>Cancel</button><button className="recruiter-submit-button" type="submit" disabled={isBusy}>{saving ? 'Saving...' : organization ? 'Save Changes' : 'Save Company Profile'}</button></div>
                </form>
            </div>
        </section>
    );
};

const RecruiterCompany = () => {
    const recruiterState = useRecruiter();
    const { organization, loading, error, refreshOrganization } = recruiterState;
    if (loading && !organization) return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading company profile...</div>;
    if (error && !organization) return <div className="recruiter-route-state recruiter-route-state--error"><strong>Company profile could not be loaded.</strong><span>{error}</span><button type="button" onClick={refreshOrganization}>Try again</button></div>;
    return <RecruiterCompanyEditor recruiterState={recruiterState} />;
};

export default RecruiterCompany;
