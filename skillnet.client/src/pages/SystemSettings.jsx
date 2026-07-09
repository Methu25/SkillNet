import { useState, useEffect } from 'react';

export default function SystemSettings() {
    const [configs, setConfigs] = useState([]);
    const [loading, setLoading] = useState(true);

    // Fetch the system configurations from your C# API
    useEffect(() => {
        fetch('/api/systemconfiguration')
            .then(res => res.json())
            .then(data => {
                setConfigs(Array.isArray(data) ? data : []);
                setLoading(false);
            })
            .catch(err => {
                console.error("Error fetching configs:", err);
                setLoading(false);
            });
    }, []);

    if (loading) {
        return <h2>Loading System Configurations...</h2>;
    }

    return (
        <div style={{ maxWidth: '800px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h2 style={{ margin: 0 }}>System Settings</h2>
                <button style={{ background: '#3b82f6', color: 'white', border: 'none', padding: '10px 16px', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold' }}>
                    Save Changes
                </button>
            </div>

            <div style={{ background: '#fff', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)', padding: '24px' }}>
                <p style={{ color: '#64748b', marginTop: 0, marginBottom: '24px' }}>
                    Modify overarching platform configurations. Changes here affect all organizations and users.
                </p>

                {configs.length === 0 ? (
                    <div style={{ padding: '20px', textAlign: 'center', color: '#94a3b8', background: '#f8fafc', borderRadius: '6px' }}>
                        No configurations found in the database.
                        <br /><br />
                        <span style={{ fontSize: '12px' }}>(Hint: You can insert defaults directly into your SQL table!)</span>
                    </div>
                ) : (
                    <form>
                        {configs.map((config) => (
                            <div key={config.key || config.Key} style={{ marginBottom: '20px' }}>
                                <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '8px', color: '#1e293b' }}>
                                    {config.key || config.Key}
                                </label>
                                <input
                                    type="text"
                                    defaultValue={config.value || config.Value}
                                    style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #cbd5e1', fontSize: '14px' }}
                                />
                                <div style={{ fontSize: '12px', color: '#64748b', marginTop: '4px' }}>
                                    {config.description || config.Description}
                                </div>
                            </div>
                        ))}
                    </form>
                )}
            </div>
        </div>
    );
}