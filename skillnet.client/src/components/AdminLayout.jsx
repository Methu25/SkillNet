import React from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import AdminSidebar from './AdminSidebar';
import '../AdminModule.css';

export default function AdminLayout() {
    const { logout } = useAuth();
    const location = useLocation();
    
    // Determine the current tab name from the URL for the header title
    const pathParts = location.pathname.split('/').filter(Boolean);
    const currentTab = pathParts.length > 1 ? pathParts[1] : 'dashboard';
    return (
        <div className="admin-layout">
            <AdminSidebar />
            <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                {/* Top Navbar */}
                <header className="admin-header">
                    <h3 style={{ margin: 0, textTransform: 'capitalize', color: 'var(--text-h)' }}>{currentTab} Management</h3>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                        <div style={{ fontWeight: 'bold', color: 'var(--text)' }}>👤 Administrator</div>
                        <button 
                            className="admin-btn admin-btn-ghost admin-btn-ghost-danger" 
                            onClick={() => logout()}
                        >
                            Log Out
                        </button>
                    </div>
                </header>
                {/* Page Content */}
                <main style={{ padding: '2rem', flex: 1, overflowY: 'auto' }}>
                    <Outlet />
                </main>
            </div>
        </div>
    );
}