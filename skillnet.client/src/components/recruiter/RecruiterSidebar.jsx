import { NavLink } from 'react-router-dom';

const links = [
    { to: '/recruiter/dashboard', label: 'Dashboard', icon: 'D', end: true },
    { to: '/recruiter/jobs', label: 'Jobs', icon: 'J' },
    { to: '/recruiter/company', label: 'Company', icon: 'C' },
    { to: '/recruiter/settings', label: 'Settings', icon: 'S' }
];

const RecruiterSidebar = ({ open, onClose }) => (
    <>
        <button
            type="button"
            className={`recruiter-sidebar-backdrop${open ? ' is-open' : ''}`}
            aria-label="Close navigation"
            onClick={onClose}
        />
        <aside className={`recruiter-sidebar${open ? ' is-open' : ''}`}>
            <div className="recruiter-brand">Skill<span>Net</span></div>
            <div className="recruiter-sidebar-label">Recruiter workspace</div>
            <nav aria-label="Recruiter navigation">
                {links.map(link => (
                    <NavLink
                        key={link.to}
                        to={link.to}
                        end={link.end}
                        onClick={onClose}
                        className={({ isActive }) => `recruiter-nav-link${isActive ? ' is-active' : ''}`}
                    >
                        <span className="recruiter-nav-icon" aria-hidden="true">{link.icon}</span>
                        {link.label}
                    </NavLink>
                ))}
            </nav>
            <NavLink className="recruiter-create-link" to="/recruiter/jobs/create" onClick={onClose}>
                <span aria-hidden="true">+</span> Create job
            </NavLink>
            <div className="recruiter-sidebar-footer">Recruit smarter. Hire better.</div>
        </aside>
    </>
);

export default RecruiterSidebar;
