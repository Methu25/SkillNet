import { useEffect, useRef } from 'react';

const RemoveSkillDialog = ({ skill, removing, onClose, onConfirm }) => {
    const dialogRef = useRef(null);
    useEffect(() => {
        const dialog = dialogRef.current;
        dialog?.showModal();
        return () => dialog?.open && dialog.close();
    }, []);

    return (
        <dialog className="skill-dialog" ref={dialogRef} onCancel={event => { event.preventDefault(); if (!removing) onClose(); }}>
            <div>
                <span className="candidate-eyebrow">Remove skill</span>
                <h2>Remove {skill.skillName}?</h2>
                <p>This removes the skill from your candidate profile. It does not delete the skill from SkillNet.</p>
                <div className="skill-dialog__actions">
                    <button type="button" className="candidate-button candidate-button--ghost" disabled={removing} onClick={onClose}>Cancel</button>
                    <button type="button" className="candidate-button skill-remove-button" disabled={removing} onClick={onConfirm}>{removing ? 'Removing…' : 'Remove Skill'}</button>
                </div>
            </div>
        </dialog>
    );
};

export default RemoveSkillDialog;
