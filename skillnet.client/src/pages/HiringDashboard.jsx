import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import './HiringDashboard.css';

const toDateTimeLocalValue = (value) => {
    if (!value) return '';

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return '';
    }

    const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    return localDate.toISOString().slice(0, 16);
};

const getCurrentDateTimeLocal = () => {
    const now = new Date();
    const localNow = new Date(now.getTime() - now.getTimezoneOffset() * 60000);
    return localNow.toISOString().slice(0, 16);
};

const normalizeDateTimeForApi = (value) => {
    if (!value) return '';
    return value.length === 16 ? `${value}:00` : value;
};

function HiringDashboard() {
    const [activeTab, setActiveTab] = useState('All Interviews');
    const [searchTerm, setSearchTerm] = useState('');

    const [interviews, setInterviews] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const [showModal, setShowModal] = useState(false);
    const [isEditMode, setIsEditMode] = useState(false);
    const [editingInterviewId, setEditingInterviewId] = useState(null);

    const [refreshKey, setRefreshKey] = useState(0);

    const [formData, setFormData] = useState({
        applicationId: '',
        interviewType: 'Technical',
        interviewRound: '',
        scheduledDate: '',
        duration: '',
        location: '',
        meetingLink: '',
        status: 'Scheduled'
    });

    useEffect(() => {
        const loadInterviews = async () => {
            try {
                setLoading(true);
                setError('');

                const response = await fetch('/api/interviews');

                if (!response.ok) {
                    throw new Error('Failed to load interviews from database.');
                }

                const data = await response.json();
                setInterviews(data);
            } catch (err) {
                setError(err.message || 'Something went wrong while loading interviews.');
            } finally {
                setLoading(false);
            }
        };

        loadInterviews();
    }, [refreshKey]);

    const resetForm = () => {
        setFormData({
            applicationId: '',
            interviewType: 'Technical',
            interviewRound: '',
            scheduledDate: '',
            duration: '',
            location: '',
            meetingLink: '',
            status: 'Scheduled'
        });

        setIsEditMode(false);
        setEditingInterviewId(null);
    };

    const openCreateModal = () => {
        resetForm();
        setFormData((previousData) => ({
            ...previousData,
            scheduledDate: getCurrentDateTimeLocal(),
            duration: '45'
        }));
        setShowModal(true);
    };

    const formatDateTime = (dateValue) => {
        if (!dateValue) return 'Not scheduled';

        const date = new Date(dateValue);

        if (Number.isNaN(date.getTime())) {
            return dateValue;
        }

        return date.toLocaleString();
    };

    const isToday = (dateValue) => {
        if (!dateValue) return false;

        const date = new Date(dateValue);
        const today = new Date();

        return (
            date.getFullYear() === today.getFullYear() &&
            date.getMonth() === today.getMonth() &&
            date.getDate() === today.getDate()
        );
    };

    const isUpcoming = (interview) => {
        if (!interview.scheduledDate) return false;

        const scheduledDate = new Date(interview.scheduledDate);
        const now = new Date();

        return (
            scheduledDate >= now &&
            interview.status !== 'Completed' &&
            interview.status !== 'Cancelled'
        );
    };

    const dashboard = {
        todaysInterviews: interviews.filter((interview) =>
            isToday(interview.scheduledDate)
        ).length,

        upcomingInterviews: interviews.filter((interview) =>
            isUpcoming(interview)
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
            return isToday(interview.scheduledDate);
        }

        if (activeTab === 'Upcoming Interviews') {
            return isUpcoming(interview);
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
            interview.interviewId?.toString().includes(searchText) ||
            interview.applicationId?.toString().includes(searchText) ||
            interview.interviewType?.toLowerCase().includes(searchText) ||
            interview.status?.toLowerCase().includes(searchText) ||
            interview.location?.toLowerCase().includes(searchText) ||
            interview.meetingLink?.toLowerCase().includes(searchText)
        );
    });

    const filteredInterviews =
        searchTerm.trim() === '' ? tabFilteredInterviews : searchResults;

    const handleInputChange = (e) => {
        const { name, value } = e.target;

        setFormData({
            ...formData,
            [name]: value
        });
    };

    const handleSubmitInterview = async () => {
        try {
            if (
                !formData.applicationId ||
                !formData.interviewRound ||
                !formData.scheduledDate ||
                !formData.duration
            ) {
                alert('Please fill Application ID, Interview Round, Date & Time, and Duration.');
                return;
            }

            if (Number(formData.applicationId) <= 0) {
                alert('Application ID must be greater than 0.');
                return;
            }

            if (Number(formData.interviewRound) <= 0) {
                alert('Interview Round must be greater than 0.');
                return;
            }

            if (Number(formData.duration) <= 0) {
                alert('Duration must be greater than 0.');
                return;
            }

            const requestBody = {
                applicationId: Number(formData.applicationId),
                interviewType: formData.interviewType,
                interviewRound: Number(formData.interviewRound),
                scheduledDate: normalizeDateTimeForApi(formData.scheduledDate),
                duration: Number(formData.duration),
                location: formData.location,
                meetingLink: formData.meetingLink,
                status: formData.status
            };

            const url = isEditMode
                ? `/api/interviews/${editingInterviewId}`
                : '/api/interviews';

            const method = isEditMode ? 'PUT' : 'POST';

            const response = await fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.log('API Error:', errorText);
                alert(errorText || 'Failed to save interview.');
                return;
            }

            const savedInterview = await response.json();

            if (isEditMode) {
                setInterviews((previousInterviews) =>
                    previousInterviews.map((interview) =>
                        interview.interviewId === editingInterviewId
                            ? savedInterview
                            : interview
                    )
                );
            } else {
                setInterviews((previousInterviews) => [
                    ...previousInterviews,
                    savedInterview
                ]);
            }

            setShowModal(false);
            resetForm();
        } catch (err) {
            console.log('Frontend Error:', err);
            alert(err.message || 'Something went wrong.');
        }
    };

    const handleEditInterview = (interview) => {
        setIsEditMode(true);
        setEditingInterviewId(interview.interviewId);

        setFormData({
            applicationId: interview.applicationId || '',
            interviewType: interview.interviewType || 'Technical',
            interviewRound: interview.interviewRound || '',
            scheduledDate: toDateTimeLocalValue(interview.scheduledDate),
            duration: interview.duration || '',
            location: interview.location || '',
            meetingLink: interview.meetingLink || '',
            status: interview.status || 'Scheduled'
        });

        setShowModal(true);
    };

    const handleDeleteInterview = async (interviewId) => {
        try {
            const confirmDelete = window.confirm('Are you sure you want to delete this interview?');

            if (!confirmDelete) {
                return;
            }

            const response = await fetch(`/api/interviews/${interviewId}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                throw new Error('Failed to delete interview.');
            }

            setRefreshKey((previousValue) => previousValue + 1);
        } catch (err) {
            alert(err.message || 'Something went wrong while deleting.');
        }
    };

    const getStatusClassName = (status) => {
        if (!status) return 'pending';

        return status
            .toLowerCase()
            .replace(/\s+/g, '-');
    };

    if (loading) {
        return (
            <div className="hiring-dashboard">
                <h2>Loading interviews from database...</h2>
            </div>
        );
    }

    if (error) {
        return (
            <div className="hiring-dashboard">
                <h2>Database/API Error</h2>
                <p>{error}</p>

                <button
                    className="create-interview-button"
                    type="button"
                    onClick={() => setRefreshKey((previousValue) => previousValue + 1)}
                >
                    Retry
                </button>
            </div>
        );
    }

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
                    onClick={openCreateModal}
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
                            placeholder="Search ID, type, status or location"
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
                                            <strong>Interview #{interview.interviewId}</strong>
                                            <span>Application #{interview.applicationId}</span>
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
                            <th>Application</th>
                            <th>Round & Type</th>
                            <th>Schedule</th>
                            <th>Location</th>
                            <th>Status</th>
                            <th>Action</th>
                        </tr>
                    </thead>

                    <tbody>
                        {filteredInterviews.map((interview) => (
                            <tr key={interview.interviewId}>
                                <td>#{interview.interviewId}</td>

                                <td>Application #{interview.applicationId}</td>

                                <td>
                                    <strong>Round {interview.interviewRound}</strong>
                                    <span>{interview.interviewType}</span>
                                </td>

                                <td>
                                    <strong>{formatDateTime(interview.scheduledDate)}</strong>
                                    <span>{interview.duration} minutes</span>
                                </td>

                                <td>{interview.location || 'N/A'}</td>

                                <td>
                                    <span
                                        className={`workspace-status ${getStatusClassName(interview.status)}`}
                                    >
                                        {interview.status || 'Pending'}
                                    </span>
                                </td>

                                <td>
                                    <div className="action-buttons">
                                        <Link
                                            className="open-link"
                                            to={`/interviews/${interview.interviewId}`}
                                        >
                                            Open
                                        </Link>

                                        <button
                                            className="edit-button"
                                            type="button"
                                            onClick={() => handleEditInterview(interview)}
                                        >
                                            Edit
                                        </button>

                                        <button
                                            className="delete-button"
                                            type="button"
                                            onClick={() => handleDeleteInterview(interview.interviewId)}
                                        >
                                            Delete
                                        </button>
                                    </div>
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

            {showModal && (
                <div className="modal-overlay">
                    <div className="create-modal">
                        <div className="modal-header">
                            <h2>{isEditMode ? 'Edit Interview' : 'Create Interview'}</h2>

                            <button
                                type="button"
                                className="close-button"
                                onClick={() => {
                                    setShowModal(false);
                                    resetForm();
                                }}
                            >
                                ×
                            </button>
                        </div>

                        <div className="modal-form">
                            <label>Application ID</label>
                            <input
                                name="applicationId"
                                type="number"
                                min="1"
                                placeholder="Enter application ID"
                                value={formData.applicationId}
                                onChange={handleInputChange}
                                required
                            />

                            <label>Interview Type</label>
                            <select
                                name="interviewType"
                                value={formData.interviewType}
                                onChange={handleInputChange}
                                required
                            >
                                <option>Technical</option>
                                <option>HR</option>
                                <option>Managerial</option>
                                <option>System Design</option>
                                <option>Culture Fit</option>
                            </select>

                            <label>Interview Round</label>
                            <input
                                name="interviewRound"
                                type="number"
                                min="1"
                                placeholder="Enter round number"
                                value={formData.interviewRound}
                                onChange={handleInputChange}
                                required
                            />

                            <label>Date & Time</label>
                            <input
                                name="scheduledDate"
                                type="datetime-local"
                                value={formData.scheduledDate}
                                onChange={handleInputChange}
                                min={getCurrentDateTimeLocal()}
                                required
                            />

                            <label>Duration</label>
                            <input
                                name="duration"
                                type="number"
                                min="1"
                                placeholder="Duration in minutes"
                                value={formData.duration}
                                onChange={handleInputChange}
                                required
                            />

                            <label>Location</label>
                            <input
                                name="location"
                                type="text"
                                placeholder="Enter location"
                                value={formData.location}
                                onChange={handleInputChange}
                            />

                            <label>Meeting Link</label>
                            <input
                                name="meetingLink"
                                type="text"
                                placeholder="Enter meeting link"
                                value={formData.meetingLink}
                                onChange={handleInputChange}
                            />

                            <label>Status</label>
                            <select
                                name="status"
                                value={formData.status}
                                onChange={handleInputChange}
                                required
                            >
                                <option>Scheduled</option>
                                <option>Confirmed</option>
                                <option>Completed</option>
                                <option>Pending Feedback</option>
                                <option>Cancelled</option>
                                <option>Evaluation Submitted</option>
                            </select>
                        </div>

                        <div className="modal-actions">
                            <button
                                type="button"
                                className="cancel-button"
                                onClick={() => {
                                    setShowModal(false);
                                    resetForm();
                                }}
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                className="save-button"
                                onClick={handleSubmitInterview}
                            >
                                {isEditMode ? 'Update Interview' : 'Save Interview'}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default HiringDashboard;