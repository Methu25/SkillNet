import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import AdminSidebar from './AdminSidebar';
import { useAuth } from '../context/AuthContext';
import '../AdminModule.css';

export default function AdminLayout({ currentTab: propTab }) {
    const { logout } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();

    const pathParts = location.pathname.split('/').filter(Boolean);
    const currentTab = propTab || (pathParts.length > 1 ? pathParts[1] : 'dashboard');

    const handleLogout = async () => {
        await logout();
        navigate('/login', { replace: true });
    };

    return (
        <div className="admin-layout">
            <AdminSidebar />
            <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                <header className="admin-header">
                    <h3 style={{ margin: 0, textTransform: 'capitalize', color: 'var(--text-h)' }}>{currentTab} Management</h3>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                        <div style={{ fontWeight: 'bold', color: 'var(--text)' }}>👤 Administrator</div>
                        <button type="button" className="admin-btn admin-btn-ghost admin-btn-ghost-danger" onClick={handleLogout}>
                            Log Out
                        </button>
                    </div>
                </header>
                <main style={{ padding: '2rem', flex: 1, overflowY: 'auto' }}>
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
