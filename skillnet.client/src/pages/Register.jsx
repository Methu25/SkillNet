import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';
import './Register.css';

const Register = () => {
    const { register } = useAuth();
    const navigate = useNavigate();

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [phone, setPhone] = useState('');
    const [roleName, setRoleName] = useState('Candidate');
    
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [loading, setLoading] = useState(false);

    // Live password check helper
    const checkPasswordPolicy = (pwd) => {
        const checks = {
            length: pwd.length >= 8,
            upper: /[A-Z]/.test(pwd),
            lower: /[a-z]/.test(pwd),
            number: /[0-9]/.test(pwd),
            special: /[^a-zA-Z0-9]/.test(pwd),
        };
        return checks;
    };

    const pwdChecks = checkPasswordPolicy(password);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        if (password !== confirmPassword) {
            setError('Passwords do not match.');
            return;
        }

        // Validate password policy locally first
        const isValid = Object.values(pwdChecks).every(Boolean);
        if (!isValid) {
            setError('Password does not satisfy the security policy.');
            return;
        }

        setLoading(true);
        try {
            await register({
                email,
                password,
                firstName,
                lastName,
                phone: phone || null,
                roleName
            });
            setSuccess('Registration successful! Redirecting to login...');
            setTimeout(() => {
                navigate('/login');
            }, 2000);
        } catch (err) {
            setError(err.message || 'Registration failed.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="register-container">
            <div className="register-card">
                <Link to="/" className="register-logo-link">Skill<span>Net</span>.</Link>
                <h2 className="register-title">Create Account</h2>
                <p className="register-subtitle">Join SkillNet to unlock the future of hiring</p>

                {error && <div className="register-error">{error}</div>}
                {success && <div className="register-success">{success}</div>}
                
                <form onSubmit={handleSubmit}>
                    <div className="register-form-row">
                        <div>
                            <label className="register-label">First Name</label>
                            <input 
                                type="text" 
                                className="register-input"
                                placeholder="John"
                                value={firstName} 
                                onChange={(e) => setFirstName(e.target.value)} 
                                required 
                            />
                        </div>
                        <div>
                            <label className="register-label">Last Name</label>
                            <input 
                                type="text" 
                                className="register-input"
                                placeholder="Doe"
                                value={lastName} 
                                onChange={(e) => setLastName(e.target.value)} 
                                required 
                            />
                        </div>
                    </div>

                    <div className="register-form-group">
                        <label className="register-label">Email Address</label>
                        <input 
                            type="email" 
                            className="register-input"
                            placeholder="name@example.com"
                            value={email} 
                            onChange={(e) => setEmail(e.target.value)} 
                            required 
                        />
                    </div>

                    <div className="register-form-group">
                        <label className="register-label">Phone Number (Optional)</label>
                        <input 
                            type="text" 
                            className="register-input"
                            placeholder="+1 (555) 000-0000"
                            value={phone} 
                            onChange={(e) => setPhone(e.target.value)} 
                        />
                    </div>

                    <div className="register-form-group">
                        <label className="register-label">Register As</label>
                        <select 
                            value={roleName} 
                            onChange={(e) => setRoleName(e.target.value)} 
                            className="register-select"
                        >
                            <option value="Candidate">Candidate</option>
                            <option value="Recruiter">Recruiter</option>
                            <option value="HiringManager">Hiring Manager</option>
                            <option value="Admin">Administrator</option>
                        </select>
                    </div>

                    <div className="register-form-group">
                        <label className="register-label">Password</label>
                        <input 
                            type="password" 
                            className="register-input"
                            placeholder="••••••••"
                            value={password} 
                            onChange={(e) => setPassword(e.target.value)} 
                            required 
                        />
                        
                        {/* Password Policy checklist */}
                        <div className="register-password-policy">
                            <strong>Password Requirements:</strong>
                            <ul>
                                <li className={pwdChecks.length ? 'valid' : 'invalid'}>At least 8 characters</li>
                                <li className={pwdChecks.upper ? 'valid' : 'invalid'}>At least 1 uppercase letter</li>
                                <li className={pwdChecks.lower ? 'valid' : 'invalid'}>At least 1 lowercase letter</li>
                                <li className={pwdChecks.number ? 'valid' : 'invalid'}>At least 1 number</li>
                                <li className={pwdChecks.special ? 'valid' : 'invalid'}>At least 1 special character</li>
                            </ul>
                        </div>
                    </div>

                    <div className="register-form-group" style={{ marginBottom: '1.5rem' }}>
                        <label className="register-label">Confirm Password</label>
                        <input 
                            type="password" 
                            className="register-input"
                            placeholder="••••••••"
                            value={confirmPassword} 
                            onChange={(e) => setConfirmPassword(e.target.value)} 
                            required 
                        />
                    </div>

                    <button type="submit" className="register-btn" disabled={loading}>
                        {loading ? 'Creating Account...' : 'Create Account'}
                    </button>
                </form>
                
                <div className="register-footer">
                    <p>Already have an account? <Link to="/login">Login here</Link></p>
                </div>
            </div>
        </div>
    );
};

export default Register;
