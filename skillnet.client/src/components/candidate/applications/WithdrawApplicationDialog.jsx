import { useEffect, useRef, useState } from 'react';

const MAX_REASON_LENGTH = 2000;

const WithdrawApplicationDialog = ({ application, withdrawing, error, onClose, onConfirm }) => {
    const dialogRef = useRef(null);
    const [reason, setReason] = useState('');

    useEffect(() => {
        const dialog = dialogRef.current;
        dialog?.showModal();
        return () => dialog?.open && dialog.close();
    }, []);

    const submit = (event) => {
        event.preventDefault();
        onConfirm(reason);
    };

    return (
        <dialog
            className="application-dialog"
            ref={dialogRef}
            onCancel={(event) => {
                event.preventDefault();
                if (!withdrawing) onClose();
            }}
        >
            <form onSubmit={submit}>
                <span className="candidate-eyebrow">Withdraw application</span>
                <h2>Withdraw from {application.jobTitle || 'this role'}?</h2>
                <p>This action updates your application status to Withdrawn. You cannot reverse it from this page.</p>
                <label htmlFor="withdraw-reason">Reason <span>(optional)</span></label>
                <textarea
                    id="withdraw-reason"
                    maxLength={MAX_REASON_LENGTH}
                    rows="5"
                    value={reason}
                    onChange={(event) => setReason(event.target.value)}
                    placeholder="Share a brief reason if you would like to."
                    disabled={withdrawing}
                />
                <small>{reason.length}/{MAX_REASON_LENGTH}</small>
                {error && <div className="application-dialog__error" role="alert">{error}</div>}
                <div className="application-dialog__actions">
                    <button type="button" className="candidate-button candidate-button--ghost" onClick={onClose} disabled={withdrawing}>Keep Application</button>
                    <button className="candidate-button application-withdraw-button" disabled={withdrawing}>
                        {withdrawing ? 'Withdrawing…' : 'Confirm Withdrawal'}
                    </button>
                </div>
            </form>
        </dialog>
    );
};

export default WithdrawApplicationDialog;
