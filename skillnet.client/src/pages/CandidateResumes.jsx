import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { resumeApi } from '../api/resumeApi';
import DashboardCard from '../components/candidate/DashboardCard';
import DeleteConfirmationDialog from '../components/candidate/resume/DeleteConfirmationDialog';
import ResumeCard from '../components/candidate/resume/ResumeCard';
import ResumeFileDialog from '../components/candidate/resume/ResumeFileDialog';
import './CandidateDashboard.css';
import './CandidateResumes.css';

const CandidateResumes = () => {
    const navigate = useNavigate();
    const [resumes, setResumes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [notice, setNotice] = useState(null);
    const [dialog, setDialog] = useState(null);
    const [submitting, setSubmitting] = useState(false);
    const [busy, setBusy] = useState({});

    const loadResumes = useCallback(async () => {
        setLoading(true);
        setLoadError('');
        try {
            const result = await resumeApi.getAll();
            setResumes(Array.isArray(result) ? result : []);
        } catch (error) {
            setLoadError(error.message || 'Your resumes could not be loaded.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        // The resume request intentionally initializes page state on mount.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadResumes();
    }, [loadResumes]);

    const refreshWithNotice = async (message) => {
        await loadResumes();
        setNotice({ type: 'success', message });
    };

    const submitFile = async (file) => {
        setSubmitting(true);
        setNotice(null);
        try {
            if (dialog.mode === 'replace') await resumeApi.replace(dialog.resume.resumeId, file);
            else await resumeApi.upload(file);
            const message = dialog.mode === 'replace' ? 'Resume replaced successfully.' : 'Resume uploaded successfully.';
            setDialog(null);
            await refreshWithNotice(message);
        } catch (error) {
            setNotice({ type: 'error', message: error.message || 'The resume could not be uploaded.' });
        } finally {
            setSubmitting(false);
        }
    };

    const runAction = async (resume, action, operation, message) => {
        setBusy(current => ({ ...current, [resume.resumeId]: action }));
        setNotice(null);
        try {
            await operation();
            await refreshWithNotice(message);
        } catch (error) {
            setNotice({ type: 'error', message: error.message || 'The resume operation failed.' });
        } finally {
            setBusy(current => { const next = { ...current }; delete next[resume.resumeId]; return next; });
        }
    };

    const download = (resume) => runAction(resume, 'download', async () => {
        const { blob, fileName } = await resumeApi.download(resume.resumeId, resume.fileName);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
    }, 'Resume download started.');

    const confirmDelete = async () => {
        const resume = dialog.resume;
        setSubmitting(true);
        try {
            await resumeApi.remove(resume.resumeId);
            setDialog(null);
            await refreshWithNotice('Resume deleted successfully.');
        } catch (error) {
            setNotice({ type: 'error', message: error.message || 'The resume could not be deleted.' });
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="candidate-dashboard-shell">
            <header className="candidate-topbar"><button className="candidate-brand" onClick={() => navigate('/candidate/dashboard')}>Skill<span>Net</span></button><button className="candidate-button candidate-button--ghost" onClick={() => navigate('/candidate/dashboard')}>Dashboard</button></header>
            <main className="candidate-dashboard candidate-resumes-page">
                <header className="resume-page-heading"><div><span className="candidate-eyebrow">Resume library</span><h1>Manage your resumes</h1><p>Keep multiple PDF resumes and choose the one used as your active version.</p></div><button className="candidate-button candidate-button--primary candidate-button--large" onClick={() => setDialog({ mode: 'upload' })}>Upload Resume</button></header>
                {notice && <div className={`resume-notice resume-notice--${notice.type}`} role={notice.type === 'error' ? 'alert' : 'status'}>{notice.message}<button aria-label="Dismiss message" onClick={() => setNotice(null)}>×</button></div>}
                {loading ? <div className="resume-list"><div className="resume-card resume-skeleton skeleton" /><div className="resume-card resume-skeleton skeleton" /></div>
                    : loadError ? <DashboardCard className="dashboard-error"><span className="dashboard-error__icon">!</span><h2>Unable to load resumes</h2><p>{loadError}</p><button className="candidate-button candidate-button--primary" onClick={loadResumes}>Retry</button></DashboardCard>
                    : resumes.length === 0 ? <DashboardCard className="resume-empty-state"><div className="resume-empty-state__icon">PDF</div><h2>Your resume library is empty</h2><p>Upload your first resume to start applying for opportunities.</p><button className="candidate-button candidate-button--primary" onClick={() => setDialog({ mode: 'upload' })}>Upload Resume</button></DashboardCard>
                    : <div className="resume-list">{resumes.map(resume => <ResumeCard key={resume.resumeId} resume={resume} busyAction={busy[resume.resumeId]} onDownload={download} onReplace={item => setDialog({ mode: 'replace', resume: item })} onSetActive={item => runAction(item, 'active', () => resumeApi.setActive(item.resumeId), 'Active resume updated.')} onDelete={item => setDialog({ mode: 'delete', resume: item })} />)}</div>}
            </main>
            {(dialog?.mode === 'upload' || dialog?.mode === 'replace') && <ResumeFileDialog mode={dialog.mode} resume={dialog.resume} submitting={submitting} onClose={() => setDialog(null)} onSubmit={submitFile} />}
            {dialog?.mode === 'delete' && <DeleteConfirmationDialog resume={dialog.resume} deleting={submitting} onClose={() => setDialog(null)} onConfirm={confirmDelete} />}
        </div>
    );
};

export default CandidateResumes;
