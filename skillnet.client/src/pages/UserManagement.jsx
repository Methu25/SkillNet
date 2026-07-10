import React, { useState, useEffect } from 'react';

export default function UserManagement() {
    const [users, setUsers] = useState([]);
    const [roles, setRoles] = useState([]);
    const [loading, setLoading] = useState(true);

    // Fetch users and roles from your C# APIs
    useEffect(() => {
        Promise.all([
            fetch('/api/user').then(res => res.json()),
            fetch('/api/userrole').then(res => res.json())
        ])
            .then(([userData, roleData]) => {
                // Ensure we always have an array even if the database is empty
                setUsers(Array.isArray(userData) ? userData : []);
                setRoles(Array.isArray(roleData) ? roleData : []);
                setLoading(false);
            })
            .catch(err => {
                console.error("Error fetching user data:", err);
                setLoading(false);
            });
    }, []);

    // Helper function to match the RoleId to the actual Role Name
    const getRoleName = (roleId) => {
        const role = roles.find(r => r.roleId === roleId || r.RoleId === roleId);
        return role ? (role.roleName || role.RoleName) : 'Unknown Role';
    };

    if (loading) {
        return <h2>Loading User Data...</h2>;
    }

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h2 style={{ margin: 0 }}>System Users</h2>
                <button style={{ background: '#3b82f6', color: 'white', border: 'none', padding: '10px 16px', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold' }}>
                    + Add New User
                </button>
            </div>

            <div style={{ background: '#fff', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)', overflow: 'hidden' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                    <thead style={{ background: '#f8fafc', borderBottom: '2px solid #e2e8f0' }}>
                        <tr>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Username</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Email</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Assigned Role</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Date Added</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.length === 0 ? (
                            <tr>
                                <td colSpan="5" style={{ padding: '20px', textAlign: 'center', color: '#94a3b8' }}>
                                    No users found in the database.
                                </td>
                            </tr>
                        ) : (
                            users.map((user) => (
                                <tr key={user.userId || user.UserId} style={{ borderBottom: '1px solid #e2e8f0' }}>
                                    <td style={{ padding: '12px 16px', fontWeight: 'bold' }}>{user.username || user.Username}</td>
                                    <td style={{ padding: '12px 16px', color: '#64748b' }}>{user.email || user.Email}</td>
                                    <td style={{ padding: '12px 16px' }}>
                                        <span style={{ background: '#e0e7ff', color: '#4338ca', padding: '4px 8px', borderRadius: '9999px', fontSize: '12px', fontWeight: 'bold' }}>
                                            {getRoleName(user.roleId || user.RoleId)}
                                        </span>
                                    </td>
                                    <td style={{ padding: '12px 16px', color: '#64748b' }}>
                                        {new Date(user.createdAt || user.CreatedAt).toLocaleDateString()}
                                    </td>
                                    <td style={{ padding: '12px 16px' }}>
                                        <button style={{ marginRight: '8px', padding: '4px 8px', background: '#f59e0b', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>Edit</button>
                                        <button style={{ padding: '4px 8px', background: '#ef4444', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>Delete</button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}