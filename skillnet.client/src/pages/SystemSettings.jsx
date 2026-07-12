import { useState, useEffect } from 'react';

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

    if (loading) return <h2>Loading System Configurations...</h2>;

    return (
        <div style={{ maxWidth: '800px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h2 style={{ margin: 0, color: '#000' }}>System Settings</h2>
                <button onClick={handleSave} style={{ background: '#3b82f6', color: 'white', border: 'none', padding: '10px 16px', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold' }}>
                    Save Changes
                </button>
            </div>

            <div style={{ background: '#fff', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)', padding: '24px' }}>
                <form onSubmit={e => e.preventDefault()}>
                    {configs.map((config) => (
                        <div key={config.key} style={{ marginBottom: '20px' }}>
                            <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '8px', color: '#1e293b' }}>
                                {config.key}
                            </label>
                            <input
                                type="text"
                                value={config.value}
                                onChange={(e) => handleChange(config.key, e.target.value)}
                                style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #cbd5e1', fontSize: '14px' }}
                            />
                            <div style={{ fontSize: '12px', color: '#64748b', marginTop: '4px' }}>
                                {config.description}
                            </div>
                        </div>
                    ))}
                </form>
            </div>
        </div>
    );
}