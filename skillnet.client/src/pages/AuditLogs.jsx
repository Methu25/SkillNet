import React, { useState, useEffect } from 'react';

export default function AuditLogs() {
    const [logs, setLogs] = useState([]);
    const [loading, setLoading] = useState(true);

    // Filter States
    const [userId, setUserId] = useState('');
    const [action, setAction] = useState('');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');

    // Function to fetch logs based on active filters
    const fetchLogs = () => {
        setLoading(true);

        // Build the query string dynamically
        const params = new URLSearchParams();
        if (userId) params.append('userId', userId);
        if (action) params.append('action', action);
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);

        fetch(`/api/auditlog?${params.toString()}`)
            .then(res => res.json())
            .then(data => {
                setLogs(Array.isArray(data) ? data : []);
                setLoading(false);
            })
            .catch(err => {
                console.error("Error fetching logs:", err);
                setLoading(false);
            });
    };

    // Fetch all logs when the component first loads
    useEffect(() => {
        fetchLogs();
    }, []);

    return (
        <div>
            <h2 style={{ margin: '0 0 20px 0' }}>System Audit Logs</h2>

            {/* Filter Bar */}
            <div style={{ background: '#fff', padding: '16px', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)', marginBottom: '20px', display: 'flex', gap: '10px', alignItems: 'flex-end' }}>
                <div style={{ flex: 1 }}>
                    <label style={{ fontSize: '12px', fontWeight: 'bold', color: '#64748b', display: 'block', marginBottom: '4px' }}>User ID</label>
                    <input type="number" placeholder="e.g. 1" value={userId} onChange={e => setUserId(e.target.value)} style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #cbd5e1' }} />
                </div>
                <div style={{ flex: 1 }}>
                    <label style={{ fontSize: '12px', fontWeight: 'bold', color: '#64748b', display: 'block', marginBottom: '4px' }}>Action Type</label>
                    <input type="text" placeholder="e.g. DELETE" value={action} onChange={e => setAction(e.target.value)} style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #cbd5e1' }} />
                </div>
                <div style={{ flex: 1 }}>
                    <label style={{ fontSize: '12px', fontWeight: 'bold', color: '#64748b', display: 'block', marginBottom: '4px' }}>Start Date</label>
                    <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)} style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #cbd5e1' }} />
                </div>
                <div style={{ flex: 1 }}>
                    <label style={{ fontSize: '12px', fontWeight: 'bold', color: '#64748b', display: 'block', marginBottom: '4px' }}>End Date</label>
                    <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)} style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #cbd5e1' }} />
                </div>
                <button onClick={fetchLogs} style={{ background: '#3b82f6', color: 'white', border: 'none', padding: '9px 16px', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                    Filter
                </button>
            </div>

            {/* Read-Only Data Table */}
            <div style={{ background: '#fff', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)', overflow: 'hidden' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '14px' }}>
                    <thead style={{ background: '#f8fafc', borderBottom: '2px solid #e2e8f0' }}>
                        <tr>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Log ID</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Timestamp</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>User ID</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Action</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>Details</th>
                            <th style={{ padding: '12px 16px', color: '#475569' }}>IP Address</th>
                        </tr>
                    </thead>
                    <tbody>
                        {loading ? (
                            <tr><td colSpan="6" style={{ padding: '20px', textAlign: 'center' }}>Loading logs...</td></tr>
                        ) : logs.length === 0 ? (
                            <tr><td colSpan="6" style={{ padding: '20px', textAlign: 'center', color: '#94a3b8' }}>No logs found matching criteria.</td></tr>
                        ) : (
                            logs.map((log) => (
                                <tr key={log.logId || log.LogId} style={{ borderBottom: '1px solid #e2e8f0' }}>
                                    <td style={{ padding: '12px 16px', color: '#64748b' }}>{log.logId || log.LogId}</td>
                                    <td style={{ padding: '12px 16px', whiteSpace: 'nowrap' }}>{new Date(log.timestamp || log.Timestamp).toLocaleString()}</td>
                                    <td style={{ padding: '12px 16px' }}>{log.userId || log.UserId || 'System'}</td>
                                    <td style={{ padding: '12px 16px', fontWeight: 'bold', color: '#1e293b' }}>{log.action || log.Action}</td>
                                    <td style={{ padding: '12px 16px', color: '#64748b' }}>{log.details || log.Details || '-'}</td>
                                    <td style={{ padding: '12px 16px', color: '#64748b', fontFamily: 'monospace' }}>{log.ipAddress || log.IpAddress || '-'}</td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}