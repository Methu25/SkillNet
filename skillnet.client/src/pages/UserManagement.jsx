import React, { useState, useEffect } from 'react';
import { jsonRequest } from '../api/apiClient';
import '../AdminModule.css';
import { adminApi } from '../api/adminApi';

export default function UserManagement() {
    const [users, setUsers] = useState([]);
    const [roles, setRoles] = useState([]);
    const [organizations, setOrganizations] = useState([]);
    const [newUser, setNewUser] = useState({ username: '', email: '', passwordHash: '', roleId: 1, isActive: true, organizationId: '' });
    const [editingId, setEditingId] = useState(null);

    const [error, setError] = useState('');

    const fetchUsers = () => {
        setError('');
        adminApi.getUsers()
            .then(data => setUsers(Array.isArray(data) ? data : []))
            .catch((err) => {
                console.error("Failed to fetch users:", err);
                setError(err.message || "Failed to fetch users. Please verify backend server and authentication.");
            });
    };

    const fetchRoles = () => {
        adminApi.getRoles()
            .then(data => setRoles(data))
            .catch(() => console.error("Failed to fetch roles."));
    };

    const fetchOrganizations = () => {
        adminApi.getOrganizations()
            .then(data => setOrganizations(Array.isArray(data) ? data : []))
            .catch(() => console.error("Failed to fetch organizations."));
    };

    useEffect(() => { 
        fetchUsers(); 
        fetchRoles();
        fetchOrganizations();
    }, []);

    const handleSaveUser = (e) => {
        e.preventDefault();
        const method = editingId ? 'PUT' : 'POST';
        const url = editingId ? `/api/user/${editingId}` : '/api/user';

        const payload = {
            ...newUser,
            organizationId: newUser.organizationId ? parseInt(newUser.organizationId) : null,
        };

        jsonRequest(url, method, payload)
            .then(({ data }) => data)
            .then(data => {
                alert(data?.message || "Saved successfully");
                setNewUser({ username: '', email: '', passwordHash: '', roleId: roles.length > 0 ? roles[0].roleId : 1, isActive: true, organizationId: '' });
                setEditingId(null);
                fetchUsers();
            })
            .catch(err => alert("Failed to save user: " + err.message));
    };

    const handleToggleStatus = (id) => {
        jsonRequest(`/api/user/${id}/toggle-status`, 'PUT')
            .then(({ data }) => data)
            .then(data => {
                alert(data?.message || "Status toggled");
                fetchUsers();
            })
            .catch(err => alert("Failed to toggle status: " + err.message));
    };

    const handleResetPassword = (id) => {
        const newPassword = prompt("Enter new password:");
        if (!newPassword) return;

        jsonRequest(`/api/user/${id}/reset-password`, 'POST', { newPassword })
            .then(({ data }) => data)
            .then(data => {
                alert(data?.message || "Password reset successful");
            })
            .catch(err => alert("Failed to reset password: " + err.message));
    };

    const handleDelete = (id) => {
        if (!window.confirm("Are you sure you want to completely delete this user? This cannot be undone. Consider disabling them instead.")) return;
        jsonRequest(`/api/user/${id}`, 'DELETE')
            .then(({ data }) => data)
            .then(data => {
                alert(data?.message || "User deleted");
                fetchUsers();
            })
            .catch(err => alert("Failed to delete user: " + err.message));
    };

    const handleEdit = (user) => {
        setEditingId(user.userId);
        setNewUser({ 
            username: user.username, 
            email: user.email, 
            passwordHash: '', 
            roleId: user.roleId, 
            isActive: user.isActive,
            organizationId: user.organizationId || ''
        });
    };

    return (
        <div className="admin-module-container">
            <h2 className="admin-page-title">User Management</h2>

            {editingId && (
                <div className="admin-card">
                    <h3 className="admin-card-title">Edit User</h3>
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
                        </div>

                        <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
                            <button type="submit" className="admin-btn admin-btn-primary">
                                {editingId ? 'Update User' : 'Save User'}
                            </button>
                            {editingId && (
                                <button type="button" className="admin-btn admin-btn-secondary" onClick={() => { setEditingId(null); setNewUser({ username: '', email: '', passwordHash: '', roleId: roles.length > 0 ? roles[0].roleId : 1, isActive: true, organizationId: '' }) }}>
                                    Cancel
                                </button>
                            )}
                        </div>
                    </form>
                </div>
            )}

            {error && (
                <div className="admin-card" style={{ background: '#fff0ef', border: '1px solid #f1cbc7', color: '#984139', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <span>{error}</span>
                    <button className="admin-btn admin-btn-secondary" onClick={fetchUsers}>Retry</button>
                </div>
            )}

            <div className="admin-card" style={{ overflowX: 'auto', padding: 0 }}>
                <div style={{ padding: '1.5rem 1.5rem 0' }}>
                    <h3 className="admin-card-title">Current Users</h3>
                </div>
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>User</th>
                            <th>Contact</th>
                            <th>Role</th>
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
                                            {(user.username || user.email || 'U').charAt(0).toUpperCase()}
                                        </div>
                                        <span style={{ fontWeight: 600 }}>{user.username}</span>
                                    </div>
                                </td>
                                <td>{user.email}</td>
                                <td>
                                    <div style={{ fontWeight: 500 }}>{user.roles || roles.find(r => r.roleId === user.roleId)?.roleName || 'No role'}</div>
                                    <div style={{ fontSize: '0.85em', opacity: 0.7, marginTop: '4px' }}>
                                        {user.organizationId ? (organizations.find(o => o.organizationId === user.organizationId)?.organizationName || 'Org') : ''}
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
