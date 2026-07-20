const AvailableSkillList = ({ skills, assignedIds, addingId, onAdd }) => {
    if (skills.length === 0) {
        return <div className="skills-search-empty">No skills match your search.</div>;
    }

    return (
        <div className="available-skill-list">
            {skills.map(skill => {
                const assigned = assignedIds.has(skill.skillId);
                return (
                    <div className={`available-skill${assigned ? ' available-skill--assigned' : ''}`} key={skill.skillId}>
                        <span>{skill.skillName}</span>
                        <button
                            type="button"
                            className="candidate-button candidate-button--secondary"
                            disabled={assigned || addingId !== null}
                            onClick={() => onAdd(skill)}
                        >
                            {assigned ? 'Added' : addingId === skill.skillId ? 'Adding…' : 'Add'}
                        </button>
                    </div>
                );
            })}
        </div>
    );
};

export default AvailableSkillList;
