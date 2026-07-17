import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const links = [
    ['Dashboard', '/candidate/dashboard'],
    ['My Profile', '/candidate/profile'],
    ['Resumes', '/candidate/resumes'],
    ['Skills', '/candidate/skills']
];

const CandidateNavigation = () => {
    const { logout } = useAuth();
    const navigate = useNavigate();

    const signOut = async () => {
        await logout();
        navigate('/login', { replace: true });
    };

    return (
        <header className="candidate-topbar">
            <button className="candidate-brand" onClick={() => navigate('/candidate/dashboard')}>Skill<span>Net</span></button>
            <nav className="candidate-navigation" aria-label="Candidate navigation">
                {links.map(([label, path]) => <NavLink key={path} to={path} className={({ isActive }) => isActive ? 'is-active' : ''}>{label}</NavLink>)}
                <span className="candidate-nav-placeholder" title="Applications module integration point">Applications</span>
            </nav>
            <button className="candidate-button candidate-button--ghost candidate-logout" onClick={signOut}>Logout</button>
        </header>
    );
};

export default CandidateNavigation;
