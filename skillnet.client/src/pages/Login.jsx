import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link, Navigate } from 'react-router-dom';

const Login = () => {
    const { login, user, loading: authLoading } = useAuth();
    const navigate = useNavigate();

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [submitting, setSubmitting] = useState(false);

    // Helper: resolve dashboard path from roles array
    const getDashboardPath = (roles = []) => {
        if (roles.includes('Admin'))         return '/admin-dashboard';
        if (roles.includes('Recruiter'))     return '/recruiter-dashboard';
        if (roles.includes('HiringManager')) return '/hiring-dashboard';
        if (roles.includes('Candidate'))     return '/candidate-dashboard';
        return '/access-denied';
    };

    // If user is already authenticated, skip the form and go straight to their dashboard
    if (!authLoading && user) {
        return <Navigate to={getDashboardPath(user.roles)} replace />;
    }

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSubmitting(true);

        try {
            const userData = await login(email, password);
            navigate(getDashboardPath(userData.roles));
        } catch (err) {
            setError(err.message || 'Login failed. Please check your credentials.');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div style={{ maxWidth: '400px', margin: '50px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
            <h2>Login to SkillNet</h2>
            {error && <div style={{ color: 'red', marginBottom: '15px', padding: '8px', backgroundColor: '#ffe6e6', borderRadius: '4px' }}>{error}</div>}
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
                    <label style={{ display: 'block', marginBottom: '5px' }}>Password</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                </div>
                <button type="submit" disabled={submitting} style={{ width: '100%', padding: '10px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                    {submitting ? 'Logging in...' : 'Login'}
                </button>
            </form>
            <div style={{ marginTop: '15px', textAlign: 'center' }}>
                <p>Don't have an account? <Link to="/register">Register here</Link></p>
                <p><Link to="/forgot-password">Forgot Password?</Link></p>
            </div>
        </div>
    );
};

export default Login;
