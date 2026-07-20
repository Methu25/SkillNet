import React, { useState, useEffect } from 'react';
import '../AdminModule.css';

export default function OrganizationManagement() {
    const [organizations, setOrganizations] = useState([]);
    const [departments, setDepartments] = useState([]);
    const [loading, setLoading] = useState(true);

    const [newOrg, setNewOrg] = useState({ organizationName: '', industry: '' });
    const [newDept, setNewDept] = useState({ organizationId: '', departmentName: '' });
    const [editingOrgId, setEditingOrgId] = useState(null);
    const [editingDeptId, setEditingDeptId] = useState(null);
    const [expandedOrgIds, setExpandedOrgIds] = useState([]);

    const toggleExpanded = (orgId) => {
        setExpandedOrgIds(prev => prev.includes(orgId) ? prev.filter(id => id !== orgId) : [...prev, orgId]);
    };

    const fetchData = () => {
        Promise.all([
            fetch('/api/organization', { cache: 'no-store' }).then(res => res.json()),
            fetch('/api/department', { cache: 'no-store' }).then(res => res.json())
        ]).then(([orgData, deptData]) => {
            setOrganizations(Array.isArray(orgData) ? orgData : []);
            setDepartments(Array.isArray(deptData) ? deptData : []);
            setLoading(false);
        });
    };

    useEffect(() => { fetchData(); }, []);

    const handleSaveOrg = (e) => {
        e.preventDefault();
        const method = editingOrgId ? 'PUT' : 'POST';
        const url = editingOrgId ? `/api/organization/${editingOrgId}` : '/api/organization';

        fetch(url, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(newOrg) })
            .then(async res => {
                const data = await res.json();
                if (!res.ok) throw new Error(data.message);
                alert(data.message);
                setNewOrg({ organizationName: '', industry: '' });
                setEditingOrgId(null);
                fetchData();
            }).catch(err => alert(err.message));
    };

    const handleDeleteOrg = (id) => {
        if (!window.confirm("Delete this organization?")) return;
        fetch(`/api/organization/${id}`, { method: 'DELETE' })
            .then(async res => {
                const data = await res.json();
                if (!res.ok) throw new Error(data.message);
                alert(data.message);
                fetchData();
            }).catch(err => alert(err.message));
    };

    const handleSaveDept = (e) => {
        e.preventDefault();
        const method = editingDeptId ? 'PUT' : 'POST';
        const url = editingDeptId ? `/api/department/${editingDeptId}` : '/api/department';

        fetch(url, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ organizationId: parseInt(newDept.organizationId), departmentName: newDept.departmentName }) })
            .then(async res => {
                const data = await res.json();
                if (!res.ok) throw new Error(data.message);
                alert(data.message);
                setNewDept({ organizationId: '', departmentName: '' });
                setEditingDeptId(null);
                fetchData();
            }).catch(err => alert(err.message));
    };

    const handleDeleteDept = (id) => {
        if (!window.confirm("Delete this department?")) return;
        fetch(`/api/department/${id}`, { method: 'DELETE' })
            .then(async res => {
                const data = await res.json();
                if (!res.ok) throw new Error(data.message);
                alert(data.message);
                fetchData();
            }).catch(err => alert(err.message));
    };

    if (loading) return <h2 className="admin-page-title">Loading...</h2>;

    return (
        <div className="admin-module-container">
            <h2 className="admin-page-title">Organizations & Departments</h2>

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

                <div className="admin-card" style={{ flex: '1 1 300px', marginBottom: 0 }}>
                    <h3 className="admin-card-title">{editingDeptId ? 'Edit Department' : '+ Add Department'}</h3>
                    <form onSubmit={handleSaveDept} className="admin-form">
                        <div className="admin-form-grid" style={{ gridTemplateColumns: '1fr', gap: '1rem', marginBottom: '1rem' }}>
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Select Organization</label>
                                <select className="admin-select" required value={newDept.organizationId} onChange={e => setNewDept({ ...newDept, organizationId: e.target.value })} style={{ width: '100%' }}>
                                    <option value="">Select Org...</option>
                                    {organizations.map(org => <option key={org.OrganizationId || org.organizationId} value={org.OrganizationId || org.organizationId}>{org.OrganizationName || org.organizationName}</option>)}
                                </select>
                            </div>
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Department Name</label>
                                <input className="admin-input" type="text" placeholder="e.g. Engineering" required value={newDept.departmentName} onChange={e => setNewDept({ ...newDept, departmentName: e.target.value })} style={{ width: '100%' }} />
                            </div>
                        </div>
                        <div style={{ display: 'flex', gap: '1rem' }}>
                            <button type="submit" className="admin-btn admin-btn-primary">{editingDeptId ? 'Update Dept' : 'Save Dept'}</button>
                            {editingDeptId && <button type="button" className="admin-btn admin-btn-secondary" onClick={() => { setEditingDeptId(null); setNewDept({ organizationId: '', departmentName: '' }); }}>Cancel</button>}
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
                            <th>Departments</th>
                            <th style={{ width: '120px' }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {organizations.map(org => {
                            const orgId = org.OrganizationId || org.organizationId;
                            const orgDepts = departments.filter(d => (d.OrganizationId || d.organizationId) === orgId);
                            const isExpanded = expandedOrgIds.includes(orgId);
                            
                            return (
                                <React.Fragment key={orgId}>
                                    <tr>
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
                                            <button onClick={() => toggleExpanded(orgId)} className="admin-btn-ghost" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', padding: '0.4rem 0.8rem', borderRadius: '999px', border: '1px solid var(--border)', background: 'var(--bg)', cursor: 'pointer' }}>
                                                {isExpanded ? '▼' : '▶'} {orgDepts.length} Department{orgDepts.length !== 1 ? 's' : ''}
                                            </button>
                                        </td>
                                        <td>
                                            <div style={{ display: 'flex', gap: '0.25rem' }}>
                                                <button onClick={() => { setEditingOrgId(orgId); setNewOrg({ organizationName: org.OrganizationName || org.organizationName, industry: org.Industry || org.industry }); }} className="admin-btn admin-btn-ghost">Edit</button>
                                                <button onClick={() => handleDeleteOrg(orgId)} className="admin-btn admin-btn-ghost admin-btn-ghost-danger">Delete</button>
                                            </div>
                                        </td>
                                    </tr>
                                    {isExpanded && (
                                        <tr style={{ background: 'var(--code-bg)' }}>
                                            <td colSpan="3" style={{ padding: '1rem 1.5rem', borderBottom: '1px solid var(--border)' }}>
                                                <div style={{ padding: '1rem', background: 'var(--bg)', borderRadius: '8px', border: '1px solid var(--border)' }}>
                                                    <h4 style={{ margin: '0 0 1rem 0', color: 'var(--text-h)' }}>Departments in {org.OrganizationName || org.organizationName}</h4>
                                                    {orgDepts.length === 0 ? (
                                                        <div style={{ opacity: 0.7, fontSize: '0.9rem' }}>No departments added yet.</div>
                                                    ) : (
                                                        <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                                                            {orgDepts.map(dept => (
                                                                <li key={dept.DepartmentId || dept.departmentId} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '0.75rem 1rem', borderBottom: '1px solid var(--border)' }}>
                                                                    <span style={{ fontWeight: 500 }}>{dept.DepartmentName || dept.departmentName}</span>
                                                                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                                                                        <button onClick={() => { setEditingDeptId(dept.DepartmentId || dept.departmentId); setNewDept({ organizationId: orgId, departmentName: dept.DepartmentName || dept.departmentName }); }} className="admin-btn admin-btn-ghost">Edit</button>
                                                                        <button onClick={() => handleDeleteDept(dept.DepartmentId || dept.departmentId)} className="admin-btn admin-btn-ghost admin-btn-ghost-danger">Delete</button>
                                                                    </div>
                                                                </li>
                                                            ))}
                                                        </ul>
                                                    )}
                                                </div>
                                            </td>
                                        </tr>
                                    )}
                                </React.Fragment>
                            );
                        })}
                    </tbody>
                </table>
                {organizations.length === 0 && <div style={{ padding: '2rem', textAlign: 'center', opacity: 0.7 }}>No organizations found.</div>}
            </div>
        </div>
    );
}