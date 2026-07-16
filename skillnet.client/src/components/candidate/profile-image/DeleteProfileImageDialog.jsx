import { useEffect, useRef } from 'react';

const DeleteProfileImageDialog = ({ deleting, error, onClose, onConfirm }) => {
    const dialogRef = useRef(null);
    useEffect(() => {
        const dialog = dialogRef.current;
        dialog?.showModal();
        return () => dialog?.open && dialog.close();
    }, []);

    return (
        <dialog className="profile-image-dialog" ref={dialogRef} onCancel={event => { event.preventDefault(); if (!deleting) onClose(); }}>
            <div>
                <span className="candidate-eyebrow">Delete picture</span>
                <h2>Return to the default avatar?</h2>
                <p>Your profile picture will be removed. Your candidate profile and account will not be affected.</p>
                {error && <div className="profile-image-error" role="alert">{error}</div>}
                <div className="profile-image-dialog__actions">
                    <button className="candidate-button candidate-button--ghost" disabled={deleting} onClick={onClose}>Cancel</button>
                    <button className="candidate-button profile-image-delete" disabled={deleting} onClick={onConfirm}>{deleting ? 'Deleting…' : 'Delete Picture'}</button>
                </div>
            </div>
        </dialog>
    );
};

export default DeleteProfileImageDialog;
