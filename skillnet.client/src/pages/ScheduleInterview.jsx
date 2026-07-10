export default function ScheduleInterview() {
    return (
        <div style={{ padding: '20px', maxWidth: '600px', margin: '0 auto' }}>
            <h2>Schedule a New Interview</h2>

            <form style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
                <div>
                    <label>Candidate Name:</label>
                    <input type="text" placeholder="Enter name" style={{ width: '100%', padding: '8px' }} />
                </div>

                <div>
                    <label>Interview Type:</label>
                    <select style={{ width: '100%', padding: '8px' }}>
                        <option>Technical</option>
                        <option>Communication</option>
                        <option>Problem Solving</option>
                    </select>
                </div>

                <button type="button" style={{ padding: '10px', background: 'blue', color: 'white' }}>
                    Save Interview
                </button>
            </form>
        </div>
    );
}