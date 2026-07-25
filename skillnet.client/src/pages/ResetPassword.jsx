import { useState } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';

const ResetPassword = () => {
    const navigate = useNavigate();
    const location = useLocation();

    const [email, setEmail] = useState(() => new URLSearchParams(location.search).get('email') || '');
    const [token, setToken] = useState(() => new URLSearchParams(location.search).get('token') || '');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [loading, setLoading] = useState(false);

    const checkPasswordPolicy = (pwd) => {
        return {
            length: pwd.length >= 8,
            upper: /[A-Z]/.test(pwd),
            lower: /[a-z]/.test(pwd),
            number: /[0-9]/.test(pwd),
            special: /[^a-zA-Z0-9]/.test(pwd),
        };
    };

    const pwdChecks = checkPasswordPolicy(newPassword);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        if (newPassword !== confirmPassword) {
            setError('Passwords do not match.');
            return;
        }

        const isValid = Object.values(pwdChecks).every(Boolean);
        if (!isValid) {
            setError('Password does not satisfy the security policy.');
            return;
        }

        setLoading(true);
        try {
            const response = await fetch('/api/auth/reset-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email, token, newPassword })
            });

            const data = await response.json();
            if (response.ok) {
                setSuccess('Password reset successful! Redirecting to login...');
                setTimeout(() => {
                    navigate('/login');
                }, 2000);
            } else {
                setError(data.message || 'Reset failed.');
            }
        } catch {
            setError('An error occurred. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ maxWidth: '400px', margin: '40px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
            <h2>Reset Password</h2>
            {error && <div style={{ color: 'red', marginBottom: '15px', padding: '8px', backgroundColor: '#ffe6e6', borderRadius: '4px' }}>{error}</div>}
            {success && <div style={{ color: 'green', marginBottom: '15px', padding: '8px', backgroundColor: '#e6ffe6', borderRadius: '4px' }}>{success}</div>}

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

                <div style={{ marginBottom: '15px' }}>
                    <label style={{ display: 'block', marginBottom: '5px' }}>Reset Token</label>
                    <input 
                        type="text" 
                        value={token} 
                        onChange={(e) => setToken(e.target.value)} 
                        required 
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                </div>

                <div style={{ marginBottom: '15px' }}>
                    <label style={{ display: 'block', marginBottom: '5px' }}>New Password</label>
                    <input 
                        type="password" 
                        value={newPassword} 
                        onChange={(e) => setNewPassword(e.target.value)} 
                        required 
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                    
                    <div style={{ fontSize: '12px', marginTop: '5px', padding: '8px', backgroundColor: '#f9f9f9', borderRadius: '4px' }}>
                        <strong>Password Policy Checklist:</strong>
                        <ul style={{ margin: '5px 0 0 0', paddingLeft: '20px' }}>
                            <li style={{ color: pwdChecks.length ? 'green' : 'red' }}>At least 8 characters</li>
                            <li style={{ color: pwdChecks.upper ? 'green' : 'red' }}>At least 1 uppercase letter</li>
                            <li style={{ color: pwdChecks.lower ? 'green' : 'red' }}>At least 1 lowercase letter</li>
                            <li style={{ color: pwdChecks.number ? 'green' : 'red' }}>At least 1 number</li>
                            <li style={{ color: pwdChecks.special ? 'green' : 'red' }}>At least 1 special character</li>
                        </ul>
                    </div>
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={{ display: 'block', marginBottom: '5px' }}>Confirm New Password</label>
                    <input 
                        type="password" 
                        value={confirmPassword} 
                        onChange={(e) => setConfirmPassword(e.target.value)} 
                        required 
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                </div>

                <button type="submit" disabled={loading} style={{ width: '100%', padding: '10px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                    {loading ? 'Resetting...' : 'Reset Password'}
                </button>
            </form>
            <div style={{ marginTop: '15px', textAlign: 'center' }}>
                <Link to="/login">Back to Login</Link>
            </div>
        </div>
    );
};

export default ResetPassword;
