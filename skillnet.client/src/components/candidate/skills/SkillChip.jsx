const SkillChip = ({ skill, removing, onRemove }) => (
    <div className="candidate-skill-chip">
        <span>{skill.skillName}</span>
        <button
            type="button"
            aria-label={`Remove ${skill.skillName}`}
            title={`Remove ${skill.skillName}`}
            disabled={removing}
            onClick={() => onRemove(skill)}
        >
            {removing ? '…' : '×'}
        </button>
    </div>
);

export default SkillChip;
