import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link, Navigate } from 'react-router-dom';
import './Login.css';

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
        <div className="login-container">
            <div className="login-card">
                <Link to="/" className="login-logo-link">Skill<span>Net</span>.</Link>
                <h2 className="login-title">Welcome Back</h2>
                <p className="login-subtitle">Log in to manage your recruitment process</p>

                {error && <div className="login-error">{error}</div>}

                <form onSubmit={handleSubmit}>
                    <div className="login-form-group">
                        <label className="login-label">Email Address</label>
                        <input
                            type="email"
                            className="login-input"
                            placeholder="name@example.com"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                        />
                    </div>
                    <div className="login-form-group">
                        <label className="login-label">Password</label>
                        <input
                            type="password"
                            className="login-input"
                            placeholder="••••••••"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                        />
                    </div>
                    <button type="submit" className="login-btn" disabled={submitting}>
                        {submitting ? 'Logging in...' : 'Log In'}
                    </button>
                </form>

                <div className="login-footer">
                    <p>Don't have an account? <Link to="/register">Register here</Link></p>
                    <div className="login-footer-links">
                        <Link to="/forgot-password">Forgot Password?</Link>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Login;
