import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { candidateApi } from '../api/candidateApi';
import DashboardCard from '../components/candidate/DashboardCard';
import ProfileCompletionCard from '../components/candidate/ProfileCompletionCard';
import ProfileFormSection from '../components/candidate/ProfileFormSection';
import ProfileImageManager from '../components/candidate/profile-image/ProfileImageManager';
import CandidateNavigation from '../components/candidate/CandidateNavigation';
import './CandidateDashboard.css';
import './CandidateProfile.css';

const emptyForm = {
    firstName: '', lastName: '', phoneNumber: '', location: '', professionalTitle: '',
    professionalSummary: '', degree: '', university: '', education: '',
    experienceYears: ''
};

const toForm = (profile) => ({
    ...emptyForm,
    firstName: profile.firstName || '',
    lastName: profile.lastName || '',
    phoneNumber: profile.phoneNumber || '',
    location: profile.location || '',
    professionalTitle: profile.professionalTitle || '',
    professionalSummary: profile.professionalSummary || '',
    degree: profile.degree || '',
    university: profile.university || '',
    education: profile.education || '',
    experienceYears: profile.experienceYears ?? ''
});

const CandidateProfile = () => {
    const navigate = useNavigate();
    const [profile, setProfile] = useState(null);
    const [form, setForm] = useState(emptyForm);
    const [savedForm, setSavedForm] = useState(emptyForm);
    const [errors, setErrors] = useState({});
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [loadError, setLoadError] = useState('');
    const [success, setSuccess] = useState('');

    const loadProfile = useCallback(async (showLoader = true) => {
        if (showLoader) setLoading(true);
        setLoadError('');
        try {
            const result = await candidateApi.getProfile();
            const nextForm = toForm(result);
            setProfile(result);
            setForm(nextForm);
            setSavedForm(nextForm);
        } catch (error) {
            setLoadError(error.status === 404
                ? 'You do not have a candidate profile yet.'
                : error.message || 'Your profile could not be loaded.');
        } finally {
            if (showLoader) setLoading(false);
        }
    }, []);

    useEffect(() => {
        // The profile request intentionally initializes page state on mount.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadProfile();
    }, [loadProfile]);

    const updateField = (event) => {
        const { name, value } = event.target;
        setForm(current => ({ ...current, [name]: value }));
        setErrors(current => ({ ...current, [name]: '' }));
        setSuccess('');
    };

    const validate = () => {
        const next = {};
        if (!form.firstName.trim()) next.firstName = 'First name is required.';
        else if (form.firstName.length > 100) next.firstName = 'Use 100 characters or fewer.';
        if (!form.lastName.trim()) next.lastName = 'Last name is required.';
        else if (form.lastName.length > 100) next.lastName = 'Use 100 characters or fewer.';
        if (!form.phoneNumber.trim()) next.phoneNumber = 'Phone number is required.';
        else if (!/^[+()\-\s\d]{7,30}$/.test(form.phoneNumber)) next.phoneNumber = 'Enter a valid phone number.';
        if (!form.location.trim()) next.location = 'Location is required.';
        else if (form.location.length > 150) next.location = 'Use 150 characters or fewer.';
        if (form.professionalTitle.length > 150) next.professionalTitle = 'Use 150 characters or fewer.';
        if (form.professionalSummary.length > 2000) next.professionalSummary = 'Use 2,000 characters or fewer.';
        if (form.degree.length > 150) next.degree = 'Use 150 characters or fewer.';
        if (form.university.length > 200) next.university = 'Use 200 characters or fewer.';
        if (form.education.length > 2000) next.education = 'Use 2,000 characters or fewer.';
        const years = Number(form.experienceYears);
        if (form.experienceYears !== '' && (!Number.isInteger(years) || years < 0 || years > 60)) {
            next.experienceYears = 'Enter a whole number from 0 to 60.';
        }
        setErrors(next);
        return Object.keys(next).length === 0;
    };

    const save = async (event) => {
        event.preventDefault();
        if (!validate()) return;
        setSaving(true);
        setSuccess('');
        try {
            await candidateApi.updateProfile({
                firstName: form.firstName.trim(), lastName: form.lastName.trim(),
                phoneNumber: form.phoneNumber.trim(), location: form.location.trim(),
                professionalTitle: form.professionalTitle.trim() || null,
                professionalSummary: form.professionalSummary.trim() || null,
                degree: form.degree.trim() || null, university: form.university.trim() || null,
                education: form.education.trim() || null,
                experienceYears: form.experienceYears === '' ? null : Number(form.experienceYears),
                profileImagePath: profile?.profileImagePath || null
            });
            await loadProfile();
            setSuccess('Your profile has been updated successfully.');
        } catch (error) {
            setErrors({ form: error.message || 'Your changes could not be saved.' });
        } finally {
            setSaving(false);
        }
    };

    const field = (name, label, options = {}) => (
        <label className={`profile-field ${options.full ? 'profile-field--full' : ''}`}>
            <span>{label}{options.required && <b> *</b>}</span>
            {options.textarea
                ? <textarea name={name} value={form[name]} onChange={updateField} rows={options.rows || 4} maxLength={options.maxLength} />
                : <input name={name} value={form[name]} onChange={updateField} type={options.type || 'text'} min={options.min} max={options.max} maxLength={options.maxLength} />}
            {errors[name] && <small role="alert">{errors[name]}</small>}
        </label>
    );

    if (loading) return <ProfileState><div className="profile-loading skeleton" /><div className="profile-loading skeleton" /></ProfileState>;
    if (loadError) return <ProfileState centered><DashboardCard className="dashboard-error"><span className="dashboard-error__icon">!</span><h1>Profile unavailable</h1><p>{loadError}</p><div className="card-actions"><button className="candidate-button candidate-button--primary" onClick={loadProfile}>Retry</button><button className="candidate-button candidate-button--secondary" onClick={() => navigate('/candidate/profile/create')}>Create Profile</button></div></DashboardCard></ProfileState>;

    const candidateName = `${profile.firstName || ''} ${profile.lastName || ''}`.trim() || 'SkillNet Candidate';
    const completion = profile.profileCompletion || {};
    return (
        <ProfileState>
            <div className="profile-page-heading"><div><span className="candidate-eyebrow">Candidate profile</span><h1>Manage your professional profile</h1><p>Keep your information accurate and ready for recruiters.</p></div><button className="candidate-button candidate-button--ghost" onClick={() => navigate('/candidate/dashboard')}>Return to Dashboard</button></div>
            <div className="candidate-profile-layout">
                <aside className="candidate-profile-sidebar">
                    <DashboardCard className="profile-identity-card">
                        <ProfileImageManager imagePath={profile.profileImagePath} candidateName={candidateName} onChanged={() => loadProfile(false)} onNotify={message => { setSuccess(message); setErrors({}); }} />
                        <h2>{profile.firstName} {profile.lastName}</h2>
                        <p className="profile-title">{profile.professionalTitle || 'Professional title not added'}</p>
                        <dl><div><dt>Degree</dt><dd>{profile.degree || 'Not added'}</dd></div><div><dt>Location</dt><dd>{profile.location || 'Not added'}</dd></div><div><dt>Completion</dt><dd>{completion.completionPercentage || 0}% · Level {completion.completionLevel || 0}</dd></div></dl>
                    </DashboardCard>
                    <ProfileCompletionCard completion={completion} showAction={false} />
                </aside>
                <DashboardCard className="profile-editor-card">
                    <form onSubmit={save} noValidate>
                        {success && <div className="profile-notice profile-notice--success" role="status">{success}</div>}
                        {errors.form && <div className="profile-notice profile-notice--error" role="alert">{errors.form}</div>}
                        <ProfileFormSection title="Basic information" description="Your essential identity and contact details.">{field('firstName', 'First name', { required: true, maxLength: 100 })}{field('lastName', 'Last name', { required: true, maxLength: 100 })}{field('phoneNumber', 'Phone number', { required: true, maxLength: 30 })}{field('location', 'Location', { required: true, maxLength: 150 })}</ProfileFormSection>
                        <ProfileFormSection title="Professional information" description="Tell recruiters about your professional direction.">{field('professionalTitle', 'Professional title', { full: true, maxLength: 150 })}{field('professionalSummary', 'Professional summary', { full: true, textarea: true, maxLength: 2000 })}</ProfileFormSection>
                        <ProfileFormSection title="Education" description="Summarize your academic background.">{field('degree', 'Degree', { maxLength: 150 })}{field('university', 'University', { maxLength: 200 })}{field('education', 'Education summary', { full: true, textarea: true, maxLength: 2000 })}</ProfileFormSection>
                        <ProfileFormSection title="Experience" description="Share your overall professional experience.">{field('experienceYears', 'Years of experience', { type: 'number', min: 0, max: 60 })}</ProfileFormSection>
                        <div className="profile-form-actions"><button type="button" className="candidate-button candidate-button--ghost" onClick={() => { setForm(savedForm); setErrors({}); setSuccess(''); }} disabled={saving}>Cancel</button><button className="candidate-button candidate-button--primary" disabled={saving}>{saving ? 'Saving…' : 'Save Changes'}</button></div>
                    </form>
                </DashboardCard>
            </div>
        </ProfileState>
    );
};

const ProfileState = ({ children, centered = false }) => <div className="candidate-dashboard-shell"><CandidateNavigation /><main className={`candidate-dashboard candidate-profile-page${centered ? ' candidate-dashboard--centered' : ''}`}>{children}</main></div>;

export default CandidateProfile;
