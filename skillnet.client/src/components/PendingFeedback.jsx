export default function PendingFeedback({ interviews, onSelectInterview }) {
    const pendingInterviews = interviews.filter(i => i.status === "Pending Feedback");

    return (
        <div>
            <h2>Needs Feedback</h2>
            <ul style={{ listStyle: 'none', padding: 0 }}>
                {pendingInterviews.map(interview => (
                    <li
                        key={interview.id}
                        style={{
                            padding: '15px',
                            border: '1px solid #ffeeba',
                            backgroundColor: '#fff3cd',
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
                                backgroundColor: '#ffc107',
                                color: '#000',
                                border: 'none',
                                cursor: 'pointer',
                                borderRadius: '3px',
                                fontWeight: 'bold'
                            }}
                        >
                            Submit Evaluation
                        </button>
                    </li>
                ))}
            </ul>
        </div>
    );
}
