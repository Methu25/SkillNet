import React, { useState, useEffect } from 'react';
import { apiRequest } from '../api/apiClient';
import '../AdminModule.css';

export default function Dashboard() {
    const [stats, setStats] = useState({
        totalUsers: 0,
        totalCandidates: 0,
        totalRecruiters: 0,
        totalOrganizations: 0,
        totalDepartments: 0,
        activeJobs: 0,
        applicationsToday: 0,
        interviewsToday: 0,
        hiresThisMonth: 0,
        recentActivities: []
    });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        apiRequest('/api/dashboard/statistics')
            .then(res => {
                setStats(res.data);
                setLoading(false);
            })
            .catch(err => {
                console.error("Error fetching stats:", err);
                setLoading(false);
            });
    }, []);

    if (loading) {
        return <h2 className="admin-page-title">Loading Dashboard Statistics...</h2>;
    }

    return (
        <div className="admin-module-container">
            <h2 className="admin-page-title">System Overview</h2>

            {/* Statistics Cards */}
            <div className="admin-grid-stats">
                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'var(--accent-bg)', borderRadius: '12px', color: 'var(--accent)' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Total Users</h3>
                        <div className="admin-stat-value" style={{ color: 'var(--accent)' }}>{stats.totalUsers}</div>
                    </div>
                </div>

                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'rgba(255, 122, 77, 0.15)', borderRadius: '12px', color: '#ff7a4d' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="8.5" cy="7" r="4"></circle><line x1="20" y1="8" x2="20" y2="14"></line><line x1="23" y1="11" x2="17" y2="11"></line></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Candidates</h3>
                        <div className="admin-stat-value" style={{ color: '#ff7a4d' }}>{stats.totalCandidates}</div>
                    </div>
                </div>

                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'rgba(255, 159, 115, 0.15)', borderRadius: '12px', color: '#ff9f73' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"></rect><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"></path></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Recruiters</h3>
                        <div className="admin-stat-value" style={{ color: '#ff9f73' }}>{stats.totalRecruiters}</div>
                    </div>
                </div>

                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'rgba(255, 184, 77, 0.15)', borderRadius: '12px', color: '#ffb84d' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 21h18"></path><path d="M9 8h1"></path><path d="M9 12h1"></path><path d="M9 16h1"></path><path d="M14 8h1"></path><path d="M14 12h1"></path><path d="M14 16h1"></path><path d="M5 21V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16"></path></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Organizations</h3>
                        <div className="admin-stat-value" style={{ color: '#ffb84d' }}>{stats.totalOrganizations}</div>
                    </div>
                </div>

                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'rgba(245, 158, 11, 0.15)', borderRadius: '12px', color: '#f59e0b' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="3" width="7" height="7"></rect><rect x="14" y="3" width="7" height="7"></rect><rect x="14" y="14" width="7" height="7"></rect><rect x="3" y="14" width="7" height="7"></rect></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Departments</h3>
                        <div className="admin-stat-value" style={{ color: '#f59e0b' }}>{stats.totalDepartments}</div>
                    </div>
                </div>

                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'rgba(16, 185, 129, 0.15)', borderRadius: '12px', color: '#10b981' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"></rect><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"></path></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Active Jobs</h3>
                        <div className="admin-stat-value" style={{ color: '#10b981' }}>{stats.activeJobs}</div>
                    </div>
                </div>

                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'rgba(59, 130, 246, 0.15)', borderRadius: '12px', color: '#3b82f6' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Applications Today</h3>
                        <div className="admin-stat-value" style={{ color: '#3b82f6' }}>{stats.applicationsToday}</div>
                    </div>
                </div>

                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'rgba(139, 92, 246, 0.15)', borderRadius: '12px', color: '#8b5cf6' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Interviews Today</h3>
                        <div className="admin-stat-value" style={{ color: '#8b5cf6' }}>{stats.interviewsToday}</div>
                    </div>
                </div>

                <div className="admin-stat-card" style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', textAlign: 'left' }}>
                    <div style={{ padding: '1rem', backgroundColor: 'rgba(236, 72, 153, 0.15)', borderRadius: '12px', color: '#ec4899' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                    </div>
                    <div>
                        <h3 className="admin-stat-title">Hires This Month</h3>
                        <div className="admin-stat-value" style={{ color: '#ec4899' }}>{stats.hiresThisMonth}</div>
                    </div>
                </div>
            </div>

            {/* Recent Activity Feed */}
            <div className="admin-card">
                <h3 className="admin-card-title">Recent System Activity</h3>
                {stats.recentActivities && stats.recentActivities.length > 0 ? (
                    <ul className="admin-timeline-list">
                        {stats.recentActivities.map((action, index) => (
                            <li key={index} className="admin-timeline-item">
                                <div className="admin-timeline-icon">
                                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"></polyline></svg>
                                </div>
                                <span style={{ color: 'var(--text-h)', fontWeight: 500 }}>{action}</span>
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p style={{ opacity: 0.7 }}>No recent activity to display.</p>
                )}
            </div>
        </div>
    );
}