import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { candidateApi } from '../api/candidateApi';
import StepCard from '../components/candidate/wizard/StepCard';
import WizardLayout from '../components/candidate/wizard/WizardLayout';
import WizardNavigation from '../components/candidate/wizard/WizardNavigation';
import WizardProgress from '../components/candidate/wizard/WizardProgress';
import './CandidateDashboard.css';
import './CandidateProfileCreate.css';

const steps = ['Basic information', 'Professional', 'Education', 'Experience', 'Review'];

const initialForm = {
    firstName: '', lastName: '', phoneNumber: '', location: '',
    professionalTitle: '', professionalSummary: '',
    degree: '', university: '', education: '',
    experienceYears: ''
};

const CandidateProfileCreate = () => {
    const navigate = useNavigate();
    const [currentStep, setCurrentStep] = useState(0);
    const [form, setForm] = useState(initialForm);
    const [errors, setErrors] = useState({});
    const [checkingProfile, setCheckingProfile] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [submitError, setSubmitError] = useState('');

    useEffect(() => {
        let active = true;
        candidateApi.profileExists()
            .then(result => {
                if (active && result.exists) navigate('/candidate/dashboard', { replace: true });
            })
            .catch(() => {})
            .finally(() => { if (active) setCheckingProfile(false); });
        return () => { active = false; };
    }, [navigate]);

    const updateField = (event) => {
        const { name, value } = event.target;
        setForm(current => ({ ...current, [name]: value }));
        setErrors(current => ({ ...current, [name]: '' }));
    };

    const validateStep = () => {
        const nextErrors = {};
        if (currentStep === 0) {
            if (!form.firstName.trim()) nextErrors.firstName = 'First name is required.';
            if (form.firstName.length > 100) nextErrors.firstName = 'First name must be 100 characters or fewer.';
            if (!form.lastName.trim()) nextErrors.lastName = 'Last name is required.';
            if (form.lastName.length > 100) nextErrors.lastName = 'Last name must be 100 characters or fewer.';
            if (!form.phoneNumber.trim()) nextErrors.phoneNumber = 'Phone number is required.';
            else if (!/^[+()\-\s\d]{7,30}$/.test(form.phoneNumber)) nextErrors.phoneNumber = 'Enter a valid phone number.';
            if (!form.location.trim()) nextErrors.location = 'Location is required.';
            if (form.location.length > 150) nextErrors.location = 'Location must be 150 characters or fewer.';
        }
        if (currentStep === 1) {
            if (form.professionalTitle.length > 150) nextErrors.professionalTitle = 'Title must be 150 characters or fewer.';
            if (form.professionalSummary.length > 2000) nextErrors.professionalSummary = 'Summary must be 2,000 characters or fewer.';
        }
        if (currentStep === 2) {
            if (form.degree.length > 150) nextErrors.degree = 'Degree must be 150 characters or fewer.';
            if (form.university.length > 200) nextErrors.university = 'University must be 200 characters or fewer.';
            if (form.education.length > 2000) nextErrors.education = 'Education summary must be 2,000 characters or fewer.';
        }
        if (currentStep === 3) {
            const years = Number(form.experienceYears);
            if (form.experienceYears !== '' && (!Number.isInteger(years) || years < 0 || years > 60)) {
                nextErrors.experienceYears = 'Years of experience must be a whole number from 0 to 60.';
            }
        }
        setErrors(nextErrors);
        return Object.keys(nextErrors).length === 0;
    };

    const next = () => { if (validateStep()) setCurrentStep(step => Math.min(step + 1, steps.length - 1)); };
    const skip = () => { setErrors({}); setCurrentStep(step => Math.min(step + 1, steps.length - 1)); };
    const back = () => { setErrors({}); setCurrentStep(step => Math.max(step - 1, 0)); };

    const finish = async () => {
        setSubmitting(true);
        setSubmitError('');
        try {
            const profile = await candidateApi.createProfile({
                firstName: form.firstName.trim(),
                lastName: form.lastName.trim(),
                phoneNumber: form.phoneNumber.trim(),
                location: form.location.trim(),
                professionalTitle: form.professionalTitle.trim() || null,
                professionalSummary: form.professionalSummary.trim() || null,
                degree: form.degree.trim() || null,
                university: form.university.trim() || null,
                education: form.education.trim() || null,
                experienceYears: form.experienceYears === '' ? null : Number(form.experienceYears),
                profileImagePath: null
            });
            navigate('/candidate/dashboard', { replace: true, state: { profileCompletion: profile.profileCompletion } });
        } catch (error) {
            setSubmitError(error.message || 'Your profile could not be created. Please try again.');
        } finally {
            setSubmitting(false);
        }
    };

    if (checkingProfile) {
        return <div className="wizard-page-loader">Checking your profile…</div>;
    }

    const field = (name, label, options = {}) => (
        <label className={`wizard-field ${options.full ? 'wizard-field--full' : ''}`}>
            <span>{label}{options.required && <b> *</b>}</span>
            {options.textarea ? (
                <textarea name={name} value={form[name]} onChange={updateField} maxLength={options.maxLength} rows={options.rows || 5} placeholder={options.placeholder} />
            ) : (
                <input name={name} value={form[name]} onChange={updateField} maxLength={options.maxLength} type={options.type || 'text'} min={options.min} max={options.max} placeholder={options.placeholder} />
            )}
            {errors[name] && <small className="wizard-field__error">{errors[name]}</small>}
        </label>
    );

    return (
        <WizardLayout>
            <WizardProgress steps={steps} currentStep={currentStep} />
            <div className="wizard-panel">
                {currentStep === 0 && <StepCard title="Basic information" description="Start with the details recruiters use to identify and contact you.">
                    <div className="wizard-fields">
                        {field('firstName', 'First name', { required: true, maxLength: 100, placeholder: 'Your first name' })}
                        {field('lastName', 'Last name', { required: true, maxLength: 100, placeholder: 'Your last name' })}
                        {field('phoneNumber', 'Phone number', { required: true, maxLength: 30, placeholder: '+94 77 123 4567' })}
                        {field('location', 'Location', { required: true, maxLength: 150, placeholder: 'City, country' })}
                    </div>
                </StepCard>}

                {currentStep === 1 && <StepCard title="Professional information" description="Tell recruiters what you do and what makes you a strong candidate.">
                    <div className="wizard-fields">
                        {field('professionalTitle', 'Professional title', { full: true, maxLength: 150, placeholder: 'e.g. Junior Software Engineer' })}
                        {field('professionalSummary', 'Professional summary', { full: true, textarea: true, maxLength: 2000, placeholder: 'Tell recruiters about yourself.' })}
                    </div>
                </StepCard>}

                {currentStep === 2 && <StepCard title="Education" description="Share your academic background and the focus of your studies.">
                    <div className="wizard-fields">
                        {field('degree', 'Degree', { maxLength: 150, placeholder: 'e.g. BSc in Information Technology' })}
                        {field('university', 'University', { maxLength: 200, placeholder: 'Institution name' })}
                        {field('education', 'Education summary', { full: true, textarea: true, maxLength: 2000, placeholder: 'Relevant coursework, achievements, or academic focus.' })}
                    </div>
                </StepCard>}

                {currentStep === 3 && <StepCard title="Experience" description="A brief overview helps recruiters understand your current career level.">
                    <div className="wizard-fields">
                        {field('experienceYears', 'Years of experience', { type: 'number', min: 0, max: 60, placeholder: '0' })}
                    </div>
                </StepCard>}

                {currentStep === 4 && <StepCard title="Review your profile" description="Check your information before creating your SkillNet profile.">
                    <div className="wizard-review">
                        <ReviewSection title="Basic information" values={[form.firstName, form.lastName, form.phoneNumber, form.location]} />
                        <ReviewSection title="Professional" values={[form.professionalTitle, form.professionalSummary]} />
                        <ReviewSection title="Education" values={[form.degree, form.university, form.education]} />
                        <ReviewSection title="Experience" values={[form.experienceYears !== '' ? `${form.experienceYears} years` : '']} />
                    </div>
                    {submitError && <div className="wizard-submit-error" role="alert">{submitError}</div>}
                </StepCard>}

                <WizardNavigation
                    canGoBack={currentStep > 0}
                    isOptional={currentStep > 0 && currentStep < 4}
                    isFinal={currentStep === 4}
                    submitting={submitting}
                    onBack={back}
                    onNext={next}
                    onSkip={skip}
                    onFinish={finish}
                />
            </div>
        </WizardLayout>
    );
};

const ReviewSection = ({ title, values }) => {
    const populated = values.filter(value => String(value || '').trim());
    return <section><h3>{title}</h3>{populated.length > 0 ? populated.map((value, index) => <p key={`${title}-${index}`}>{value}</p>) : <p className="muted-copy">Skipped</p>}</section>;
};

export default CandidateProfileCreate;
