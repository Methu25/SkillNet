import React from 'react';

export default function AdminSidebar({ currentTab, setCurrentTab }) {
    const menuItems = [
        { id: 'dashboard', label: '📊 Dashboard' },
        { id: 'users', label: '👥 User Management' },
        { id: 'organizations', label: '🏢 Organizations' },
        { id: 'configs', label: '⚙️ System Settings' },
        { id: 'logs', label: '📜 Audit Logs' }
    ];

    return (
        <div style={{ width: '260px', background: '#1e293b', color: '#fff', height: '100vh', padding: '20px' }}>
            <h2>SkillNet Admin</h2>
            <hr style={{ borderColor: '#334155', margin: '20px 0' }} />
            <ul style={{ listStyle: 'none', padding: 0 }}>
                {menuItems.map((item) => (
                    <li
                        key={item.id}
                        onClick={() => setCurrentTab(item.id)}
                        style={{
                            padding: '12px 16px',
                            cursor: 'pointer',
                            borderRadius: '6px',
                            marginBottom: '8px',
                            background: currentTab === item.id ? '#3b82f6' : 'transparent',
                            transition: 'background 0.2s'
                        }}
                    >
                        {item.label}
                    </li>
                ))}
            </ul>
        </div>
    );
}