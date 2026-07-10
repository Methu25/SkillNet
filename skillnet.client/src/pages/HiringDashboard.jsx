import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';

const HiringDashboard = () => {
    const { user, logout } = useAuth();
    const [testResponse, setTestResponse] = useState('');

    const testEndpoint = async (url) => {
        setTestResponse('Calling API...');
        try {
            const token = localStorage.getItem('token');
            const response = await fetch(url, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            const data = await response.json();
            setTestResponse(`[Status: ${response.status}] ${JSON.stringify(data)}`);
        } catch (err) {
            setTestResponse(`Error: ${err.message}`);
        }
    };

    return (
        <div style={{ maxWidth: '600px', margin: '40px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
            <h1 style={{ color: '#28a745' }}>Hiring Manager Dashboard 📋</h1>
            <p>Welcome, Hiring Manager!</p>
            
            <div style={{ backgroundColor: '#f8f9fa', padding: '15px', borderRadius: '4px', marginBottom: '20px' }}>
                <h3>Profile Information</h3>
                <p><strong>Email:</strong> {user?.email}</p>
                <p><strong>Name:</strong> {user?.firstName} {user?.lastName}</p>
                <p><strong>Phone:</strong> {user?.phone || 'N/A'}</p>
                <p><strong>Roles:</strong> {user?.roles?.join(', ')}</p>
                <p><strong>Status:</strong> {user?.status}</p>
            </div>

            <div style={{ border: '1px solid #ddd', padding: '15px', borderRadius: '4px', marginBottom: '20px' }}>
                <h3>Backend RBAC Testing Console</h3>
                <div style={{ display: 'flex', gap: '10px', marginBottom: '10px' }}>
                    <button onClick={() => testEndpoint('/api/TestSecure/all-users')} style={{ padding: '8px 12px' }}>Test All Users</button>
                    <button onClick={() => testEndpoint('/api/TestSecure/admin-only')} style={{ padding: '8px 12px' }}>Test Admin Only</button>
                    <button onClick={() => testEndpoint('/api/TestSecure/candidate-only')} style={{ padding: '8px 12px' }}>Test Candidate Only</button>
                </div>
                {testResponse && (
                    <pre style={{ backgroundColor: '#333', color: '#fff', padding: '10px', borderRadius: '4px', overflowX: 'auto', fontSize: '12px' }}>
                        {testResponse}
                    </pre>
                )}
            </div>

            <button onClick={logout} style={{ padding: '10px 15px', backgroundColor: '#6c757d', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                Logout
            </button>
        </div>
    );
};

export default HiringDashboard;
