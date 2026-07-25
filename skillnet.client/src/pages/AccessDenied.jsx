import { useNavigate } from 'react-router-dom';

const AccessDenied = () => {
    const navigate = useNavigate();

    return (
        <div style={{ maxWidth: '500px', margin: '80px auto', padding: '30px', textAlign: 'center', border: '1px solid #dc3545', borderRadius: '8px', backgroundColor: '#fff5f5' }}>
            <h1 style={{ color: '#dc3545', fontSize: '48px', margin: '0 0 20px 0' }}>403</h1>
            <h2 style={{ color: '#333' }}>Access Denied</h2>
            <p style={{ color: '#666', marginBottom: '25px' }}>You do not have the required permissions to view this dashboard page.</p>
            <button onClick={() => navigate('/login')} style={{ padding: '10px 20px', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                Go to Login
            </button>
        </div>
    );
};

export default AccessDenied;
