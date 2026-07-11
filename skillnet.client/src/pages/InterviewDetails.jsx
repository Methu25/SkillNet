import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import './InterviewDetails.css';

function InterviewDetails() {
    const { id } = useParams();
    const [isSubmitted, setIsSubmitted] = useState(false);

    const interviewDetails = {
        1: {
            interviewId: 1,
            candidateName: 'Nimal Perera',
            email: 'nimal.perera@gmail.com',
            phone: '+94 77 123 4567',
            jobTitle: 'Software Engineer Intern',
            interviewType: 'Technical',
            interviewRound: 1,
            scheduledDate: 'Jul 12, 2026, 10:00 AM',
            duration: 60,
            location: 'Online',
            meetingLink: 'https://meet.google.com/skillnet-demo',
            status: 'Scheduled',
            resumeSummary: 'Computer Science undergraduate with React, C#, SQL Server, and REST API project experience.',
            applicationSummary: 'Shortlisted for Software Engineer Intern role based on academic projects and full-stack development skills.'
        },
        2: {
            interviewId: 2,
            candidateName: 'Kavindi Silva',
            email: 'kavindi.silva@gmail.com',
            phone: '+94 76 456 7890',
            jobTitle: 'Frontend Developer Intern',
            interviewType: 'HR',
            interviewRound: 1,
            scheduledDate: 'Jul 12, 2026, 2:00 PM',
            duration: 45,
            location: 'Online',
            meetingLink: 'https://meet.google.com/frontend-demo',
            status: 'Confirmed',
            resumeSummary: 'Frontend-focused candidate with React, JavaScript, HTML, CSS, and UI design experience.',
            applicationSummary: 'Shortlisted for Frontend Developer Intern role due to strong UI project portfolio.'
        },
        3: {
            interviewId: 3,
            candidateName: 'Avishka Fernando',
            email: 'avishka.fernando@gmail.com',
            phone: '+94 71 234 5678',
            jobTitle: 'Backend Developer Intern',
            interviewType: 'Technical',
            interviewRound: 2,
            scheduledDate: 'Jul 13, 2026, 9:30 AM',
            duration: 60,
            location: 'Meeting Room 2',
            meetingLink: 'N/A',
            status: 'Scheduled',
            resumeSummary: 'Backend-focused candidate with C#, ASP.NET Web API, SQL Server, and database design knowledge.',
            applicationSummary: 'Selected for second technical round after showing good backend development understanding.'
        },
        4: {
            interviewId: 4,
            candidateName: 'Dineth Jayawardena',
            email: 'dineth.jayawardena@gmail.com',
            phone: '+94 75 987 6543',
            jobTitle: 'QA Intern',
            interviewType: 'Technical',
            interviewRound: 2,
            scheduledDate: 'Jul 10, 2026, 11:00 AM',
            duration: 60,
            location: 'Online',
            meetingLink: 'https://meet.google.com/qa-demo',
            status: 'Completed',
            resumeSummary: 'Candidate has knowledge of manual testing, test cases, bug reporting, and basic automation testing.',
            applicationSummary: 'Moved to technical interview after passing initial screening for QA internship.'
        },
        5: {
            interviewId: 5,
            candidateName: 'Sandali Perera',
            email: 'sandali.perera@gmail.com',
            phone: '+94 77 555 8899',
            jobTitle: 'UI/UX Intern',
            interviewType: 'Managerial',
            interviewRound: 3,
            scheduledDate: 'Jul 9, 2026, 3:00 PM',
            duration: 60,
            location: 'Meeting Room 1',
            meetingLink: 'N/A',
            status: 'Evaluation Submitted',
            resumeSummary: 'UI/UX candidate with wireframing, Figma, user research, and interface design experience.',
            applicationSummary: 'Reached managerial round after strong design portfolio review and technical discussion.'
        },
        6: {
            interviewId: 6,
            candidateName: 'Ravindu Silva',
            email: 'ravindu.silva@gmail.com',
            phone: '+94 70 321 4567',
            jobTitle: 'Full Stack Intern',
            interviewType: 'HR',
            interviewRound: 1,
            scheduledDate: 'Jul 14, 2026, 1:30 PM',
            duration: 45,
            location: 'Online',
            meetingLink: 'https://meet.google.com/fullstack-demo',
            status: 'Pending Feedback',
            resumeSummary: 'Full-stack candidate with React, Node.js, ASP.NET, SQL Server, and API integration experience.',
            applicationSummary: 'Shortlisted for Full Stack Intern role based on full-stack project experience.'
        }
    };

    const interview = interviewDetails[id] || interviewDetails[1];

    const assignedInterviewers = [
        {
            interviewerId: 1,
            name: 'Kasun Jayasinghe',
            position: 'Senior Software Engineer',
            role: 'Lead Interviewer'
        },
        {
            interviewerId: 2,
            name: 'Tharushi Fernando',
            position: 'HR Executive',
            role: 'Panel Member'
        },
        {
            interviewerId: 3,
            name: 'Ravindu Perera',
            position: 'Technical Lead',
            role: 'Observer'
        }
    ];

    const interviewAgenda = [
        'Candidate introduction',
        'Project and experience discussion',
        'Technical questions',
        'Problem-solving scenario',
        'Candidate questions',
        'Final recommendation'
    ];

    const evaluation = {
        technicalScore: 85,
        communicationScore: 78,
        problemSolvingScore: 82,
        cultureFitScore: 80,
        overallScore: 81,
        recommendation: 'Hire',
        comments: 'Candidate shows good technical understanding and clear communication. Suitable for internship role.'
    };

    const handleSubmitEvaluation = () => {
        const submittedEvaluation = {
            interviewId: interview.interviewId,
            candidateName: interview.candidateName,
            technicalScore: evaluation.technicalScore,
            communicationScore: evaluation.communicationScore,
            problemSolvingScore: evaluation.problemSolvingScore,
            cultureFitScore: evaluation.cultureFitScore,
            overallScore: evaluation.overallScore,
            recommendation: evaluation.recommendation,
            comments: evaluation.comments,
            submittedAt: new Date().toLocaleString()
        };

        const existingEvaluations =
            JSON.parse(localStorage.getItem('submittedEvaluations')) || [];

        existingEvaluations.push(submittedEvaluation);

        localStorage.setItem(
            'submittedEvaluations',
            JSON.stringify(existingEvaluations)
        );

        setIsSubmitted(true);
    };

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
                    <p><strong>Name:</strong> {interview.candidateName}</p>
                    <p><strong>Email:</strong> {interview.email}</p>
                    <p><strong>Phone:</strong> {interview.phone}</p>
                    <p><strong>Job Role:</strong> {interview.jobTitle}</p>
                </section>

                <section className="details-card">
                    <h2>Interview Information</h2>
                    <p><strong>Type:</strong> {interview.interviewType}</p>
                    <p><strong>Round:</strong> {interview.interviewRound}</p>
                    <p><strong>Date & Time:</strong> {interview.scheduledDate}</p>
                    <p><strong>Duration:</strong> {interview.duration} minutes</p>
                    <p><strong>Status:</strong> {interview.status}</p>
                    <p><strong>Location:</strong> {interview.location}</p>
                    <p><strong>Meeting Link:</strong> {interview.meetingLink}</p>
                </section>
            </div>

            <section className="details-card full-width">
                <h2>Resume Summary</h2>
                <p>{interview.resumeSummary}</p>
            </section>

            <section className="details-card full-width">
                <h2>Application Summary</h2>
                <p>{interview.applicationSummary}</p>
            </section>

            <section className="details-card full-width">
                <h2>Assigned Interviewers</h2>

                <div className="interviewer-grid">
                    {assignedInterviewers.map((interviewer) => (
                        <div className="interviewer-card" key={interviewer.interviewerId}>
                            <h3>{interviewer.name}</h3>
                            <p>{interviewer.position}</p>
                            <span>{interviewer.role}</span>
                        </div>
                    ))}
                </div>
            </section>

            <section className="details-card full-width">
                <h2>Interview Agenda</h2>

                <ol className="agenda-list">
                    {interviewAgenda.map((item, index) => (
                        <li key={index}>{item}</li>
                    ))}
                </ol>
            </section>

            <section className="details-card full-width">
                <h2>Evaluation Form</h2>

                <div className="score-grid">
                    <div>
                        <label>Technical Score</label>
                        <input type="number" defaultValue={evaluation.technicalScore} />
                    </div>

                    <div>
                        <label>Communication Score</label>
                        <input type="number" defaultValue={evaluation.communicationScore} />
                    </div>

                    <div>
                        <label>Problem Solving Score</label>
                        <input type="number" defaultValue={evaluation.problemSolvingScore} />
                    </div>

                    <div>
                        <label>Culture Fit Score</label>
                        <input type="number" defaultValue={evaluation.cultureFitScore} />
                    </div>

                    <div>
                        <label>Overall Score</label>
                        <input type="number" defaultValue={evaluation.overallScore} readOnly />
                    </div>

                    <div>
                        <label>Recommendation</label>
                        <select defaultValue={evaluation.recommendation}>
                            <option>Strong Hire</option>
                            <option>Hire</option>
                            <option>Next Round</option>
                            <option>Hold</option>
                            <option>Reject</option>
                        </select>
                    </div>
                </div>

                <label>Interview Notes / Final Comments</label>
                <textarea defaultValue={evaluation.comments}></textarea>

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