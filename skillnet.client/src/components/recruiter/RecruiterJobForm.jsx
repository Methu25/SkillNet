import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { recruiterApi } from '../../api/recruiterApi';

const employmentTypes = ['Full-time', 'Part-time', 'Contract', 'Internship'];
const workModes = ['Remote', 'Hybrid', 'Onsite'];
const experienceLevels = ['Junior', 'Mid', 'Senior'];

const emptyForm = {
    title: '', description: '', categoryId: '', employmentType: '', workMode: '',
    location: '', salaryMin: '', salaryMax: '', experienceLevel: '',
    applicationDeadline: '', skillIds: []
};

const toDateInput = (value) => value ? String(value).slice(0, 10) : '';

const validate = (form) => {
    const errors = {};
    if (!form.title.trim()) errors.title = 'Job title is required.';
    else if (form.title.trim().length > 200) errors.title = 'Job title must be 200 characters or fewer.';
    if (!form.description.trim()) errors.description = 'Description is required.';
    if (!form.categoryId) errors.categoryId = 'Category is required.';
    if (!form.employmentType) errors.employmentType = 'Employment type is required.';
    if (!form.workMode) errors.workMode = 'Work mode is required.';
    if (form.location.trim().length > 255) errors.location = 'Location must be 255 characters or fewer.';
    if (form.employmentType.length > 50) errors.employmentType = 'Employment type must be 50 characters or fewer.';
    if (form.workMode.length > 50) errors.workMode = 'Work mode must be 50 characters or fewer.';
    if (form.experienceLevel.length > 50) errors.experienceLevel = 'Experience level must be 50 characters or fewer.';

    const minimum = form.salaryMin === '' ? null : Number(form.salaryMin);
    const maximum = form.salaryMax === '' ? null : Number(form.salaryMax);
    if (minimum !== null && (Number.isNaN(minimum) || minimum < 0)) errors.salaryMin = 'Enter a valid non-negative salary.';
    if (maximum !== null && (Number.isNaN(maximum) || maximum < 0)) errors.salaryMax = 'Enter a valid non-negative salary.';
    if (minimum !== null && maximum !== null && maximum < minimum) errors.salaryMax = 'Maximum salary must be at least the minimum salary.';
    return errors;
};

const toRequest = (form) => ({
    title: form.title.trim(),
    description: form.description.trim(),
    categoryId: Number(form.categoryId),
    employmentType: form.employmentType,
    workMode: form.workMode,
    location: form.location.trim() || null,
    salaryMin: form.salaryMin === '' ? null : Number(form.salaryMin),
    salaryMax: form.salaryMax === '' ? null : Number(form.salaryMax),
    experienceLevel: form.experienceLevel || null,
    applicationDeadline: form.applicationDeadline || null,
    skillIds: form.skillIds.map(Number)
});

