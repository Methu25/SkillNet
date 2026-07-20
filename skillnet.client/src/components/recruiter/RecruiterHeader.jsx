import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useRecruiter } from '../../context/RecruiterContext';

const getTitle = (pathname) => {
    if (pathname.includes('/jobs/create')) return 'Create job';
    if (pathname.includes('/edit')) return 'Edit job';
    if (/\/jobs\/[^/]+$/.test(pathname)) return 'Job details';
    if (pathname.includes('/jobs')) return 'Jobs';
    if (pathname.includes('/company')) return 'Company profile';
    if (pathname.includes('/setup')) return 'Organization setup';
    if (pathname.includes('/pending')) return 'Verification';
    if (pathname.includes('/settings')) return 'Settings';
    return 'Dashboard';
};

const RecruiterHeader = ({ onMenu }) => {
    const { user, logout } = useAuth();
    const { approvalStatus } = useRecruiter();
    const location = useLocation();
    const navigate = useNavigate();
    const initials = `${user?.firstName?.[0] || ''}${user?.lastName?.[0] || ''}` || 'R';

    const handleLogout = async () => {
        await logout();
        navigate('/login', { replace: true });
    };

    return (
        <header className="recruiter-header">
            <div className="recruiter-header-title">
                <button type="button" className="recruiter-menu-button" onClick={onMenu} aria-label="Open navigation">☰</button>
                <div><span>Recruiter workspace</span><h1>{getTitle(location.pathname)}</h1></div>
            </div>
            <div className="recruiter-header-actions">
                {approvalStatus && <span className={`recruiter-status recruiter-status--${approvalStatus.toLowerCase()}`}>{approvalStatus}</span>}
                <div className="recruiter-user">
                    <span className="recruiter-avatar">{initials}</span>
                    <div><strong>{user?.firstName || 'Recruiter'} {user?.lastName || ''}</strong><span>{user?.email}</span></div>
                </div>
                <button type="button" className="recruiter-logout" onClick={handleLogout}>Log out</button>
            </div>
        </header>
    );
};

export default RecruiterHeader;
