export default function CandidateEvaluations({
    selectedInterview,
    evaluationForm,
    onInputChange,
    onSubmit,
    onBack
}) {
    if (!selectedInterview) return null;

    return (
        <div style={{ padding: '20px', fontFamily: 'sans-serif', maxWidth: '1400px' }}>
            <button
                onClick={onBack}
                style={{
                    marginBottom: '20px',
                    padding: '8px 15px',
                    backgroundColor: '#757575',
                    color: 'white',
                    border: 'none',
                    cursor: 'pointer',
                    borderRadius: '3px'
                }}
            >
                ← Back to Dashboard
            </button>

            <h2>Evaluating: {selectedInterview.candidateName}</h2>
            <p>
                <strong>Role:</strong> {selectedInterview.role} |{' '}
                <strong>Type:</strong> {selectedInterview.type}
            </p>

            <hr style={{ margin: '20px 0' }} />

            {/* TWO-COLUMN LAYOUT */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '30px' }}>

                {/* LEFT COLUMN: EVALUATION FORM */}
                <div>
                    <h3>Evaluation Form</h3>
                    <form
                        onSubmit={onSubmit}
                        style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}
                    >
                        <div>
                            <label>Technical Score (1-10): </label>
                            <input
                                type="number"
                                name="technicalScore"
                                min="1"
                                max="10"
                                value={evaluationForm.technicalScore}
                                onChange={onInputChange}
                                required
                            />
                        </div>

                        <div>
                            <label>Communication Score (1-10): </label>
                            <input
                                type="number"
                                name="communicationScore"
                                min="1"
                                max="10"
                                value={evaluationForm.communicationScore}
                                onChange={onInputChange}
                                required
                            />
                        </div>

                        <div>
                            <label>Problem Solving Score (1-10): </label>
                            <input
                                type="number"
                                name="problemSolvingScore"
                                min="1"
                                max="10"
                                value={evaluationForm.problemSolvingScore}
                                onChange={onInputChange}
                                required
                            />
                        </div>

                        <div>
                            <label>Culture Fit Score (1-10): </label>
                            <input
                                type="number"
                                name="cultureFitScore"
                                min="1"
                                max="10"
                                value={evaluationForm.cultureFitScore}
                                onChange={onInputChange}
                                required
                            />
                        </div>

                        <div>
                            <label>Recommendation: </label>
                            <select
                                name="recommendation"
                                value={evaluationForm.recommendation}
                                onChange={onInputChange}
                                required
                            >
                                <option value="">-- Select --</option>
                                <option value="Strong Hire">Strong Hire</option>
                                <option value="Hire">Hire</option>
                                <option value="Next Round">Next Round</option>
                                <option value="Hold">Hold</option>
                                <option value="Reject">Reject</option>
                            </select>
                        </div>

                        <div>
                            <label>Final Comments: </label>
                            <br />
                            <textarea
                                name="comments"
                                rows="4"
                                style={{ width: '100%' }}
                                value={evaluationForm.comments}
                                onChange={onInputChange}
                                required
                            ></textarea>
                        </div>

                        <button
                            type="submit"
                            style={{
                                padding: '10px',
                                backgroundColor: '#28a745',
                                color: 'white',
                                border: 'none',
                                cursor: 'pointer',
                                borderRadius: '3px',
                                fontWeight: 'bold'
                            }}
                        >
                            Submit Evaluation
                        </button>
                    </form>
                </div>

                {/* RIGHT COLUMN: CANDIDATE INFORMATION */}
                <div>
                    <h3>Candidate Information</h3>

                    {/* RESUME SUMMARY */}
                    <div
                        style={{
                            marginBottom: '20px',
                            padding: '15px',
                            border: '1px solid #ddd',
                            borderRadius: '5px',
                            backgroundColor: '#f9f9f9'
                        }}
                    >
                        <h4>Resume Summary</h4>
                        <p>
                            <strong>Email:</strong>{' '}
                            {selectedInterview.candidateName
                                .toLowerCase()
                                .replace(/\s+/g, '.')}
                            @example.com
                        </p>
                        <p>
                            <strong>Phone:</strong> +1 (555) 123-4567
                        </p>
                        <p>
                            <strong>Location:</strong> San Francisco, CA
                        </p>
                        <p>
                            <strong>Years of Experience:</strong> 5-7 years
                        </p>
                        <p>
                            <strong>Key Skills:</strong> React, Node.js, SQL, AWS, Docker
                        </p>
                        <p>
                            <strong>Education:</strong> BS in Computer Science
                        </p>
                    </div>

                    {/* APPLICATION SUMMARY */}
                    <div
                        style={{
                            padding: '15px',
                            border: '1px solid #ddd',
                            borderRadius: '5px',
                            backgroundColor: '#f9f9f9'
                        }}
                    >
                        <h4>Application Summary</h4>
                        <p>
                            <strong>Application Date:</strong> January 15, 2025
                        </p>
                        <p>
                            <strong>Source:</strong> LinkedIn
                        </p>
                        <p>
                            <strong>Status:</strong> In Progress
                        </p>
                        <p>
                            <strong>Current Round:</strong> Interview Evaluation
                        </p>
                        <p>
                            <strong>Cover Letter Excerpt:</strong> "I am excited to apply for
                            the {selectedInterview.role} position. With my background in
                            full-stack development and passion for clean code, I believe I
                            can make a significant contribution to your team."
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}
