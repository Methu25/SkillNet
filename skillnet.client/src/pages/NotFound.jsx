import React from 'react';
import { useNavigate } from 'react-router-dom';

const NotFound = () => {
    const navigate = useNavigate();

    return (
        <div style={{ maxWidth: '500px', margin: '80px auto', padding: '30px', textAlign: 'center', border: '1px solid #ccc', borderRadius: '8px' }}>
            <h1 style={{ color: '#6c757d', fontSize: '48px', margin: '0 0 20px 0' }}>404</h1>
            <h2>Page Not Found</h2>
            <p style={{ color: '#666', marginBottom: '25px' }}>The page you are looking for does not exist.</p>
            <button onClick={() => navigate('/login')} style={{ padding: '10px 20px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                Go to Login
            </button>
        </div>
    );
};

export default NotFound;
