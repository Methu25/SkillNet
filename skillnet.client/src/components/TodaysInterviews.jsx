export default function TodaysInterviews({ interviews, onSelectInterview }) {
    const todaysInterviews = interviews.filter(i => i.date === "Today");

    return (
        <div>
            <h2>Today's Interviews</h2>
            <ul style={{ listStyle: 'none', padding: 0 }}>
                {todaysInterviews.map(interview => (
                    <li
                        key={interview.id}
                        style={{
                            padding: '15px',
                            border: '1px solid #eee',
                            marginBottom: '10px',
                            borderRadius: '5px',
                            backgroundColor: '#fff'
                        }}
                    >
                        <strong>{interview.candidateName}</strong> - {interview.role} <br />
                        <small style={{ color: '#666' }}>
                            {interview.time} | {interview.type}
                        </small>
                        <br />
                        <button
                            onClick={() => onSelectInterview(interview)}
                            style={{
                                marginTop: '10px',
                                padding: '5px 10px',
                                cursor: 'pointer',
                                backgroundColor: '#2196f3',
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
        </div>
    );
}
