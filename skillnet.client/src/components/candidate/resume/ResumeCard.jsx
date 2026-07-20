const formatSize = (bytes = 0) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};

const ResumeCard = ({ resume, busyAction, onDownload, onReplace, onSetActive, onDelete }) => {
    const busy = Boolean(busyAction);
    return (
        <article className={`resume-card${resume.isActive ? ' resume-card--active' : ''}`}>
            <div className="resume-card__icon" aria-hidden="true">PDF</div>
            <div className="resume-card__details">
                <div className="resume-card__titleline">
                    <h2>{resume.fileName}</h2>
                    {resume.isActive && <span className="resume-active-badge">Active resume</span>}
                </div>
                <dl className="resume-metadata">
                    <div><dt>Type</dt><dd>{resume.fileType || 'application/pdf'}</dd></div>
                    <div><dt>Size</dt><dd>{formatSize(resume.fileSize)}</dd></div>
                    <div><dt>Uploaded</dt><dd>{new Date(resume.uploadedDate).toLocaleDateString()}</dd></div>
                </dl>
                <div className="resume-card__actions">
                    <button className="candidate-button candidate-button--secondary" disabled={busy} onClick={() => onDownload(resume)}>{busyAction === 'download' ? 'Downloading…' : 'Download'}</button>
                    <button className="candidate-button candidate-button--ghost" disabled={busy} onClick={() => onReplace(resume)}>Replace</button>
                    {!resume.isActive && <button className="candidate-button candidate-button--ghost" disabled={busy} onClick={() => onSetActive(resume)}>{busyAction === 'active' ? 'Updating…' : 'Set Active'}</button>}
                    <button className="candidate-button resume-delete-button" disabled={busy} onClick={() => onDelete(resume)}>Delete</button>
                </div>
            </div>
        </article>
    );
};

export default ResumeCard;
