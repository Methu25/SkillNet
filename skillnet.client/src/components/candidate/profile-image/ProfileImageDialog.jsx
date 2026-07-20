import { useEffect, useRef, useState } from 'react';

const MAX_SIZE = 5 * 1024 * 1024;
const allowedTypes = new Set(['image/jpeg', 'image/png', 'image/webp']);
const allowedExtensions = new Set(['jpg', 'jpeg', 'png', 'webp']);

const validateImage = (file) => {
    if (!file || file.size === 0) return 'Choose a non-empty image.';
    const extension = file.name.split('.').pop()?.toLowerCase();
    if (!allowedTypes.has(file.type) || !allowedExtensions.has(extension)) return 'Choose a JPEG, PNG, or WEBP image.';
    if (file.size > MAX_SIZE) return 'The image must be 5 MB or smaller.';
    return '';
};

const ProfileImageDialog = ({ replacing, uploading, requestError, onClose, onUpload }) => {
    const dialogRef = useRef(null);
    const [file, setFile] = useState(null);
    const [previewUrl, setPreviewUrl] = useState('');
    const [validationError, setValidationError] = useState('');

    useEffect(() => {
        const dialog = dialogRef.current;
        dialog?.showModal();
        return () => dialog?.open && dialog.close();
    }, []);

    useEffect(() => () => { if (previewUrl) URL.revokeObjectURL(previewUrl); }, [previewUrl]);

    const chooseFile = (event) => {
        const selected = event.target.files?.[0] || null;
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        setFile(selected);
        setValidationError(selected ? validateImage(selected) : 'Choose an image.');
        setPreviewUrl(selected ? URL.createObjectURL(selected) : '');
    };

    const submit = (event) => {
        event.preventDefault();
        const error = validateImage(file);
        if (error) return setValidationError(error);
        onUpload(file);
    };

    return (
        <dialog className="profile-image-dialog" ref={dialogRef} onCancel={event => { event.preventDefault(); if (!uploading) onClose(); }}>
            <form onSubmit={submit}>
                <span className="candidate-eyebrow">{replacing ? 'Change picture' : 'Upload picture'}</span>
                <h2>{replacing ? 'Choose a new profile picture' : 'Add your profile picture'}</h2>
                <p>{replacing ? 'Your current picture will be replaced after the new image is saved.' : 'Use a clear JPEG, PNG, or WEBP image up to 5 MB.'}</p>
                <div className="profile-image-preview">
                    {previewUrl ? <img src={previewUrl} alt="Selected profile preview" /> : <span>Preview</span>}
                </div>
                <label className="profile-image-picker">
                    <span>{file ? file.name : 'Select an image'}</span>
                    <input type="file" accept="image/jpeg,image/png,image/webp,.jpg,.jpeg,.png,.webp" onChange={chooseFile} disabled={uploading} />
                    {file && <small>{(file.size / (1024 * 1024)).toFixed(2)} MB</small>}
                </label>
                {(validationError || requestError) && <div className="profile-image-error" role="alert">{validationError || requestError}</div>}
                <div className="profile-image-dialog__actions">
                    <button type="button" className="candidate-button candidate-button--ghost" disabled={uploading} onClick={onClose}>Cancel</button>
                    <button className="candidate-button candidate-button--primary" disabled={uploading || !file}>{uploading ? 'Uploading…' : replacing ? 'Change Picture' : 'Upload Picture'}</button>
                </div>
            </form>
        </dialog>
    );
};

export default ProfileImageDialog;
