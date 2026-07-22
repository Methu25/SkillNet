export default function CompletedInterviews({ interviews, onSelectInterview }) {
    const completedInterviews = interviews.filter(i => i.status === "Completed");

    return (
        <div>
            <h2>Completed Interviews</h2>
            {completedInterviews.length === 0 ? (
                <p style={{ color: '#999' }}>No completed interviews yet.</p>
            ) : (
                <ul style={{ listStyle: 'none', padding: 0 }}>
                    {completedInterviews.map(interview => (
                        <li
                            key={interview.id}
                            style={{
                                padding: '15px',
                                border: '1px solid #c8e6c9',
                                backgroundColor: '#e8f5e9',
                                marginBottom: '10px',
                                borderRadius: '5px'
                            }}
                        >
                            <strong>{interview.candidateName}</strong> - {interview.role} <br />
                            <small style={{ color: '#666' }}>
                                {interview.date} at {interview.time} | {interview.type}
                            </small>
                            <br />
                            <button
                                onClick={() => onSelectInterview(interview)}
                                style={{
                                    marginTop: '10px',
                                    padding: '5px 10px',
                                    cursor: 'pointer',
                                    backgroundColor: '#4caf50',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '3px'
                                }}
                            >
                                View Details
                            </button>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
