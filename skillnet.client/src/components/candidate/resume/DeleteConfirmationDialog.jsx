import { useEffect, useRef } from 'react';

const DeleteConfirmationDialog = ({ resume, deleting, onClose, onConfirm }) => {
    const dialogRef = useRef(null);
    useEffect(() => {
        const dialog = dialogRef.current;
        dialog?.showModal();
        return () => dialog?.open && dialog.close();
    }, []);

    return (
        <dialog className="resume-dialog" ref={dialogRef} onCancel={(event) => { event.preventDefault(); if (!deleting) onClose(); }}>
            <div>
                <span className="candidate-eyebrow">Delete resume</span>
                <h2>Remove {resume.fileName}?</h2>
                <p>This resume will be permanently removed. If it is active, the backend may select another resume as active.</p>
                <div className="resume-dialog__actions">
                    <button className="candidate-button candidate-button--ghost" onClick={onClose} disabled={deleting}>Cancel</button>
                    <button className="candidate-button resume-delete-button" onClick={onConfirm} disabled={deleting}>{deleting ? 'Deleting…' : 'Delete Resume'}</button>
                </div>
            </div>
        </dialog>
    );
};

export default DeleteConfirmationDialog;
