import React, { useState, useEffect } from 'react';
import { apiRequest, jsonRequest } from '../api/apiClient';
import '../AdminModule.css';

export default function OrganizationManagement() {
    const [organizations, setOrganizations] = useState([]);
    const [loading, setLoading] = useState(true);

    const [newOrg, setNewOrg] = useState({ organizationName: '', industry: '' });
    const [editingOrgId, setEditingOrgId] = useState(null);

    const fetchData = () => {
        apiRequest('/api/organization', { cache: 'no-store' })
            .then(res => {
                const orgData = res.data;
                setOrganizations(Array.isArray(orgData) ? orgData : []);
                setLoading(false);
            })
            .catch(err => {
                console.error(err);
                setLoading(false);
            });
    };

    useEffect(() => { fetchData(); }, []);

    const handleSaveOrg = (e) => {
        e.preventDefault();
        const method = editingOrgId ? 'PUT' : 'POST';
        const url = editingOrgId ? `/api/organization/${editingOrgId}` : '/api/organization';

        jsonRequest(url, method, newOrg)
            .then(res => {
                alert(res.data?.message || "Saved successfully");
                setNewOrg({ organizationName: '', industry: '' });
                setEditingOrgId(null);
                fetchData();
            }).catch(err => alert(err.message));
    };

    const handleDeleteOrg = (id) => {
        if (!window.confirm("Delete this organization?")) return;
        apiRequest(`/api/organization/${id}`, { method: 'DELETE' })
            .then(res => {
                alert(res.data?.message || "Deleted successfully");
                fetchData();
            }).catch(err => alert(err.message));
    };

    if (loading) return <h2 className="admin-page-title">Loading...</h2>;

    return (
        <div className="admin-module-container">
            <h2 className="admin-page-title">Organization Management</h2>

            <div style={{ display: 'flex', gap: '1.5rem', marginBottom: '2rem', flexWrap: 'wrap' }}>
                <div className="admin-card" style={{ flex: '1 1 300px', marginBottom: 0 }}>
                    <h3 className="admin-card-title">{editingOrgId ? 'Edit Organization' : '+ Add Organization'}</h3>
                    <form onSubmit={handleSaveOrg} className="admin-form">
                        <div className="admin-form-grid" style={{ gridTemplateColumns: '1fr', gap: '1rem', marginBottom: '1rem' }}>
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Organization Name</label>
                                <input className="admin-input" type="text" placeholder="e.g. Acme Corp" required value={newOrg.organizationName} onChange={e => setNewOrg({ ...newOrg, organizationName: e.target.value })} style={{ width: '100%' }} />
                            </div>
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Industry</label>
                                <input className="admin-input" type="text" placeholder="e.g. Technology" value={newOrg.industry} onChange={e => setNewOrg({ ...newOrg, industry: e.target.value })} style={{ width: '100%' }} />
                            </div>
                        </div>
                        <div style={{ display: 'flex', gap: '1rem' }}>
                            <button type="submit" className="admin-btn admin-btn-primary">{editingOrgId ? 'Update Org' : 'Save Org'}</button>
                            {editingOrgId && <button type="button" className="admin-btn admin-btn-secondary" onClick={() => { setEditingOrgId(null); setNewOrg({ organizationName: '', industry: '' }); }}>Cancel</button>}
                        </div>
                    </form>
                </div>
            </div>

            <div className="admin-card" style={{ overflowX: 'auto', padding: 0 }}>
                <div style={{ padding: '1.5rem 1.5rem 0' }}>
                    <h3 className="admin-card-title">Current Organizations</h3>
                </div>
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>Organization & Industry</th>
                            <th style={{ width: '120px' }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {organizations.map(org => {
                            const orgId = org.OrganizationId || org.organizationId;
                            
                            return (
                                <tr key={orgId}>
                                    <td>
                                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                                            <div className="admin-avatar" style={{ borderRadius: '8px' }}>
                                                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 21h18"></path><path d="M9 8h1"></path><path d="M9 12h1"></path><path d="M9 16h1"></path><path d="M14 8h1"></path><path d="M14 12h1"></path><path d="M14 16h1"></path><path d="M5 21V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16"></path></svg>
                                            </div>
                                            <div>
                                                <div style={{ fontWeight: 600 }}>{org.OrganizationName || org.organizationName}</div>
                                                <div style={{ fontSize: '0.85em', opacity: 0.7 }}>{org.Industry || org.industry || 'No industry'}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td>
                                        <div style={{ display: 'flex', gap: '0.25rem' }}>
                                            <button onClick={() => { setEditingOrgId(orgId); setNewOrg({ organizationName: org.OrganizationName || org.organizationName, industry: org.Industry || org.industry }); }} className="admin-btn admin-btn-ghost">Edit</button>
                                            <button onClick={() => handleDeleteOrg(orgId)} className="admin-btn admin-btn-ghost admin-btn-ghost-danger">Delete</button>
                                        </div>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
                {organizations.length === 0 && <div style={{ padding: '2rem', textAlign: 'center', opacity: 0.7 }}>No organizations found.</div>}
            </div>
        </div>
    );
}