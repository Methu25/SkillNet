import { useState, useEffect } from 'react';
import '../AdminModule.css';

export default function SystemSettings() {
    const [configs, setConfigs] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetch('/api/systemconfiguration', { cache: 'no-store' })
            .then(res => res.json())
            .then(data => {
                setConfigs(Array.isArray(data) ? data : []);
                setLoading(false);
            })
            .catch(err => console.error(err));
    }, []);

    // Function to handle typing in the text boxes
    const handleChange = (key, newValue) => {
        setConfigs(configs.map(c => c.key === key ? { ...c, value: newValue } : c));
    };

    // Function to send data to C# when Save is clicked
    const handleSave = () => {
        fetch('/api/systemconfiguration', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(configs)
        })
            .then(res => res.json())
            .then(data => alert(data.message))
            .catch(err => alert("Failed to save changes."));
    };

    if (loading) return <h2 className="admin-page-title">Loading System Configurations...</h2>;

    return (
        <div className="admin-module-container">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px', flexWrap: 'wrap', gap: '1rem' }}>
                <h2 className="admin-page-title" style={{ marginBottom: 0 }}>System Settings</h2>
                <button onClick={handleSave} className="admin-btn admin-btn-primary">
                    Save Changes
                </button>
            </div>

            <div className="admin-card">
                <form onSubmit={e => e.preventDefault()} className="admin-form">
                    {configs.map((config) => (
                        <div key={config.key} className="admin-settings-row">
                            <div className="admin-settings-info">
                                <h4>{config.key}</h4>
                                <p>{config.description}</p>
                            </div>
                            <div style={{ flex: '0 0 300px' }}>
                                {['AllowMultipleApplications', 'RequireStrongPassword'].includes(config.key) ? (
                                    <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', height: '100%' }}>
                                        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
                                            <input 
                                                type="radio" 
                                                name={config.key} 
                                                value="true"
                                                checked={config.value === 'true' || config.value === 'True' || config.value === '1'} 
                                                onChange={() => handleChange(config.key, 'true')} 
                                            />
                                            Yes
                                        </label>
                                        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
                                            <input 
                                                type="radio" 
                                                name={config.key} 
                                                value="false"
                                                checked={config.value === 'false' || config.value === 'False' || config.value === '0'} 
                                                onChange={() => handleChange(config.key, 'false')} 
                                            />
                                            No
                                        </label>
                                    </div>
                                ) : (
                                    <input
                                        type="text"
                                        value={config.value}
                                        onChange={(e) => handleChange(config.key, e.target.value)}
                                        className="admin-input"
                                        style={{ width: '100%' }}
                                    />
                                )}
                            </div>
                        </div>
                    ))}
                    {configs.length === 0 && <p style={{ opacity: 0.7 }}>No configuration keys found.</p>}
                </form>
            </div>
        </div>
    );
}