const RecruiterJobForm = ({ jobId = null }) => {
    const navigate = useNavigate();
    const isEdit = jobId !== null;
    const [form, setForm] = useState(emptyForm);
    const [categories, setCategories] = useState([]);
    const [skills, setSkills] = useState([]);
    const [loading, setLoading] = useState(true);
    const [errors, setErrors] = useState({});
    const [apiError, setApiError] = useState('');
    const [success, setSuccess] = useState('');
    const [processing, setProcessing] = useState('');
    const [savedJobId, setSavedJobId] = useState(jobId);
    const [reloadKey, setReloadKey] = useState(0);

    useEffect(() => {
        let active = true;
        const requests = [recruiterApi.getCategories(), recruiterApi.getSkills()];
        if (isEdit) requests.push(recruiterApi.getJob(jobId));

        Promise.all(requests)
            .then(([categoryResponse, skillResponse, job]) => {
                if (!active) return;
                const loadedCategories = Array.isArray(categoryResponse) ? categoryResponse : [];
                const loadedSkills = Array.isArray(skillResponse) ? skillResponse : [];
                setCategories(loadedCategories);
                setSkills(loadedSkills);
                if (job) {
                    const selectedNames = new Set((job.skills || []).map((name) => name.toLowerCase()));
                    setForm({
                        title: job.title || '', description: job.description || '', categoryId: String(job.categoryId || ''),
                        employmentType: job.employmentType || '', workMode: job.workMode || '', location: job.location || '',
                        salaryMin: job.salaryMin ?? '', salaryMax: job.salaryMax ?? '', experienceLevel: job.experienceLevel || '',
                        applicationDeadline: toDateInput(job.applicationDeadline),
                        skillIds: loadedSkills.filter((skill) => selectedNames.has(skill.skillName.toLowerCase())).map((skill) => String(skill.skillId))
                    });
                }
                setApiError('');
            })
            .catch((requestError) => {
                if (active) setApiError(requestError.message || 'The job form could not be loaded.');
            })
            .finally(() => {
                if (active) setLoading(false);
            });

        return () => { active = false; };
    }, [isEdit, jobId, reloadKey]);

    const handleChange = ({ target }) => {
        const value = target.multiple ? [...target.selectedOptions].map((option) => option.value) : target.value;
        setForm((current) => ({ ...current, [target.name]: value }));
        setErrors((current) => ({ ...current, [target.name]: undefined }));
        setApiError('');
        setSuccess('');
    };

    const save = async (publishAfterSave) => {
        if (processing) return;
        const validationErrors = validate(form);
        setErrors(validationErrors);
        if (Object.keys(validationErrors).length) return;

        setProcessing(publishAfterSave ? 'publish' : 'save');
        setApiError('');
        setSuccess('');
        try {
            const request = toRequest(form);
            const saved = savedJobId
                ? await recruiterApi.updateJob(savedJobId, request)
                : await recruiterApi.createJob(request);
            const currentJobId = saved.jobId;
            setSavedJobId(currentJobId);

            if (publishAfterSave) {
                setSuccess('Job saved as a draft. Publishing now...');
                await recruiterApi.publishJob(currentJobId);
            }

            navigate(`/recruiter/jobs/${currentJobId}`, {
                replace: !isEdit,
                state: { success: publishAfterSave ? 'Job saved and published successfully.' : `Job ${isEdit ? 'updated' : 'created'} successfully.` }
            });
        } catch (requestError) {
            setApiError(requestError.message || 'The job could not be saved.');
        } finally {
            setProcessing('');
        }
    };

    if (loading) return <div className="recruiter-route-state"><span className="recruiter-spinner" />Loading job form...</div>;
    if (apiError && categories.length === 0 && skills.length === 0) {
        return <div className="recruiter-route-state recruiter-route-state--error"><strong>The job form could not be loaded.</strong><span>{apiError}</span><button type="button" onClick={() => { setLoading(true); setReloadKey((value) => value + 1); }}>Try again</button></div>;
    }

    return (
        <section className="recruiter-job-form-page">
            <div className="recruiter-page-heading"><div><span className="recruiter-eyebrow">{isEdit ? `Job #${jobId}` : 'New opportunity'}</span><h2>{isEdit ? 'Edit job' : 'Create a job'}</h2><p>{isEdit ? 'Update the job information and save your changes.' : 'Build a complete job post, then save it as a draft or publish it.'}</p></div></div>
            {apiError && <div className="recruiter-setup-alert recruiter-setup-alert--error" role="alert">{apiError}</div>}
            {success && <div className="recruiter-setup-alert recruiter-setup-alert--success" role="status">{success}</div>}

            <form className="recruiter-setup-card" onSubmit={(event) => { event.preventDefault(); save(false); }} noValidate>
                <div className="recruiter-setup-intro"><div><h3>Job information</h3><p>Fields marked with an asterisk are required.</p></div></div>
                <div className="recruiter-form-grid">
                    <label className="recruiter-form-field recruiter-form-field--wide"><span>Job title *</span><input name="title" value={form.title} onChange={handleChange} maxLength="200" disabled={Boolean(processing)} aria-invalid={Boolean(errors.title)} />{errors.title && <small className="recruiter-field-error">{errors.title}</small>}</label>
                    <label className="recruiter-form-field recruiter-form-field--wide"><span>Description *</span><textarea name="description" value={form.description} onChange={handleChange} rows="8" disabled={Boolean(processing)} aria-invalid={Boolean(errors.description)} />{errors.description && <small className="recruiter-field-error">{errors.description}</small>}</label>
                    <label className="recruiter-form-field"><span>Category *</span><select name="categoryId" value={form.categoryId} onChange={handleChange} disabled={Boolean(processing)} aria-invalid={Boolean(errors.categoryId)}><option value="">Select category</option>{categories.map((category) => <option value={category.categoryId} key={category.categoryId}>{category.name}</option>)}</select>{errors.categoryId && <small className="recruiter-field-error">{errors.categoryId}</small>}</label>
                    <label className="recruiter-form-field"><span>Employment type *</span><select name="employmentType" value={form.employmentType} onChange={handleChange} disabled={Boolean(processing)} aria-invalid={Boolean(errors.employmentType)}><option value="">Select employment type</option>{employmentTypes.map((type) => <option value={type} key={type}>{type}</option>)}</select>{errors.employmentType && <small className="recruiter-field-error">{errors.employmentType}</small>}</label>
                    <label className="recruiter-form-field"><span>Work mode *</span><select name="workMode" value={form.workMode} onChange={handleChange} disabled={Boolean(processing)} aria-invalid={Boolean(errors.workMode)}><option value="">Select work mode</option>{workModes.map((mode) => <option value={mode} key={mode}>{mode}</option>)}</select>{errors.workMode && <small className="recruiter-field-error">{errors.workMode}</small>}</label>
                    <label className="recruiter-form-field"><span>Location</span><input name="location" value={form.location} onChange={handleChange} maxLength="255" disabled={Boolean(processing)} />{errors.location && <small className="recruiter-field-error">{errors.location}</small>}</label>
                    <label className="recruiter-form-field"><span>Minimum salary</span><input name="salaryMin" type="number" min="0" step="0.01" value={form.salaryMin} onChange={handleChange} disabled={Boolean(processing)} />{errors.salaryMin && <small className="recruiter-field-error">{errors.salaryMin}</small>}</label>
                    <label className="recruiter-form-field"><span>Maximum salary</span><input name="salaryMax" type="number" min="0" step="0.01" value={form.salaryMax} onChange={handleChange} disabled={Boolean(processing)} />{errors.salaryMax && <small className="recruiter-field-error">{errors.salaryMax}</small>}</label>
                    <label className="recruiter-form-field"><span>Experience level</span><select name="experienceLevel" value={form.experienceLevel} onChange={handleChange} disabled={Boolean(processing)}><option value="">Not specified</option>{experienceLevels.map((level) => <option value={level} key={level}>{level}</option>)}</select></label>
                    <label className="recruiter-form-field"><span>Application deadline</span><input name="applicationDeadline" type="date" value={form.applicationDeadline} onChange={handleChange} disabled={Boolean(processing)} /></label>
                    <label className="recruiter-form-field recruiter-form-field--wide"><span>Skills</span><select className="recruiter-skill-select" name="skillIds" multiple value={form.skillIds} onChange={handleChange} disabled={Boolean(processing)}>{skills.map((skill) => <option value={skill.skillId} key={skill.skillId}>{skill.skillName}</option>)}</select><small className="recruiter-form-hint">Hold Ctrl (Windows) or Command (Mac) to select multiple skills.</small></label>
                </div>
                <div className="recruiter-setup-actions">
                    <button className="recruiter-secondary-button" type="submit" disabled={Boolean(processing)}>{processing === 'save' ? 'Saving...' : isEdit ? 'Update job' : 'Save as draft'}</button>
                    <button className="recruiter-submit-button" type="button" onClick={() => save(true)} disabled={Boolean(processing)}>{processing === 'publish' ? 'Saving & publishing...' : 'Save & publish'}</button>
                </div>
            </form>
        </section>
    );
};

export default RecruiterJobForm;
