import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { applicationApi } from '../api/applicationApi';
import { useAuth } from '../context/AuthContext';

const RecruiterDashboard = () => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const [testResponse, setTestResponse] = useState('');
    const [jobs, setJobs] = useState([]);
    const [jobsLoading, setJobsLoading] = useState(true);
    const [jobsError, setJobsError] = useState('');

    const loadJobs = useCallback(async () => {
        setJobsLoading(true);
        setJobsError('');
        try {
            const result = await applicationApi.getRecruiterJobs();
            setJobs(Array.isArray(result) ? result : []);
        } catch (error) {
            setJobsError(error.message || 'Your jobs could not be loaded.');
        } finally {
            setJobsLoading(false);
        }
    }, []);

    useEffect(() => {
        // The recruiter job request initializes the dashboard entry point.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadJobs();
    }, [loadJobs]);

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
            <h1 style={{ color: '#007bff' }}>Recruiter Dashboard 💼</h1>
            <p>Welcome, Recruiter!</p>
            
            <div style={{ backgroundColor: '#f8f9fa', padding: '15px', borderRadius: '4px', marginBottom: '20px' }}>
                <h3>Profile Information</h3>
                <p><strong>Email:</strong> {user?.email}</p>
                <p><strong>Name:</strong> {user?.firstName} {user?.lastName}</p>
                <p><strong>Phone:</strong> {user?.phone || 'N/A'}</p>
                <p><strong>Roles:</strong> {user?.roles?.join(', ')}</p>
                <p><strong>Status:</strong> {user?.status}</p>
            </div>

            <section style={{ border: '1px solid #f0d5c6', padding: '15px', borderRadius: '8px', marginBottom: '20px', background: '#fffaf7' }}>
                <h2 style={{ marginTop: 0 }}>Your Jobs</h2>
                {jobsLoading ? <p>Loading your jobs…</p>
                    : jobsError ? <div role="alert"><p>{jobsError}</p><button onClick={loadJobs}>Retry</button></div>
                    : jobs.length === 0 ? <p>No jobs are available for this recruiter profile.</p>
                    : jobs.map(job => <article key={job.jobId} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '12px', padding: '12px 0', borderTop: '1px solid #eee2db' }}>
                        <div><strong>{job.title}</strong><div style={{ color: '#68707c', fontSize: '.82rem' }}>{job.status} · {job.location || job.workMode || 'Location not listed'}</div></div>
                        <button onClick={() => navigate(`/recruiter/jobs/${job.jobId}/applicants`)} style={{ padding: '9px 13px', color: '#fff', background: '#FF681F', border: 0, borderRadius: '8px', cursor: 'pointer', fontWeight: 700 }}>View Applicants</button>
                    </article>)}
            </section>

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

export default RecruiterDashboard;
