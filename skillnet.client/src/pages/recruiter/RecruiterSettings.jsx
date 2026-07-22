import { useEffect, useState } from 'react';
import { ApiError } from '../../api/apiClient';
import { recruiterApi } from '../../api/recruiterApi';
import { useRecruiter } from '../../context/RecruiterContext';

const emptyForm = { headline: '', bio: '', linkedInUrl: '', experienceYears: '' };

const toForm = (profile) => ({
    headline: profile?.headline || '',
    bio: profile?.bio || '',
    linkedInUrl: profile?.linkedInUrl || '',
    experienceYears: profile?.experienceYears ?? ''
});

const validate = (form) => {
    const errors = {};
    if (form.headline.trim().length > 200) errors.headline = 'Headline must be 200 characters or fewer.';
    if (form.linkedInUrl.trim().length > 255) errors.linkedInUrl = 'LinkedIn URL must be 255 characters or fewer.';
    if (form.experienceYears !== '') {
        const years = Number(form.experienceYears);
        if (!Number.isInteger(years) || years < -2147483648 || years > 2147483647) {
            errors.experienceYears = 'Experience years must be a valid whole number.';
        }
    }
    return errors;
};

const RecruiterSettings = () => {
    const { organization } = useRecruiter();
    const [profile, setProfile] = useState(null);
    const [form, setForm] = useState(emptyForm);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [missing, setMissing] = useState(false);
    const [errors, setErrors] = useState({});
    const [apiError, setApiError] = useState('');
    const [success, setSuccess] = useState('');
    const [reloadKey, setReloadKey] = useState(0);

    useEffect(() => {
        let active = true;
        recruiterApi.getProfile()
            .then((response) => {
                if (!active) return;
                setProfile(response || null);
                setForm(toForm(response));
                setMissing(!response);
                setApiError('');
            })
            .catch((requestError) => {
                if (!active) return;
                if (requestError instanceof ApiError && requestError.status === 404) {
                    setMissing(true);
                    setApiError('');
                } else {
                    setApiError(requestError.message || 'Recruiter profile could not be loaded.');
                }
            })
            .finally(() => {
                if (active) setLoading(false);
            });
        return () => { active = false; };
    }, [reloadKey]);

    const handleChange = ({ target: { name, value } }) => {
        setForm((current) => ({ ...current, [name]: value }));
        setErrors((current) => ({ ...current, [name]: undefined }));
        setApiError('');
        setSuccess('');
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        if (saving) return;

        const validationErrors = validate(form);
        setErrors(validationErrors);
        if (Object.keys(validationErrors).length) return;

        setSaving(true);
        setApiError('');
        setSuccess('');
        try {
            const saved = await recruiterApi.updateProfile({
                headline: form.headline.trim() || null,
                bio: form.bio.trim() || null,
                linkedInUrl: form.linkedInUrl.trim() || null,
                experienceYears: form.experienceYears === '' ? null : Number(form.experienceYears)
            });
            setProfile(saved);
            setForm(toForm(saved));
            setSuccess('Recruiter profile saved successfully.');
        } catch (requestError) {
            setApiError(requestError.message || 'Recruiter profile could not be saved.');
        } finally {
            setSaving(false);
        }
    };

    const retry = () => {
        if (loading) return;
        setLoading(true);
        setApiError('');
        setReloadKey((value) => value + 1);
    };

    if (loading) return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading recruiter settings...</div>;
    if (apiError && !profile) {
        return <div className="recruiter-route-state recruiter-route-state--error"><strong>Recruiter profile could not be loaded.</strong><span>{apiError}</span><button type="button" onClick={retry}>Try again</button></div>;
    }
    if (missing && !profile) {
        return <div className="recruiter-route-state"><strong>No recruiter profile found</strong><span>Your authenticated account does not currently have recruiter profile information.</span></div>;
    }

    return (
        <section className="recruiter-settings-page">
            <div className="recruiter-page-heading"><div><span className="recruiter-eyebrow">Profile</span><h2>Recruiter settings</h2><p>Update the professional information connected to your recruiter account.</p></div></div>
            {apiError && <div className="recruiter-setup-alert recruiter-setup-alert--error" role="alert">{apiError}</div>}
            {success && <div className="recruiter-setup-alert recruiter-setup-alert--success" role="status">{success}</div>}

            <div className="recruiter-settings-grid">
                <aside className="recruiter-settings-identity">
                    <span className="recruiter-settings-avatar">RP</span>
                    <h3>Recruiter profile</h3>
                    <p>{profile?.headline || 'Professional details'}</p>
                    <dl>
                        <div><dt>User ID</dt><dd>#{profile?.userId}</dd></div>
                        {(profile?.organizationName || organization?.organizationName) && <div><dt>Organization</dt><dd>{profile?.organizationName || organization?.organizationName}</dd></div>}
                        {profile?.organizationId && <div><dt>Organization ID</dt><dd>#{profile.organizationId}</dd></div>}
                    </dl>
                </aside>

                <form className="recruiter-settings-form" onSubmit={handleSubmit} noValidate>
                    <div className="recruiter-setup-intro"><div><h3>Professional information</h3><p>Only fields supported by the recruiter profile API can be changed here.</p></div></div>
                    <div className="recruiter-form-grid">
                        <label className="recruiter-form-field recruiter-form-field--wide"><span>Headline</span><input name="headline" value={form.headline} onChange={handleChange} maxLength="200" disabled={saving} placeholder="e.g. Senior Technical Recruiter" aria-invalid={Boolean(errors.headline)} />{errors.headline && <small className="recruiter-field-error">{errors.headline}</small>}</label>
                        <label className="recruiter-form-field"><span>LinkedIn URL</span><input name="linkedInUrl" value={form.linkedInUrl} onChange={handleChange} maxLength="255" disabled={saving} placeholder="https://linkedin.com/in/..." aria-invalid={Boolean(errors.linkedInUrl)} />{errors.linkedInUrl && <small className="recruiter-field-error">{errors.linkedInUrl}</small>}</label>
                        <label className="recruiter-form-field"><span>Experience years</span><input name="experienceYears" type="number" step="1" value={form.experienceYears} onChange={handleChange} disabled={saving} aria-invalid={Boolean(errors.experienceYears)} />{errors.experienceYears && <small className="recruiter-field-error">{errors.experienceYears}</small>}</label>
                        <label className="recruiter-form-field recruiter-form-field--wide"><span>Bio</span><textarea name="bio" value={form.bio} onChange={handleChange} rows="7" disabled={saving} /></label>
                    </div>
                    <div className="recruiter-setup-actions"><button className="recruiter-submit-button" type="submit" disabled={saving}>{saving ? 'Saving...' : 'Save profile'}</button></div>
                </form>
            </div>
        </section>
    );
};

export default RecruiterSettings;
