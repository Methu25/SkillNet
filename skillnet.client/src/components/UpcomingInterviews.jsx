export default function UpcomingInterviews({ interviews, onSelectInterview }) {
    const upcomingInterviews = interviews.filter(i => i.status === "Scheduled");

    return (
        <div>
            <h2>Upcoming Interviews</h2>
            {upcomingInterviews.length === 0 ? (
                <p style={{ color: '#999' }}>No upcoming interviews scheduled.</p>
            ) : (
                <ul style={{ listStyle: 'none', padding: 0 }}>
                    {upcomingInterviews.map(interview => (
                        <li
                            key={interview.id}
                            style={{
                                padding: '15px',
                                border: '1px solid #e0e0e0',
                                marginBottom: '10px',
                                borderRadius: '5px',
                                backgroundColor: '#f5f5f5'
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
                                    backgroundColor: '#9c27b0',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '3px'
                                }}
                            >
                                View Interview
                            </button>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
