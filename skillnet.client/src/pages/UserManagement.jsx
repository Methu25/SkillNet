import { useState, useEffect } from 'react';
import '../AdminModule.css';

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
        <div className="admin-module-container">
            <h2 className="admin-page-title">User Management</h2>

            <div className="admin-card">
                <h3 className="admin-card-title">{editingId ? 'Edit User' : '+ Add New User'}</h3>
                <form onSubmit={handleSaveUser} className="admin-form">
                    <div className="admin-form-grid">
                        <div>
                            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Username</label>
                            <input className="admin-input" type="text" placeholder="e.g. jdoe" required value={newUser.username} onChange={e => setNewUser({ ...newUser, username: e.target.value })} style={{ width: '100%' }} />
                        </div>
                        <div>
                            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Email Address</label>
                            <input className="admin-input" type="email" placeholder="john@example.com" required value={newUser.email} onChange={e => setNewUser({ ...newUser, email: e.target.value })} style={{ width: '100%' }} />
                        </div>
                        {!editingId && (
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Password</label>
                                <input className="admin-input" type="password" placeholder="Secure password" required value={newUser.passwordHash} onChange={e => setNewUser({ ...newUser, passwordHash: e.target.value })} style={{ width: '100%' }} />
                            </div>
                        )}
                        <div>
                            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Role</label>
                            <select className="admin-select" required value={newUser.roleId} onChange={e => setNewUser({ ...newUser, roleId: parseInt(e.target.value) })} style={{ width: '100%' }}>
                                <option value="" disabled>Select Role</option>
                                {roles.map(r => (
                                    <option key={r.roleId} value={r.roleId}>{r.roleName}</option>
                                ))}
                            </select>
                        </div>
                        <div>
                            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Organization</label>
                            <select className="admin-select" value={newUser.organizationId} onChange={e => setNewUser({ ...newUser, organizationId: e.target.value })} style={{ width: '100%' }}>
                                <option value="">No Organization</option>
                                {organizations.map(o => (
                                    <option key={o.organizationId} value={o.organizationId}>{o.organizationName}</option>
                                ))}
                            </select>
                        </div>
                        <div>
                            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Department</label>
                            <select className="admin-select" value={newUser.departmentId} onChange={e => setNewUser({ ...newUser, departmentId: e.target.value })} style={{ width: '100%' }}>
                                <option value="">No Department</option>
                                {departments.filter(d => !newUser.organizationId || d.organizationId === parseInt(newUser.organizationId)).map(d => (
                                    <option key={d.departmentId} value={d.departmentId}>{d.departmentName}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
                        <button type="submit" className="admin-btn admin-btn-primary">
                            {editingId ? 'Update User' : 'Save User'}
                        </button>
                        {editingId && (
                            <button type="button" className="admin-btn admin-btn-secondary" onClick={() => { setEditingId(null); setNewUser({ username: '', email: '', passwordHash: '', roleId: roles.length > 0 ? roles[0].roleId : 1, isActive: true, organizationId: '', departmentId: '' }) }}>
                                Cancel
                            </button>
                        )}
                    </div>
                </form>
            </div>

            <div className="admin-card" style={{ overflowX: 'auto', padding: 0 }}>
                <div style={{ padding: '1.5rem 1.5rem 0' }}>
                    <h3 className="admin-card-title">Current Users</h3>
                </div>
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>User</th>
                            <th>Contact</th>
                            <th>Role & Organization</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(user => (
                            <tr key={user.userId}>
                                <td>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                                        <div className="admin-avatar">
                                            {user.username.charAt(0).toUpperCase()}
                                        </div>
                                        <span style={{ fontWeight: 600 }}>{user.username}</span>
                                    </div>
                                </td>
                                <td>{user.email}</td>
                                <td>
                                    <div style={{ fontWeight: 500 }}>{roles.find(r => r.roleId === user.roleId)?.roleName || user.roleId}</div>
                                    <div style={{ fontSize: '0.85em', opacity: 0.7, marginTop: '4px' }}>
                                        {user.organizationId ? (organizations.find(o => o.organizationId === user.organizationId)?.organizationName || 'Org') : ''}
                                        {user.departmentId ? ` - ${departments.find(d => d.departmentId === user.departmentId)?.departmentName || 'Dept'}` : ''}
                                    </div>
                                </td>
                                <td>
                                    <span className={user.isActive ? 'admin-badge-active' : 'admin-badge-inactive'} style={{ marginLeft: 0 }}>
                                        {user.isActive ? 'Active' : 'Disabled'}
                                    </span>
                                </td>
                                <td>
                                    <div style={{ display: 'flex', gap: '0.25rem' }}>
                                        <button onClick={() => handleEdit(user)} className="admin-btn admin-btn-ghost">Edit</button>
                                        <button onClick={() => handleResetPassword(user.userId)} className="admin-btn admin-btn-ghost">Reset</button>
                                        <button onClick={() => handleToggleStatus(user.userId)} className="admin-btn admin-btn-ghost">
                                            {user.isActive ? 'Disable' : 'Activate'}
                                        </button>
                                        <button onClick={() => handleDelete(user.userId)} className="admin-btn admin-btn-ghost admin-btn-ghost-danger">Delete</button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                {users.length === 0 && <div style={{ padding: '2rem', textAlign: 'center', opacity: 0.7 }}>No users found.</div>}
            </div>
        </div>
    );
}