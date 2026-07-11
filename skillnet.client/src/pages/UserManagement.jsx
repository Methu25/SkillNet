import { useState, useEffect } from 'react';

export default function UserManagement() {
    const [users, setUsers] = useState([]);
    const [roles, setRoles] = useState([]);
    const [organizations, setOrganizations] = useState([]);
    const [departments, setDepartments] = useState([]);
    const [newUser, setNewUser] = useState({ username: '', email: '', passwordHash: '', roleId: 1, isActive: true, organizationId: '', departmentId: '' });
    const [editingId, setEditingId] = useState(null);

    const fetchUsers = () => {
        fetch('/api/user', { cache: 'no-store' })
            .then(res => res.json())
            .then(data => setUsers(data))
            .catch(() => console.error("Failed to fetch users."));
    };

    const fetchRoles = () => {
        fetch('/api/userrole', { cache: 'no-store' })
            .then(res => res.json())
            .then(data => setRoles(data))
            .catch(() => console.error("Failed to fetch roles."));
    };

    const fetchOrganizations = () => {
        fetch('/api/organization', { cache: 'no-store' })
            .then(res => res.json())
            .then(data => setOrganizations(data))
            .catch(() => console.error("Failed to fetch organizations."));
    };

    const fetchDepartments = () => {
        fetch('/api/department', { cache: 'no-store' })
            .then(res => res.json())
            .then(data => setDepartments(data))
            .catch(() => console.error("Failed to fetch departments."));
    };

    useEffect(() => { 
        fetchUsers(); 
        fetchRoles();
        fetchOrganizations();
        fetchDepartments();
    }, []);

    const handleSaveUser = (e) => {
        e.preventDefault();
        const method = editingId ? 'PUT' : 'POST';
        const url = editingId ? `/api/user/${editingId}` : '/api/user';

        const payload = {
            ...newUser,
            organizationId: newUser.organizationId ? parseInt(newUser.organizationId) : null,
            departmentId: newUser.departmentId ? parseInt(newUser.departmentId) : null,
        };

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
            .then(res => res.json())
            .then(data => {
                alert(data.message);
                setNewUser({ username: '', email: '', passwordHash: '', roleId: roles.length > 0 ? roles[0].roleId : 1, isActive: true, organizationId: '', departmentId: '' });
                setEditingId(null);
                fetchUsers();
            });
    };

    const handleToggleStatus = (id) => {
        fetch(`/api/user/${id}/toggle-status`, { method: 'PUT' })
            .then(res => res.json())
            .then(data => {
                alert(data.message);
                fetchUsers();
            });
    };

    const handleResetPassword = (id) => {
        const newPassword = prompt("Enter new password:");
        if (!newPassword) return;

        fetch(`/api/user/${id}/reset-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ newPassword })
        })
            .then(res => res.json())
            .then(data => {
                alert(data.message);
            });
    };

    const handleDelete = (id) => {
        if (!window.confirm("Are you sure you want to completely delete this user? This cannot be undone. Consider disabling them instead.")) return;
        fetch(`/api/user/${id}`, { method: 'DELETE' })
            .then(res => res.json())
            .then(data => {
                alert(data.message);
                fetchUsers();
            });
    };

    const handleEdit = (user) => {
        setEditingId(user.userId);
        setNewUser({ 
            username: user.username, 
            email: user.email, 
            passwordHash: '', 
            roleId: user.roleId, 
            isActive: user.isActive,
            organizationId: user.organizationId || '',
            departmentId: user.departmentId || ''
        });
    };

    return (
        <div style={{ maxWidth: '800px' }}>
            <h2 style={{ color: '#000' }}>User Management</h2>

            <div style={{ background: '#fff', padding: '20px', borderRadius: '8px', marginBottom: '20px', color: '#333' }}>
                <h3>{editingId ? 'Edit User' : '+ Add New User'}</h3>
                <form onSubmit={handleSaveUser} style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                    <input type="text" placeholder="Username" required value={newUser.username} onChange={e => setNewUser({ ...newUser, username: e.target.value })} style={{ padding: '8px' }} />
                    <input type="email" placeholder="Email" required value={newUser.email} onChange={e => setNewUser({ ...newUser, email: e.target.value })} style={{ padding: '8px' }} />
                    {!editingId && <input type="password" placeholder="Password" required value={newUser.passwordHash} onChange={e => setNewUser({ ...newUser, passwordHash: e.target.value })} style={{ padding: '8px' }} />}
                    
                    <select required value={newUser.roleId} onChange={e => setNewUser({ ...newUser, roleId: parseInt(e.target.value) })} style={{ padding: '8px' }}>
                        <option value="" disabled>Select Role</option>
                        {roles.map(r => (
                            <option key={r.roleId} value={r.roleId}>{r.roleName}</option>
                        ))}
                    </select>

                    <select value={newUser.organizationId} onChange={e => setNewUser({ ...newUser, organizationId: e.target.value })} style={{ padding: '8px' }}>
                        <option value="">No Organization</option>
                        {organizations.map(o => (
                            <option key={o.organizationId} value={o.organizationId}>{o.organizationName}</option>
                        ))}
                    </select>

                    <select value={newUser.departmentId} onChange={e => setNewUser({ ...newUser, departmentId: e.target.value })} style={{ padding: '8px' }}>
                        <option value="">No Department</option>
                        {departments.filter(d => !newUser.organizationId || d.organizationId === parseInt(newUser.organizationId)).map(d => (
                            <option key={d.departmentId} value={d.departmentId}>{d.departmentName}</option>
                        ))}
                    </select>

                    <button type="submit" style={{ background: '#8b5cf6', color: 'white', padding: '10px', border: 'none' }}>
                        {editingId ? 'Update User' : 'Save User'}
                    </button>
                    {editingId && <button type="button" onClick={() => { setEditingId(null); setNewUser({ username: '', email: '', passwordHash: '', roleId: roles.length > 0 ? roles[0].roleId : 1, isActive: true, organizationId: '', departmentId: '' }) }} style={{ padding: '10px' }}>Cancel</button>}
                </form>
            </div>

            <div style={{ background: '#fff', padding: '20px', borderRadius: '8px', color: '#333' }}>
                <h3>Current Users</h3>
                <ul style={{ listStyleType: 'none', padding: 0 }}>
                    {users.map(user => (
                        <li key={user.userId} style={{ padding: '10px', borderBottom: '1px solid #eee', display: 'flex', justifyContent: 'space-between' }}>
                            <span>
                                <strong>{user.username}</strong> ({user.email}) - Role: {roles.find(r => r.roleId === user.roleId)?.roleName || user.roleId}
                                <span style={{ marginLeft: '10px', padding: '2px 6px', borderRadius: '4px', fontSize: '12px', background: user.isActive ? '#dcfce7' : '#fee2e2', color: user.isActive ? '#166534' : '#991b1b' }}>
                                    {user.isActive ? 'Active' : 'Disabled'}
                                </span>
                                <div style={{ fontSize: '0.85em', color: '#666', marginTop: '4px' }}>
                                    {user.organizationId && <span>Org: {organizations.find(o => o.organizationId === user.organizationId)?.organizationName || user.organizationId}</span>}
                                    {user.departmentId && <span style={{ marginLeft: '10px' }}>Dept: {departments.find(d => d.departmentId === user.departmentId)?.departmentName || user.departmentId}</span>}
                                </div>
                            </span>
                            <div>
                                <button onClick={() => handleEdit(user)} style={{ marginRight: '10px', background: '#eab308', border: 'none', padding: '5px 10px', color: 'white', cursor: 'pointer' }}>Edit</button>
                                <button onClick={() => handleResetPassword(user.userId)} style={{ marginRight: '10px', background: '#3b82f6', border: 'none', padding: '5px 10px', color: 'white', cursor: 'pointer' }}>Reset Password</button>
                                <button onClick={() => handleToggleStatus(user.userId)} style={{ marginRight: '10px', background: user.isActive ? '#f97316' : '#22c55e', border: 'none', padding: '5px 10px', color: 'white', cursor: 'pointer' }}>
                                    {user.isActive ? 'Disable' : 'Activate'}
                                </button>
                                <button onClick={() => handleDelete(user.userId)} style={{ background: '#ef4444', border: 'none', padding: '5px 10px', color: 'white', cursor: 'pointer' }}>Delete</button>
                            </div>
                        </li>
                    ))}
                </ul>
            </div>
        </div>
    );
}