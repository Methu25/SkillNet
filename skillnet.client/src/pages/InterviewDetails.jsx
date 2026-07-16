import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import './InterviewDetails.css';

function InterviewDetails() {
    const { id } = useParams();

    const [interview, setInterview] = useState(null);
    const [evaluation, setEvaluation] = useState({
        interviewerId: '',
        technicalScore: '',
        communicationScore: '',
        problemSolvingScore: '',
        cultureFitScore: '',
        recommendation: 'Hold',
        comments: ''
    });

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [isSubmitted, setIsSubmitted] = useState(false);

    useEffect(() => {
        const loadInterviewDetails = async () => {
            try {
                setLoading(true);
                setError('');

                const response = await fetch(`/api/interviews/${id}`);

                if (!response.ok) {
                    throw new Error('Failed to load interview details from database.');
                }

                const data = await response.json();
                setInterview(data);
            } catch (err) {
                setError(err.message || 'Something went wrong while loading interview details.');
            } finally {
                setLoading(false);
            }
        };

        loadInterviewDetails();
    }, [id]);

    const formatDateTime = (dateValue) => {
        if (!dateValue) return 'Not scheduled';

        const date = new Date(dateValue);

        if (Number.isNaN(date.getTime())) {
            return dateValue;
        }

        return date.toLocaleString();
    };

    const calculateOverallScore = () => {
        const technical = Number(evaluation.technicalScore) || 0;
        const communication = Number(evaluation.communicationScore) || 0;
        const problemSolving = Number(evaluation.problemSolvingScore) || 0;
        const cultureFit = Number(evaluation.cultureFitScore) || 0;

        if (
            technical === 0 &&
            communication === 0 &&
            problemSolving === 0 &&
            cultureFit === 0
        ) {
            return 0;
        }

        return Math.round((technical + communication + problemSolving + cultureFit) / 4);
    };

    const handleEvaluationChange = (e) => {
        const { name, value } = e.target;

        setEvaluation({
            ...evaluation,
            [name]: value
        });
    };

    const handleSubmitEvaluation = async () => {
        try {
            if (
                !evaluation.interviewerId ||
                !evaluation.technicalScore ||
                !evaluation.communicationScore ||
                !evaluation.problemSolvingScore ||
                !evaluation.cultureFitScore
            ) {
                alert('Please fill Interviewer ID and all score fields.');
                return;
            }

            const requestBody = {
                interviewerId: Number(evaluation.interviewerId),
                technicalScore: Number(evaluation.technicalScore),
                communicationScore: Number(evaluation.communicationScore),
                problemSolvingScore: Number(evaluation.problemSolvingScore),
                cultureFitScore: Number(evaluation.cultureFitScore),
                recommendation: evaluation.recommendation,
                comments: evaluation.comments
            };

            const response = await fetch(`/api/interviews/${id}/evaluation`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Failed to submit evaluation.');
            }

            setIsSubmitted(true);
            alert('Evaluation submitted successfully.');
        } catch (err) {
            alert(err.message || 'Something went wrong while submitting evaluation.');
        }
    };

    if (loading) {
        return (
            <div className="interview-details-page">
                <h2>Loading interview details from database...</h2>
            </div>
        );
    }

    if (error) {
        return (
            <div className="interview-details-page">
                <h2>Database/API Error</h2>
                <p>{error}</p>

                <Link className="back-button" to="/hiring-dashboard">
                    Back to Dashboard
                </Link>
            </div>
        );
    }

    if (!interview) {
        return (
            <div className="interview-details-page">
                <h2>Interview not found.</h2>

                <Link className="back-button" to="/hiring-dashboard">
                    Back to Dashboard
                </Link>
            </div>
        );
    }

    return (
        <div className="interview-details-page">
            <div className="details-header">
                <div>
                    <h1>Interview Details</h1>
                    <p>Interview ID: {interview.interviewId}</p>
                </div>

                <Link className="back-button" to="/hiring-dashboard">
                    Back to Dashboard
                </Link>
            </div>

            <div className="details-grid">
                <section className="details-card">
                    <h2>Candidate Information</h2>
                    <p><strong>Name:</strong> {interview.candidateName || 'Not available'}</p>
                    <p><strong>Email:</strong> {interview.candidateEmail || 'Not available'}</p>
                    <p><strong>Job Role:</strong> {interview.jobTitle || 'Not available'}</p>
                    <p><strong>Experience:</strong> {interview.experienceYears ?? 'Not available'} years</p>
                    <p><strong>Skills:</strong> {interview.candidateSkills || 'Not available'}</p>
                </section>

                <section className="details-card">
                    <h2>Interview Information</h2>
                    <p><strong>Type:</strong> {interview.interviewType}</p>
                    <p><strong>Round:</strong> {interview.interviewRound}</p>
                    <p><strong>Date & Time:</strong> {formatDateTime(interview.scheduledDate)}</p>
                    <p><strong>Duration:</strong> {interview.duration} minutes</p>
                    <p><strong>Status:</strong> {interview.status || 'Pending'}</p>
                    <p><strong>Location:</strong> {interview.location || 'N/A'}</p>
                    <p><strong>Meeting Link:</strong> {interview.meetingLink || 'N/A'}</p>
                </section>
            </div>

            <section className="details-card full-width">
                <h2>Candidate Summary</h2>
                <p>{interview.candidateSummary || 'No candidate summary available.'}</p>
            </section>

            <section className="details-card full-width">
                <h2>Evaluation Form</h2>

                <div className="score-grid">
                    <div>
                        <label>Interviewer ID</label>
                        <input
                            name="interviewerId"
                            type="number"
                            value={evaluation.interviewerId}
                            onChange={handleEvaluationChange}
                            placeholder="Enter interviewer ID"
                        />
                    </div>

                    <div>
                        <label>Technical Score</label>
                        <input
                            name="technicalScore"
                            type="number"
                            value={evaluation.technicalScore}
                            onChange={handleEvaluationChange}
                            placeholder="0 - 100"
                        />
                    </div>

                    <div>
                        <label>Communication Score</label>
                        <input
                            name="communicationScore"
                            type="number"
                            value={evaluation.communicationScore}
                            onChange={handleEvaluationChange}
                            placeholder="0 - 100"
                        />
                    </div>

                    <div>
                        <label>Problem Solving Score</label>
                        <input
                            name="problemSolvingScore"
                            type="number"
                            value={evaluation.problemSolvingScore}
                            onChange={handleEvaluationChange}
                            placeholder="0 - 100"
                        />
                    </div>

                    <div>
                        <label>Culture Fit Score</label>
                        <input
                            name="cultureFitScore"
                            type="number"
                            value={evaluation.cultureFitScore}
                            onChange={handleEvaluationChange}
                            placeholder="0 - 100"
                        />
                    </div>

                    <div>
                        <label>Overall Score</label>
                        <input
                            type="number"
                            value={calculateOverallScore()}
                            readOnly
                        />
                    </div>

                    <div>
                        <label>Recommendation</label>
                        <select
                            name="recommendation"
                            value={evaluation.recommendation}
                            onChange={handleEvaluationChange}
                        >
                            <option>Strong Hire</option>
                            <option>Hire</option>
                            <option>Next Round</option>
                            <option>Hold</option>
                            <option>Reject</option>
                        </select>
                    </div>
                </div>

                <label>Interview Notes / Final Comments</label>
                <textarea
                    name="comments"
                    value={evaluation.comments}
                    onChange={handleEvaluationChange}
                    placeholder="Enter final comments"
                ></textarea>

                <button
                    className="submit-button"
                    type="button"
                    onClick={handleSubmitEvaluation}
                >
                    Submit Evaluation
                </button>

                {isSubmitted && (
                    <div className="success-message">
                        Evaluation submitted successfully.
                    </div>
                )}
            </section>
        </div>
    );
}

export default InterviewDetails;