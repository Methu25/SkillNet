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
                    <div style={{ fontWeight: 'bold', color: 'var(--text)' }}>👤 Administrator</div>
                </header>
                {/* Page Content */}
                <main style={{ padding: '2rem', flex: 1, overflowY: 'auto' }}>
                    {children}
                </main>
            </div>
        </div>
    );
}