import React, { useState, useEffect } from 'react';

export default function OrganizationManagement() {
    const [organizations, setOrganizations] = useState([]);
    const [departments, setDepartments] = useState([]);
    const [loading, setLoading] = useState(true);

    // Fetch both Organizations and Departments simultaneously
    useEffect(() => {
        Promise.all([
            fetch('/api/organization').then(res => res.json()),
            fetch('/api/department').then(res => res.json())
        ])
            .then(([orgData, deptData]) => {
                setOrganizations(Array.isArray(orgData) ? orgData : []);
                setDepartments(Array.isArray(deptData) ? deptData : []);
                setLoading(false);
            })
            .catch(err => {
                console.error("Error fetching org data:", err);
                setLoading(false);
            });
    }, []);

    if (loading) {
        return <h2>Loading Organizations & Departments...</h2>;
    }

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h2 style={{ margin: 0 }}>Organizations & Departments</h2>
                <div>
                    <button style={{ background: '#10b981', color: 'white', border: 'none', padding: '10px 16px', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold', marginRight: '10px' }}>
                        + Add Organization
                    </button>
                    <button style={{ background: '#3b82f6', color: 'white', border: 'none', padding: '10px 16px', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold' }}>
                        + Add Department
                    </button>
                </div>
            </div>

            {organizations.length === 0 ? (
                <div style={{ background: '#fff', padding: '20px', borderRadius: '8px', textAlign: 'center', color: '#64748b' }}>
                    No organizations found.
                </div>
            ) : (
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '20px' }}>
                    {organizations.map(org => {
                        // Filter departments that belong only to this specific organization
                        const orgId = org.organizationId || org.OrganizationId;
                        const orgDepts = departments.filter(d => (d.organizationId || d.OrganizationId) === orgId);

                        return (
                            <div key={orgId} style={{ background: '#fff', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)', overflow: 'hidden' }}>
                                <div style={{ background: '#f8fafc', padding: '16px', borderBottom: '1px solid #e2e8f0' }}>
                                    <h3 style={{ margin: 0, color: '#1e293b' }}>{org.organizationName || org.OrganizationName}</h3>
                                    <span style={{ fontSize: '12px', color: '#64748b' }}>{org.industry || org.Industry || 'Industry not specified'}</span>
                                </div>

                                <div style={{ padding: '16px' }}>
                                    <h4 style={{ margin: '0 0 10px 0', color: '#475569', fontSize: '14px' }}>Mapped Departments:</h4>
                                    {orgDepts.length === 0 ? (
                                        <p style={{ margin: 0, fontSize: '13px', color: '#94a3b8' }}>No departments mapped yet.</p>
                                    ) : (
                                        <ul style={{ margin: 0, paddingLeft: '20px', fontSize: '13px', color: '#334155' }}>
                                            {orgDepts.map(dept => (
                                                <li key={dept.departmentId || dept.DepartmentId} style={{ marginBottom: '6px' }}>
                                                    {dept.departmentName || dept.DepartmentName}
                                                </li>
                                            ))}
                                        </ul>
                                    )}
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
}