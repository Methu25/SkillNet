import { useCallback, useEffect, useMemo, useState } from 'react';
import { skillApi } from '../api/skillApi';
import DashboardCard from '../components/candidate/DashboardCard';
import AvailableSkillList from '../components/candidate/skills/AvailableSkillList';
import RemoveSkillDialog from '../components/candidate/skills/RemoveSkillDialog';
import SkillChip from '../components/candidate/skills/SkillChip';
import SkillSearch from '../components/candidate/skills/SkillSearch';
import CandidateNavigation from '../components/candidate/CandidateNavigation';
import './CandidateDashboard.css';
import './CandidateSkills.css';

const CandidateSkills = () => {
    const [candidateSkills, setCandidateSkills] = useState([]);
    const [availableSkills, setAvailableSkills] = useState([]);
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [notice, setNotice] = useState(null);
    const [addingId, setAddingId] = useState(null);
    const [removeSkill, setRemoveSkill] = useState(null);
    const [removing, setRemoving] = useState(false);

    const loadSkills = useCallback(async (showLoader = true) => {
        if (showLoader) setLoading(true);
        setLoadError('');
        try {
            const [assigned, available] = await Promise.all([
                skillApi.getCandidateSkills(),
                skillApi.getAvailableSkills()
            ]);
            setCandidateSkills(Array.isArray(assigned) ? assigned : []);
            setAvailableSkills(Array.isArray(available) ? available : []);
        } catch (error) {
            setLoadError(error.message || 'Your skills could not be loaded.');
        } finally {
            if (showLoader) setLoading(false);
        }
    }, []);

    useEffect(() => {
        // The skill requests intentionally initialize page state on mount.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadSkills();
    }, [loadSkills]);

    const assignedIds = useMemo(() => new Set(candidateSkills.map(skill => skill.skillId)), [candidateSkills]);
    const filteredSkills = useMemo(() => {
        const query = search.trim().toLocaleLowerCase();
        return availableSkills
            .filter(skill => !query || skill.skillName.toLocaleLowerCase().includes(query))
            .sort((left, right) => left.skillName.localeCompare(right.skillName));
    }, [availableSkills, search]);

    const addSkill = async (skill) => {
        setAddingId(skill.skillId);
        setNotice(null);
        try {
            await skillApi.addSkill(skill.skillId);
            await loadSkills(false);
            setNotice({ type: 'success', message: `${skill.skillName} was added to your profile.` });
        } catch (error) {
            setNotice({ type: 'error', message: error.message || 'The skill could not be added.' });
        } finally {
            setAddingId(null);
        }
    };

    const confirmRemove = async () => {
        const skill = removeSkill;
        setRemoving(true);
        setNotice(null);
        try {
            await skillApi.removeSkill(skill.skillId);
            setRemoveSkill(null);
            await loadSkills(false);
            setNotice({ type: 'success', message: `${skill.skillName} was removed from your profile.` });
        } catch (error) {
            setNotice({ type: 'error', message: error.message || 'The skill could not be removed.' });
        } finally {
            setRemoving(false);
        }
    };

    return (
        <div className="candidate-dashboard-shell">
            <CandidateNavigation />
            <main className="candidate-dashboard candidate-skills-page">
                <header className="skills-page-heading">
                    <div><span className="candidate-eyebrow">Professional skills</span><h1>Manage your skills</h1><p>Add relevant skills to strengthen your profile and help recruiters understand your capabilities.</p></div>
                    <button className="candidate-button candidate-button--primary candidate-button--large" onClick={() => document.getElementById('available-skills')?.scrollIntoView({ behavior: 'smooth' })}>Add Skills</button>
                </header>

                {notice && <div className={`skills-notice skills-notice--${notice.type}`} role={notice.type === 'error' ? 'alert' : 'status'}>{notice.message}<button onClick={() => setNotice(null)} aria-label="Dismiss message">×</button></div>}

                {loading ? <div className="skills-layout"><div className="skills-skeleton skeleton" /><div className="skills-skeleton skeleton" /></div>
                    : loadError ? <DashboardCard className="dashboard-error"><span className="dashboard-error__icon">!</span><h2>Unable to load skills</h2><p>{loadError}</p><button className="candidate-button candidate-button--primary" onClick={() => loadSkills()}>Retry</button></DashboardCard>
                    : <div className="skills-layout">
                        <DashboardCard title={`Your skills (${candidateSkills.length})`} className="current-skills-card">
                            <p className="skills-helper-copy">Adding relevant skills contributes to your backend-calculated profile completion.</p>
                            {candidateSkills.length === 0
                                ? <div className="current-skills-empty"><span>+</span><p>You have not added any skills yet.</p><button className="candidate-button candidate-button--primary" onClick={() => document.getElementById('available-skills')?.scrollIntoView({ behavior: 'smooth' })}>Add Skills</button></div>
                                : <div className="candidate-skill-list">{candidateSkills.map(skill => <SkillChip key={skill.skillId} skill={skill} removing={removing && removeSkill?.skillId === skill.skillId} onRemove={setRemoveSkill} />)}</div>}
                        </DashboardCard>
                        <DashboardCard title="Available skills" className="available-skills-card" >
                            <div id="available-skills" className="available-skills-anchor" />
                            <SkillSearch value={search} onChange={setSearch} />
                            <AvailableSkillList skills={filteredSkills} assignedIds={assignedIds} addingId={addingId} onAdd={addSkill} />
                        </DashboardCard>
                    </div>}
            </main>
            {removeSkill && <RemoveSkillDialog skill={removeSkill} removing={removing} onClose={() => setRemoveSkill(null)} onConfirm={confirmRemove} />}
        </div>
    );
};

export default CandidateSkills;
