import { useEffect, useRef, useState } from 'react';

const MAX_COVER_LETTER_LENGTH = 2000;

const ApplyJobDialog = ({ job, resumes, loadingResumes, loadError, submitting, submitError, success, onClose, onSubmit, onManageResumes, onViewApplications }) => {
    const dialogRef = useRef(null);
    const [resumeId, setResumeId] = useState('');
    const [coverLetter, setCoverLetter] = useState('');

    useEffect(() => {
        const dialog = dialogRef.current;
        dialog?.showModal();
        return () => dialog?.open && dialog.close();
    }, []);

    const selectedResumeId = resumeId || String(
        resumes.find(resume => resume.isActive)?.resumeId || ''
    );

    const submit = (event) => {
        event.preventDefault();
        if (!selectedResumeId) return;
        onSubmit({
            jobId: job.jobId,
            resumeId: Number(selectedResumeId),
            coverLetter: coverLetter.trim() || null
        });
    };

    return (
        <dialog className="job-apply-dialog" ref={dialogRef} onCancel={(event) => { event.preventDefault(); if (!submitting) onClose(); }}>
            {success ? (
                <div className="job-apply-success">
                    <span className="job-apply-success__icon">✓</span>
                    <span className="candidate-eyebrow">Application submitted</span>
                    <h2>Your application is on its way</h2>
                    <p>You successfully applied for <strong>{job.title}</strong>.</p>
                    <div className="job-apply-dialog__actions">
                        <button className="candidate-button candidate-button--ghost" onClick={onClose}>Continue browsing</button>
                        <button className="candidate-button candidate-button--primary" onClick={onViewApplications}>View Applications</button>
                    </div>
                </div>
            ) : (
                <form onSubmit={submit}>
                    <span className="candidate-eyebrow">Apply for this role</span>
                    <h2>{job.title}</h2>
                    <p>{job.organizationName || job.location || 'SkillNet opportunity'}</p>

                    {loadingResumes ? <div className="job-apply-loading skeleton" />
                        : loadError ? <div className="job-apply-error" role="alert">{loadError}</div>
                        : resumes.length === 0 ? <div className="job-no-resume"><strong>No resume available</strong><p>Upload a resume before applying for this role.</p><button type="button" className="candidate-button candidate-button--primary" onClick={onManageResumes}>Upload Resume</button></div>
                        : <>
                            <fieldset className="job-resume-options">
                                <legend>Select a resume <span>Required</span></legend>
                                {resumes.map(resume => <label key={resume.resumeId} className={String(resume.resumeId) === selectedResumeId ? 'is-selected' : ''}>
                                    <input type="radio" name="resume" value={resume.resumeId} checked={String(resume.resumeId) === selectedResumeId} onChange={(event) => setResumeId(event.target.value)} disabled={submitting} />
                                    <span className="job-resume-icon">PDF</span>
                                    <span><strong>{resume.fileName}</strong><small>{resume.isActive ? 'Active resume' : `Uploaded ${new Date(resume.uploadedDate).toLocaleDateString()}`}</small></span>
                                </label>)}
                            </fieldset>
                            <label className="job-cover-letter" htmlFor="application-cover-letter">Cover letter <span>Optional</span></label>
                            <textarea id="application-cover-letter" rows="6" maxLength={MAX_COVER_LETTER_LENGTH} value={coverLetter} onChange={(event) => setCoverLetter(event.target.value)} placeholder="Tell the recruiter why this opportunity interests you." disabled={submitting} />
                            <small className="job-cover-letter-count">{coverLetter.length}/{MAX_COVER_LETTER_LENGTH}</small>
                        </>}

                    {submitError && <div className="job-apply-error" role="alert">{submitError}</div>}
                    <div className="job-apply-dialog__actions">
                        <button type="button" className="candidate-button candidate-button--ghost" onClick={onClose} disabled={submitting}>Cancel</button>
                        {resumes.length > 0 && !loadError && <button className="candidate-button candidate-button--primary" disabled={submitting || !selectedResumeId}>{submitting ? 'Submitting…' : 'Submit Application'}</button>}
                    </div>
                </form>
            )}
        </dialog>
    );
};

export default ApplyJobDialog;
