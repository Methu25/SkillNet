import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const getTitle = (pathname) => {
    if (pathname.includes('/jobs/create')) return 'Create Job';
    if (pathname.includes('/jobs/') && pathname.includes('/edit')) return 'Edit Job';
    if (pathname.includes('/jobs/')) return 'Job Details';
    if (pathname.endsWith('/jobs')) return 'Jobs';
    if (pathname.includes('/company') || pathname.includes('/setup')) return 'Organization Profile';
    if (pathname.includes('/settings')) return 'Settings';
    return 'Dashboard';
};

const RecruiterHeader = ({ onMenu }) => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const initials = `${user?.firstName?.[0] || ''}${user?.lastName?.[0] || ''}` || 'R';

    const handleLogout = async () => {
        await logout();
        navigate('/login', { replace: true });
    };

    return (
        <header className="recruiter-header">
            <div className="recruiter-header-title">
                <button type="button" className="recruiter-menu-button" onClick={onMenu} aria-label="Open recruiter navigation">☰</button>
                <div><span>Recruiter workspace</span><h1>{getTitle(location.pathname)}</h1></div>
            </div>
            <div className="recruiter-header-actions">
                <div className="recruiter-user">
                    <span className="recruiter-avatar" aria-hidden="true">{initials}</span>
                    <div><strong>{user?.firstName || 'Recruiter'} {user?.lastName || ''}</strong><span>{user?.email}</span></div>
                </div>
                <button type="button" className="recruiter-logout" onClick={handleLogout}>Logout</button>
            </div>
        </header>
    );
};

export default RecruiterHeader;
