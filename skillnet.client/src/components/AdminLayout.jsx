import React from 'react';
import AdminSidebar from './AdminSidebar';
import '../AdminModule.css';

export default function AdminLayout({ currentTab, setCurrentTab, children }) {
    return (
        <div className="admin-layout">
            <AdminSidebar currentTab={currentTab} setCurrentTab={setCurrentTab} />
            <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                {/* Top Navbar */}
                <header className="admin-header">
                    <h3 style={{ margin: 0, textTransform: 'capitalize', color: 'var(--text-h)' }}>{currentTab} Management</h3>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                        <div style={{ fontWeight: 'bold', color: 'var(--text)' }}>👤 Administrator</div>
                        <button 
                            className="admin-btn admin-btn-ghost admin-btn-ghost-danger" 
                            onClick={() => {
                                // Standard way to clear auth tokens for JWT authentication
                                localStorage.clear();
                                sessionStorage.clear();
                                window.location.href = '/'; // Redirects to the root/login page
                            }}
                        >
                            Log Out
                        </button>
                    </div>
                </header>
                {/* Page Content */}
                <main style={{ padding: '2rem', flex: 1, overflowY: 'auto' }}>
                    {children}
                </main>
            </div>
        </div>
    );
}