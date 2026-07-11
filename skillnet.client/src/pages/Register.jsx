import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

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
        <div style={{ maxWidth: '450px', margin: '30px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
            <h2>Register for SkillNet</h2>
            {error && <div style={{ color: 'red', marginBottom: '15px', padding: '8px', backgroundColor: '#ffe6e6', borderRadius: '4px' }}>{error}</div>}
            {success && <div style={{ color: 'green', marginBottom: '15px', padding: '8px', backgroundColor: '#e6ffe6', borderRadius: '4px' }}>{success}</div>}
            
            <form onSubmit={handleSubmit}>
                <div style={{ display: 'flex', gap: '10px', marginBottom: '15px' }}>
                    <div style={{ flex: 1 }}>
                        <label style={{ display: 'block', marginBottom: '5px' }}>First Name</label>
                        <input 
                            type="text" 
                            value={firstName} 
                            onChange={(e) => setFirstName(e.target.value)} 
                            required 
                            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                        />
                    </div>
                    <div style={{ flex: 1 }}>
                        <label style={{ display: 'block', marginBottom: '5px' }}>Last Name</label>
                        <input 
                            type="text" 
                            value={lastName} 
                            onChange={(e) => setLastName(e.target.value)} 
                            required 
                            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                        />
                    </div>
                </div>

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
                    <label style={{ display: 'block', marginBottom: '5px' }}>Phone Number (Optional)</label>
                    <input 
                        type="text" 
                        value={phone} 
                        onChange={(e) => setPhone(e.target.value)} 
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                </div>

                <div style={{ marginBottom: '15px' }}>
                    <label style={{ display: 'block', marginBottom: '5px' }}>Register As</label>
                    <select 
                        value={roleName} 
                        onChange={(e) => setRoleName(e.target.value)} 
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    >
                        <option value="Candidate">Candidate</option>
                        <option value="Recruiter">Recruiter</option>
                        <option value="HiringManager">Hiring Manager</option>
                        <option value="Admin">Administrator</option>
                    </select>
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
                    
                    {/* Password Policy visual checklist */}
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
                    <label style={{ display: 'block', marginBottom: '5px' }}>Confirm Password</label>
                    <input 
                        type="password" 
                        value={confirmPassword} 
                        onChange={(e) => setConfirmPassword(e.target.value)} 
                        required 
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                </div>

                <button type="submit" disabled={loading} style={{ width: '100%', padding: '10px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                    {loading ? 'Registering...' : 'Register'}
                </button>
            </form>
            <div style={{ marginTop: '15px', textAlign: 'center' }}>
                <p>Already have an account? <Link to="/login">Login here</Link></p>
            </div>
        </div>
    );
};

export default Register;
