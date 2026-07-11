import { useState, useEffect } from 'react';

export default function OrganizationManagement() {
    const [organizations, setOrganizations] = useState([]);
    const [departments, setDepartments] = useState([]);
    const [loading, setLoading] = useState(true);

    const [newOrg, setNewOrg] = useState({ organizationName: '', industry: '' });
    const [newDept, setNewDept] = useState({ organizationId: '', departmentName: '' });
    const [editingOrgId, setEditingOrgId] = useState(null);
    const [editingDeptId, setEditingDeptId] = useState(null);

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

    if (loading) return <h2>Loading...</h2>;

    return (
        <div>
            <h2 style={{ color: '#000' }}>Organizations & Departments</h2>

            <div style={{ display: 'flex', gap: '20px', marginBottom: '20px' }}>
                <div style={{ flex: 1, background: '#fff', padding: '20px', borderRadius: '8px', color: '#333' }}>
                    <h3>{editingOrgId ? 'Edit Organization' : '+ Add Organization'}</h3>
                    <form onSubmit={handleSaveOrg} style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                        <input type="text" placeholder="Name" required value={newOrg.organizationName} onChange={e => setNewOrg({ ...newOrg, organizationName: e.target.value })} style={{ padding: '8px' }} />
                        <input type="text" placeholder="Industry" value={newOrg.industry} onChange={e => setNewOrg({ ...newOrg, industry: e.target.value })} style={{ padding: '8px' }} />
                        <button type="submit" style={{ background: '#10b981', color: 'white', padding: '10px', border: 'none' }}>Save</button>
                    </form>
                </div>

                <div style={{ flex: 1, background: '#fff', padding: '20px', borderRadius: '8px', color: '#333' }}>
                    <h3>{editingDeptId ? 'Edit Department' : '+ Add Department'}</h3>
                    <form onSubmit={handleSaveDept} style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                        <select required value={newDept.organizationId} onChange={e => setNewDept({ ...newDept, organizationId: e.target.value })} style={{ padding: '8px' }}>
                            <option value="">Select Org...</option>
                            {organizations.map(org => <option key={org.OrganizationId || org.organizationId} value={org.OrganizationId || org.organizationId}>{org.OrganizationName || org.organizationName}</option>)}
                        </select>
                        <input type="text" placeholder="Dept Name" required value={newDept.departmentName} onChange={e => setNewDept({ ...newDept, departmentName: e.target.value })} style={{ padding: '8px' }} />
                        <button type="submit" style={{ background: '#3b82f6', color: 'white', padding: '10px', border: 'none' }}>Save</button>
                    </form>
                </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(350px, 1fr))', gap: '20px' }}>
                {organizations.map(org => {
                    const orgId = org.OrganizationId || org.organizationId;
                    return (
                        <div key={orgId} style={{ background: '#fff', padding: '15px', borderRadius: '8px', border: '1px solid #ccc', color: '#333' }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                <h3>{org.OrganizationName || org.organizationName}</h3>
                                <div>
                                    <button onClick={() => { setEditingOrgId(orgId); setNewOrg({ organizationName: org.OrganizationName || org.organizationName, industry: org.Industry || org.industry }); }} style={{ background: '#eab308', border: 'none', padding: '5px', color: 'white', marginRight: '5px' }}>Edit</button>
                                    <button onClick={() => handleDeleteOrg(orgId)} style={{ background: '#ef4444', border: 'none', padding: '5px', color: 'white' }}>Del</button>
                                </div>
                            </div>

                            <hr />
                            <h4>Departments:</h4>
                            <ul style={{ paddingLeft: '20px' }}>
                                {departments.filter(d => (d.OrganizationId || d.organizationId) === orgId).map(dept => (
                                    <li key={dept.DepartmentId || dept.departmentId} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '5px' }}>
                                        <span>{dept.DepartmentName || dept.departmentName}</span>
                                        <div>
                                            <button onClick={() => { setEditingDeptId(dept.DepartmentId || dept.departmentId); setNewDept({ organizationId: orgId, departmentName: dept.DepartmentName || dept.departmentName }); }} style={{ background: '#eab308', border: 'none', padding: '2px 5px', color: 'white', marginRight: '5px', fontSize: '12px' }}>Edit</button>
                                            <button onClick={() => handleDeleteDept(dept.DepartmentId || dept.departmentId)} style={{ background: '#ef4444', border: 'none', padding: '2px 5px', color: 'white', fontSize: '12px' }}>Del</button>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}