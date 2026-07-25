import { useState, useEffect, useCallback } from 'react';
import { jsonRequest } from '../api/apiClient';
import '../AdminModule.css';
import { adminApi } from '../api/adminApi';

export default function OrganizationManagement() {
    const [organizations, setOrganizations] = useState([]);
    const [pendingOrganizations, setPendingOrganizations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [reviewingId, setReviewingId] = useState(null);

    const [newOrg, setNewOrg] = useState({ organizationName: '', industry: '' });
    const [editingOrgId, setEditingOrgId] = useState(null);

    const fetchData = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            const [orgData, pendingData] = await Promise.all([
                adminApi.getOrganizations(),
                adminApi.getPendingOrganizations()
            ]);
            setOrganizations(Array.isArray(orgData) ? orgData : []);
            setPendingOrganizations(Array.isArray(pendingData) ? pendingData : []);
        } catch (requestError) {
            setError(requestError.message || 'Unable to load organization management data.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { 
        // eslint-disable-next-line react-hooks/set-state-in-effect
        fetchData(); 
    }, [fetchData]);

    const reviewOrganization = async (organization, approve) => {
        let reason = '';
        if (!approve) {
            reason = window.prompt(`Why is ${organization.organizationName} being rejected?`)?.trim() || '';
            if (!reason) return;
        }
        setReviewingId(organization.organizationId);
        try {
            if (approve) await adminApi.approveOrganization(organization.organizationId);
            else await adminApi.rejectOrganization(organization.organizationId, reason);
            await fetchData();
        } catch (requestError) {
            setError(requestError.message || 'The organization review could not be saved.');
        } finally {
            setReviewingId(null);
        }
    };

    const handleSaveOrg = (e) => {
        e.preventDefault();
        const method = editingOrgId ? 'PUT' : 'POST';
        const url = editingOrgId ? `/api/organization/${editingOrgId}` : '/api/organization';

        jsonRequest(url, method, newOrg)
            .then(({ data }) => {
                alert(data?.message || "Saved successfully");
                setNewOrg({ organizationName: '', industry: '' });
                setEditingOrgId(null);
                fetchData();
            }).catch(err => alert(err.message));
    };

    const handleDeleteOrg = (id) => {
        if (!window.confirm("Delete this organization?")) return;
        jsonRequest(`/api/organization/${id}`, 'DELETE')
            .then(({ data }) => {
                alert(data?.message || "Deleted successfully");
                fetchData();
            }).catch(err => alert(err.message));
    };

    if (loading) return <h2 className="admin-page-title">Loading...</h2>;

    return (
        <div className="admin-module-container">
            <h2 className="admin-page-title">Organizations & Departments</h2>
            {error && <div className="admin-card" role="alert" style={{ color: '#ff8a8a' }}>{error} <button className="admin-btn admin-btn-secondary" onClick={fetchData}>Retry</button></div>}

            <div className="admin-card">
                <h3 className="admin-card-title">Pending recruiter approvals ({pendingOrganizations.length})</h3>
                {pendingOrganizations.length === 0 ? <p>No organizations are waiting for review.</p> : pendingOrganizations.map(org => (
                    <div key={org.organizationId} style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', padding: '1rem 0', borderBottom: '1px solid var(--border)' }}>
                        <div><strong>{org.organizationName}</strong><div>{org.industry || 'Industry not provided'}</div><small>Submitted {org.submittedAt ? new Date(org.submittedAt).toLocaleString() : 'recently'}</small></div>
                        <div style={{ display: 'flex', gap: '.5rem', alignItems: 'center' }}>
                            <button className="admin-btn admin-btn-primary" disabled={reviewingId === org.organizationId} onClick={() => reviewOrganization(org, true)}>Approve</button>
                            <button className="admin-btn admin-btn-secondary" disabled={reviewingId === org.organizationId} onClick={() => reviewOrganization(org, false)}>Reject</button>
                        </div>
                    </div>
                ))}
            </div>

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
