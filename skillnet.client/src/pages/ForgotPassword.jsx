import { useState } from 'react';
import { Link } from 'react-router-dom';

const ForgotPassword = () => {
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState('');
    const [debugToken, setDebugToken] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        setDebugToken('');
        setError('');
        setLoading(true);

        try {
            const response = await fetch('/api/auth/forgot-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email })
            });

            const data = await response.json();
            if (response.ok) {
                setMessage(data.message);
                if (data.debugToken) {
                    setDebugToken(data.debugToken);
                }
            } else {
                setError(data.message || 'Request failed.');
            }
        } catch {
            setError('An error occurred. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ maxWidth: '400px', margin: '50px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
            <h2>Forgot Password</h2>
            <p style={{ color: '#666', fontSize: '14px' }}>Enter your registered email address and we will generate a password reset token.</p>
            
            {error && <div style={{ color: 'red', marginBottom: '15px', padding: '8px', backgroundColor: '#ffe6e6', borderRadius: '4px' }}>{error}</div>}
            {message && <div style={{ color: 'green', marginBottom: '15px', padding: '8px', backgroundColor: '#e6ffe6', borderRadius: '4px' }}>{message}</div>}
            
            {/* QA/Testing Helper banner showing the token */}
            {debugToken && (
                <div style={{ border: '1px dashed orange', backgroundColor: '#fff9e6', padding: '10px', borderRadius: '4px', marginBottom: '15px' }}>
                    <strong style={{ color: 'orange' }}>[Developer Debug Token]</strong>
                    <p style={{ margin: '5px 0', fontSize: '12px' }}>Copy this token to use on the reset page:</p>
                    <code style={{ wordBreak: 'break-all', fontWeight: 'bold', fontSize: '14px', backgroundColor: '#eee', padding: '2px 5px', borderRadius: '3px' }}>
                        {debugToken}
                    </code>
                    <div style={{ marginTop: '10px' }}>
                        <Link to={`/reset-password?email=${encodeURIComponent(email)}&token=${debugToken}`} style={{ color: '#007bff', fontWeight: 'bold' }}>
                            Go to Reset Page directly
                        </Link>
                    </div>
                </div>
            )}

            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: '15px' }}>
                    <label style={{ display: 'block', marginBottom: '5px' }}>Email Address</label>
                    <input 
                        type="email" 
                        value={email} 
                        onChange={(e) => setEmail(e.target.value)} 
                        required 
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                </div>
                <button type="submit" disabled={loading} style={{ width: '100%', padding: '10px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                    {loading ? 'Submitting...' : 'Send Reset Link'}
                </button>
            </form>
            <div style={{ marginTop: '15px', textAlign: 'center' }}>
                <Link to="/login">Back to Login</Link>
            </div>
        </div>
    );
};

export default ForgotPassword;
