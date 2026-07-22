import { NavLink, useLocation } from 'react-router-dom';

const links = [
    { to: '/recruiter/dashboard', label: 'Dashboard', icon: 'DB', end: true },
    { to: '/recruiter/company', label: 'Organization Profile', icon: 'OP' },
    { to: '/recruiter/jobs', label: 'Jobs', icon: 'JB' },
    { to: '/recruiter/settings', label: 'Settings', icon: 'ST' }
];

const RecruiterSidebar = ({ open, onClose }) => {
    const location = useLocation();

    return (
        <>
        <button
            type="button"
            className={`recruiter-sidebar-backdrop${open ? ' is-open' : ''}`}
            aria-label="Close navigation"
            onClick={onClose}
        />
        <aside className={`recruiter-sidebar${open ? ' is-open' : ''}`}>
            <NavLink className="recruiter-brand" to="/recruiter/dashboard" onClick={onClose} aria-label="SkillNet recruiter dashboard">Skill<span>Net</span></NavLink>
            <div className="recruiter-sidebar-label">Recruiter workspace</div>
            <nav aria-label="Recruiter navigation">
                {links.map(link => (
                    <NavLink
                        key={link.to}
                        to={link.to}
                        end={link.end}
                        onClick={onClose}
                        className={({ isActive }) => `recruiter-nav-link${isActive || (link.to === '/recruiter/company' && location.pathname.includes('/recruiter/setup')) ? ' is-active' : ''}`}
                    >
                        <span className="recruiter-nav-icon" aria-hidden="true">{link.icon}</span>
                        {link.label}
                    </NavLink>
                ))}
            </nav>
            <div className="recruiter-sidebar-footer"><strong>SkillNet Recruiter</strong><span>Build your team with confidence.</span></div>
        </aside>
        </>
    );
};

export default RecruiterSidebar;
