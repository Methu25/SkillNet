import React, { useState, useEffect } from 'react';

export default function Dashboard() {
    const [stats, setStats] = useState({
        totalUsers: 0,
        totalCandidates: 0,
        totalRecruiters: 0,
        totalOrganizations: 0,
        totalDepartments: 0,
        recentActivities: []
    });
    const [loading, setLoading] = useState(true);

    // This connects to the C# DashboardController you built earlier!
    useEffect(() => {
        fetch('/api/dashboard/statistics')
            .then(res => res.json())
            .then(data => {
                setStats(data);
                setLoading(false);
            })
            .catch(err => {
                console.error("Error fetching stats:", err);
                setLoading(false);
            });
    }, []);

    if (loading) {
        return <h2>Loading Dashboard Statistics...</h2>;
    }

    // Visual styling for the stat cards
    const cardStyle = {
        background: '#fff',
        padding: '20px',
        borderRadius: '8px',
        boxShadow: '0 4px 6px rgba(0,0,0,0.1)',
        flex: 1,
        margin: '0 10px',
        textAlign: 'center'
    };

    return (
        <div>
            <h2 style={{ marginTop: 0, color: '#000' }}>System Overview</h2>

            {/* Statistics Cards */}
            <div style={{ display: 'flex', justifyContent: 'space-between', margin: '20px -10px', flexWrap: 'wrap', rowGap: '20px' }}>
                <div style={cardStyle}>
                    <h3 style={{ color: '#64748b', margin: '0 0 10px 0' }}>Total Users</h3>
                    <div style={{ fontSize: '32px', fontWeight: 'bold', color: '#3b82f6' }}>{stats.totalUsers}</div>
                </div>

                <div style={cardStyle}>
                    <h3 style={{ color: '#64748b', margin: '0 0 10px 0' }}>Candidates</h3>
                    <div style={{ fontSize: '32px', fontWeight: 'bold', color: '#8b5cf6' }}>{stats.totalCandidates}</div>
                </div>

                <div style={cardStyle}>
                    <h3 style={{ color: '#64748b', margin: '0 0 10px 0' }}>Recruiters</h3>
                    <div style={{ fontSize: '32px', fontWeight: 'bold', color: '#ec4899' }}>{stats.totalRecruiters}</div>
                </div>

                <div style={cardStyle}>
                    <h3 style={{ color: '#64748b', margin: '0 0 10px 0' }}>Organizations</h3>
                    <div style={{ fontSize: '32px', fontWeight: 'bold', color: '#10b981' }}>{stats.totalOrganizations}</div>
                </div>

                <div style={cardStyle}>
                    <h3 style={{ color: '#64748b', margin: '0 0 10px 0' }}>Departments</h3>
                    <div style={{ fontSize: '32px', fontWeight: 'bold', color: '#f59e0b' }}>{stats.totalDepartments}</div>
                </div>
            </div>

            {/* Recent Activity Feed */}
            <div style={{ background: '#fff', padding: '20px', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)', marginTop: '20px', color: '#333' }}>
                <h3 style={{ borderBottom: '1px solid #e2e8f0', paddingBottom: '10px', marginTop: 0 }}>Recent System Activity</h3>
                {stats.recentActivities && stats.recentActivities.length > 0 ? (
                    <ul style={{ paddingLeft: '20px', color: '#334155' }}>
                        {stats.recentActivities.map((action, index) => (
                            <li key={index} style={{ marginBottom: '10px' }}>{action}</li>
                        ))}
                    </ul>
                ) : (
                    <p style={{ color: '#94a3b8' }}>No recent activity to display.</p>
                )}
            </div>
        </div>
    );
}