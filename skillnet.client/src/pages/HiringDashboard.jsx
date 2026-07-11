import { useState } from 'react';
import { Link } from 'react-router-dom';
import './HiringDashboard.css';

function HiringDashboard() {
    const [activeTab, setActiveTab] = useState('All Interviews');
    const [searchTerm, setSearchTerm] = useState('');
    const [showCreateModal, setShowCreateModal] = useState(false);

    const interviews = [
        {
            interviewId: 1,
            applicationId: 1001,
            candidateName: 'Nimal Perera',
            jobTitle: 'Software Engineer Intern',
            interviewType: 'Technical',
            interviewRound: 1,
            scheduledDate: 'Jul 12, 2026, 10:00 AM',
            duration: 60,
            status: 'Scheduled'
        },
        {
            interviewId: 2,
            applicationId: 1002,
            candidateName: 'Kavindi Silva',
            jobTitle: 'Frontend Developer Intern',
            interviewType: 'HR',
            interviewRound: 1,
            scheduledDate: 'Jul 12, 2026, 2:00 PM',
            duration: 45,
            status: 'Confirmed'
        },
        {
            interviewId: 3,
            applicationId: 1003,
            candidateName: 'Avishka Fernando',
            jobTitle: 'Backend Developer Intern',
            interviewType: 'Technical',
            interviewRound: 2,
            scheduledDate: 'Jul 13, 2026, 9:30 AM',
            duration: 60,
            status: 'Scheduled'
        },
        {
            interviewId: 4,
            applicationId: 1004,
            candidateName: 'Dineth Jayawardena',
            jobTitle: 'QA Intern',
            interviewType: 'Technical',
            interviewRound: 2,
            scheduledDate: 'Jul 10, 2026, 11:00 AM',
            duration: 60,
            status: 'Completed'
        },
        {
            interviewId: 5,
            applicationId: 1005,
            candidateName: 'Sandali Perera',
            jobTitle: 'UI/UX Intern',
            interviewType: 'Managerial',
            interviewRound: 3,
            scheduledDate: 'Jul 9, 2026, 3:00 PM',
            duration: 60,
            status: 'Evaluation Submitted'
        },
        {
            interviewId: 6,
            applicationId: 1006,
            candidateName: 'Ravindu Silva',
            jobTitle: 'Full Stack Intern',
            interviewType: 'HR',
            interviewRound: 1,
            scheduledDate: 'Jul 14, 2026, 1:30 PM',
            duration: 45,
            status: 'Pending Feedback'
        }
    ];

    const dashboard = {
        todaysInterviews: interviews.filter((interview) =>
            interview.scheduledDate.includes('Jul 12')
        ).length,

        upcomingInterviews: interviews.filter((interview) =>
            interview.status === 'Scheduled' || interview.status === 'Confirmed'
        ).length,

        candidateEvaluations: interviews.filter((interview) =>
            interview.status === 'Evaluation Submitted'
        ).length,

        pendingFeedback: interviews.filter((interview) =>
            interview.status === 'Pending Feedback'
        ).length,

        completedInterviews: interviews.filter((interview) =>
            interview.status === 'Completed' || interview.status === 'Evaluation Submitted'
        ).length,

        totalInterviews: interviews.length
    };

    const tabs = [
        'All Interviews',
        "Today's Interviews",
        'Upcoming Interviews',
        'Candidate Evaluations',
        'Pending Feedback',
        'Completed Interviews'
    ];

    const tabFilteredInterviews = interviews.filter((interview) => {
        if (activeTab === 'All Interviews') {
            return true;
        }

        if (activeTab === "Today's Interviews") {
            return interview.scheduledDate.includes('Jul 12');
        }

        if (activeTab === 'Upcoming Interviews') {
            return interview.status === 'Scheduled' || interview.status === 'Confirmed';
        }

        if (activeTab === 'Candidate Evaluations') {
            return interview.status === 'Evaluation Submitted';
        }

        if (activeTab === 'Pending Feedback') {
            return interview.status === 'Pending Feedback';
        }

        if (activeTab === 'Completed Interviews') {
            return interview.status === 'Completed' || interview.status === 'Evaluation Submitted';
        }

        return true;
    });

    const searchResults = interviews.filter((interview) => {
        const searchText = searchTerm.toLowerCase();

        return (
            interview.interviewId.toString().includes(searchText) ||
            interview.applicationId.toString().includes(searchText) ||
            interview.candidateName.toLowerCase().includes(searchText) ||
            interview.jobTitle.toLowerCase().includes(searchText) ||
            interview.interviewType.toLowerCase().includes(searchText) ||
            interview.status.toLowerCase().includes(searchText)
        );
    });

    const filteredInterviews =
        searchTerm.trim() === '' ? tabFilteredInterviews : searchResults;

    return (
        <div className="hiring-dashboard">
            <div className="dashboard-header">
                <div>
                    <p className="module-label">Evaluation & Decision Module</p>
                    <h1>Hiring Manager Dashboard</h1>
                    <p>
                        Schedule interviews, review candidate sessions, and submit structured hiring recommendations.
                    </p>
                </div>

                <button
                    className="create-interview-button"
                    type="button"
                    onClick={() => setShowCreateModal(true)}
                >
                    + Create Interview
                </button>
            </div>

            <div className="stats-grid">
                <div className="stat-card">
                    <h3>Today&apos;s Interviews</h3>
                    <p>{dashboard.todaysInterviews}</p>
                </div>

                <div className="stat-card">
                    <h3>Upcoming Interviews</h3>
                    <p>{dashboard.upcomingInterviews}</p>
                </div>

                <div className="stat-card">
                    <h3>Candidate Evaluations</h3>
                    <p>{dashboard.candidateEvaluations}</p>
                </div>

                <div className="stat-card">
                    <h3>Pending Feedback</h3>
                    <p>{dashboard.pendingFeedback}</p>
                </div>

                <div className="stat-card">
                    <h3>Completed Interviews</h3>
                    <p>{dashboard.completedInterviews}</p>
                </div>

                <div className="stat-card">
                    <h3>Total Interviews</h3>
                    <p>{dashboard.totalInterviews}</p>
                </div>
            </div>

            <section className="workspace-card">
                <div className="workspace-top">
                    <div>
                        <p className="section-label">Interview Management</p>
                        <h2>Interview Workspace</h2>
                        <p>{filteredInterviews.length} interviews currently available.</p>
                    </div>

                    <div className="search-wrapper">
                        <input
                            className="search-box"
                            type="text"
                            placeholder="Search ID, type, status or candidate"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />

                        {searchTerm.trim() !== '' && (
                            <div className="search-popup">
                                <h4>Search Results</h4>

                                {searchResults.length > 0 ? (
                                    searchResults.map((interview) => (
                                        <Link
                                            to={`/interviews/${interview.interviewId}`}
                                            className="search-result-item"
                                            key={interview.interviewId}
                                        >
                                            <strong>
                                                #{interview.interviewId} - {interview.candidateName}
                                            </strong>
                                            <span>{interview.jobTitle}</span>
                                            <small>
                                                {interview.interviewType} • {interview.status}
                                            </small>
                                        </Link>
                                    ))
                                ) : (
                                    <p className="no-search-result">
                                        No matching interviews found.
                                    </p>
                                )}
                            </div>
                        )}
                    </div>
                </div>

                <div className="workspace-tabs">
                    {tabs.map((tab) => (
                        <button
                            key={tab}
                            type="button"
                            className={`tab-button ${activeTab === tab ? 'active' : ''}`}
                            onClick={() => setActiveTab(tab)}
                        >
                            {tab}
                        </button>
                    ))}
                </div>

                <table className="workspace-table">
                    <thead>
                        <tr>
                            <th>Interview</th>
                            <th>Candidate</th>
                            <th>Application</th>
                            <th>Round & Type</th>
                            <th>Schedule</th>
                            <th>Status</th>
                            <th>Action</th>
                        </tr>
                    </thead>

                    <tbody>
                        {filteredInterviews.map((interview) => (
                            <tr key={interview.interviewId}>
                                <td>#{interview.interviewId}</td>

                                <td>
                                    <strong>{interview.candidateName}</strong>
                                    <span>{interview.jobTitle}</span>
                                </td>

                                <td>Application #{interview.applicationId}</td>

                                <td>
                                    <strong>Round {interview.interviewRound}</strong>
                                    <span>{interview.interviewType}</span>
                                </td>

                                <td>
                                    <strong>{interview.scheduledDate}</strong>
                                    <span>{interview.duration} minutes</span>
                                </td>

                                <td>
                                    <span className="workspace-status">
                                        {interview.status}
                                    </span>
                                </td>

                                <td>
                                    <Link
                                        className="open-link"
                                        to={`/interviews/${interview.interviewId}`}
                                    >
                                        Open interview →
                                    </Link>
                                </td>
                            </tr>
                        ))}

                        {filteredInterviews.length === 0 && (
                            <tr>
                                <td colSpan="7" className="empty-row">
                                    No interviews found for {activeTab}.
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </section>

            {showCreateModal && (
                <div className="modal-overlay">
                    <div className="create-modal">
                        <div className="modal-header">
                            <h2>Create Interview</h2>
                            <button
                                type="button"
                                className="close-button"
                                onClick={() => setShowCreateModal(false)}
                            >
                                ×
                            </button>
                        </div>

                        <div className="modal-form">
                            <label>Candidate Name</label>
                            <input type="text" placeholder="Enter candidate name" />

                            <label>Application ID</label>
                            <input type="number" placeholder="Enter application ID" />

                            <label>Interview Type</label>
                            <select>
                                <option>Technical</option>
                                <option>HR</option>
                                <option>Managerial</option>
                            </select>

                            <label>Interview Round</label>
                            <input type="number" placeholder="Enter round number" />

                            <label>Date & Time</label>
                            <input type="datetime-local" />

                            <label>Duration</label>
                            <input type="number" placeholder="Duration in minutes" />

                            <label>Status</label>
                            <select>
                                <option>Scheduled</option>
                                <option>Confirmed</option>
                                <option>Completed</option>
                                <option>Pending Feedback</option>
                            </select>
                        </div>

                        <div className="modal-actions">
                            <button
                                type="button"
                                className="cancel-button"
                                onClick={() => setShowCreateModal(false)}
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                className="save-button"
                                onClick={() => setShowCreateModal(false)}
                            >
                                Save Interview
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default HiringDashboard;