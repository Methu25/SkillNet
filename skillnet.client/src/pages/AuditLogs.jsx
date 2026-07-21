import React, { useState, useEffect } from 'react';
import { apiRequest } from '../api/apiClient';
import '../AdminModule.css';

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

        apiRequest(`/api/auditlog?${params.toString()}`)
            .then(res => {
                setLogs(Array.isArray(res.data) ? res.data : []);
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
        <div className="admin-module-container">
            <h2 className="admin-page-title">System Audit Logs</h2>

            {/* Filter Bar */}
            <div className="admin-filter-bar">
                <div className="admin-filter-group">
                    <label>User ID</label>
                    <input className="admin-input" type="number" placeholder="e.g. 1" value={userId} onChange={e => setUserId(e.target.value)} style={{ width: '100%', boxSizing: 'border-box' }} />
                </div>
                <div className="admin-filter-group">
                    <label>Action Type</label>
                    <input className="admin-input" type="text" placeholder="e.g. DELETE" value={action} onChange={e => setAction(e.target.value)} style={{ width: '100%', boxSizing: 'border-box' }} />
                </div>
                <div className="admin-filter-group">
                    <label>Start Date</label>
                    <input className="admin-input" type="date" value={startDate} onChange={e => setStartDate(e.target.value)} style={{ width: '100%', boxSizing: 'border-box' }} />
                </div>
                <div className="admin-filter-group">
                    <label>End Date</label>
                    <input className="admin-input" type="date" value={endDate} onChange={e => setEndDate(e.target.value)} style={{ width: '100%', boxSizing: 'border-box' }} />
                </div>
                <button onClick={fetchLogs} className="admin-btn admin-btn-primary" style={{ height: '42px', padding: '0 2rem' }}>
                    Filter Logs
                </button>
            </div>

            {/* Read-Only Data Table */}
            <div className="admin-card" style={{ overflowX: 'auto', padding: 0 }}>
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>Log ID</th>
                            <th>Timestamp</th>
                            <th>User ID</th>
                            <th>Action</th>
                            <th>Details</th>
                            <th>IP Address</th>
                        </tr>
                    </thead>
                    <tbody>
                        {loading ? (
                            <tr><td colSpan="6" style={{ padding: '20px', textAlign: 'center' }}>Loading logs...</td></tr>
                        ) : logs.length === 0 ? (
                            <tr><td colSpan="6" style={{ padding: '20px', textAlign: 'center', opacity: 0.7 }}>No logs found matching criteria.</td></tr>
                        ) : (
                            logs.map((log) => (
                                <tr key={log.logId || log.LogId}>
                                    <td style={{ opacity: 0.7 }}>{log.logId || log.LogId}</td>
                                    <td style={{ whiteSpace: 'nowrap' }}>{new Date(log.timestamp || log.Timestamp).toLocaleString()}</td>
                                    <td>{log.userId || log.UserId || 'System'}</td>
                                    <td style={{ fontWeight: 'bold', color: 'var(--text-h)' }}>{log.action || log.Action}</td>
                                    <td style={{ opacity: 0.7 }}>{log.details || log.Details || '-'}</td>
                                    <td style={{ opacity: 0.7, fontFamily: 'var(--mono)' }}>{log.ipAddress || log.IpAddress || '-'}</td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}