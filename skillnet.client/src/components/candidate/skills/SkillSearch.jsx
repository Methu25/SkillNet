const SkillSearch = ({ value, onChange }) => (
    <label className="skill-search">
        <span className="sr-only">Search available skills</span>
        <span aria-hidden="true">⌕</span>
        <input
            type="search"
            value={value}
            onChange={event => onChange(event.target.value)}
            placeholder="Search available skills"
        />
        {value && <button type="button" onClick={() => onChange('')} aria-label="Clear skill search">×</button>}
    </label>
);

export default SkillSearch;
