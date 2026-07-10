import React from 'react';
import AdminSidebar from './AdminSidebar';

export default function AdminLayout({ currentTab, setCurrentTab, children }) {
    return (
        <div style={{ display: 'flex', minHeight: '100vh', background: '#f8fafc' }}>
            <AdminSidebar currentTab={currentTab} setCurrentTab={setCurrentTab} />
            <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                {/* Top Navbar */}
                <header style={{ background: '#fff', padding: '16px 32px', borderBottom: '1px solid #e2e8f0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <h3 style={{ margin: 0, textTransform: 'capitalize' }}>{currentTab} Management</h3>
                    <div style={{ fontWeight: 'bold', color: '#64748b' }}>👤 Administrator</div>
                </header>
                {/* Page Content */}
                <main style={{ padding: '32px', flex: 1 }}>
                    {children}
                </main>
            </div>
        </div>
    );
}