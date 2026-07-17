import { useEffect, useRef, useState } from 'react';

const MAX_FILE_SIZE = 10 * 1024 * 1024;

const validatePdf = (file) => {
    if (!file || file.size === 0) return 'Choose a non-empty PDF file.';
    if (!file.name.toLowerCase().endsWith('.pdf') || file.type !== 'application/pdf') return 'Only PDF files are supported.';
    if (file.size > MAX_FILE_SIZE) return 'The PDF must be 10 MB or smaller.';
    return '';
};

const ResumeFileDialog = ({ mode, resume, submitting, onClose, onSubmit }) => {
    const dialogRef = useRef(null);
    const [file, setFile] = useState(null);
    const [error, setError] = useState('');

    useEffect(() => {
        const dialog = dialogRef.current;
        dialog?.showModal();
        return () => dialog?.open && dialog.close();
    }, []);

    const selectFile = (event) => {
        const selected = event.target.files?.[0] || null;
        setFile(selected);
        setError(selected ? validatePdf(selected) : 'Choose a PDF file.');
    };

    const submit = (event) => {
        event.preventDefault();
        const validation = validatePdf(file);
        if (validation) return setError(validation);
        onSubmit(file);
    };

    const replacing = mode === 'replace';
    return (
        <dialog className="resume-dialog" ref={dialogRef} onCancel={(event) => { event.preventDefault(); if (!submitting) onClose(); }}>
            <form onSubmit={submit}>
                <span className="candidate-eyebrow">{replacing ? 'Replace resume' : 'Upload resume'}</span>
                <h2>{replacing ? `Replace ${resume.fileName}?` : 'Add a new resume'}</h2>
                <p>{replacing ? 'The new PDF will replace this file while the backend preserves its active status.' : 'Upload a PDF resume. Your first resume will become active automatically.'}</p>
                <label className="resume-file-picker">
                    <span>{file ? file.name : 'Choose PDF file'}</span>
                    <input type="file" accept="application/pdf,.pdf" onChange={selectFile} disabled={submitting} />
                    {file && <small>{(file.size / (1024 * 1024)).toFixed(2)} MB</small>}
                </label>
                {error && <div className="resume-dialog__error" role="alert">{error}</div>}
                {replacing && <p className="resume-confirm-copy">Confirming will replace the current stored resume.</p>}
                <div className="resume-dialog__actions">
                    <button type="button" className="candidate-button candidate-button--ghost" onClick={onClose} disabled={submitting}>Cancel</button>
                    <button className="candidate-button candidate-button--primary" disabled={submitting || !file}>{submitting ? 'Uploading…' : replacing ? 'Confirm Replace' : 'Upload Resume'}</button>
                </div>
            </form>
        </dialog>
    );
};

export default ResumeFileDialog;
