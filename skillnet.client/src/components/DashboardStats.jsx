export default function DashboardStats({ stats }) {
    const statItems = [
        { label: 'Pending Feedback', value: stats.pendingFeedback, color: '#ff9800' },
        { label: 'Completed Today', value: stats.completedToday, color: '#4caf50' },
        { label: 'Upcoming This Week', value: stats.upcomingThisWeek, color: '#2196f3' }
    ];

    return (
        <div style={{ display: 'flex', gap: '20px', marginBottom: '30px' }}>
            {statItems.map((item, index) => (
                <div
                    key={index}
                    style={{
                        flex: 1,
                        padding: '20px',
                        border: `3px solid ${item.color}`,
                        borderRadius: '8px',
                        backgroundColor: '#f9f9f9',
                        textAlign: 'center'
                    }}
                >
                    <h3 style={{ margin: '0 0 10px 0', color: item.color }}>
                        {item.value}
                    </h3>
                    <p style={{ margin: 0, color: '#666' }}>
                        {item.label}
                    </p>
                </div>
            ))}
        </div>
    );
}
