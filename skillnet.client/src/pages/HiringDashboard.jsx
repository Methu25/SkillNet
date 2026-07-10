import { useState, useEffect } from 'react';
import DashboardStats from '../components/DashboardStats';
import TodaysInterviews from '../components/TodaysInterviews';
import PendingFeedback from '../components/PendingFeedback';
import UpcomingInterviews from '../components/UpcomingInterviews';
import CompletedInterviews from '../components/CompletedInterviews';
import CandidateEvaluations from '../components/CandidateEvaluations';

export default function HiringDashboard() {
    const [interviews, setInterviews] = useState([]);
    const [stats, setStats] = useState({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // State to track which interview we are currently evaluating
    const [selectedInterview, setSelectedInterview] = useState(null);

    // State to track evaluation form inputs
    const [evaluationForm, setEvaluationForm] = useState({
        technicalScore: '',
        communicationScore: '',
        problemSolvingScore: '',
        cultureFitScore: '',
        recommendation: '',
        comments: ''
    });

    useEffect(() => {
        const fetchDashboardData = async () => {
            try {
                setLoading(true);
                setError(null);

                // Fetch dashboard stats
                const statsResponse = await fetch('/api/hiring/dashboard');
                if (!statsResponse.ok) throw new Error(`Failed to fetch stats: ${statsResponse.status}`);
                const statsData = await statsResponse.json();
                console.log('Dashboard Stats:', statsData);
                setStats(statsData);

                // Fetch today's and completed interviews
                const interviewsResponse = await fetch('/api/hiring/interviews');
                if (!interviewsResponse.ok) throw new Error(`Failed to fetch interviews: ${interviewsResponse.status}`);
                const interviewsData = await interviewsResponse.json();
                console.log('Interviews Data:', interviewsData);

                // Fetch upcoming interviews
                const upcomingResponse = await fetch('/api/hiring/upcoming');
                if (!upcomingResponse.ok) throw new Error(`Failed to fetch upcoming: ${upcomingResponse.status}`);
                const upcomingData = await upcomingResponse.json();
                console.log('Upcoming Interviews Data:', upcomingData);

                // Combine all interviews from both endpoints
                const allInterviews = [...(interviewsData || []), ...(upcomingData || [])];
                setInterviews(allInterviews);

                setLoading(false);
            } catch (error) {
                console.error('Error fetching dashboard data:', error);
                setError(error.message || 'Failed to load dashboard data');
                setLoading(false);
            }
        };

        fetchDashboardData();
    }, []);

    // NEW: Function to handle form submission
    const handleEvaluationSubmit = async (e) => {
        e.preventDefault();

        // Create the JSON payload
        const payload = {
            interviewId: selectedInterview.id,
            candidateName: selectedInterview.candidateName,
            evaluations: {
                technicalScore: parseInt(evaluationForm.technicalScore),
                communicationScore: parseInt(evaluationForm.communicationScore),
                problemSolvingScore: parseInt(evaluationForm.problemSolvingScore),
                cultureFitScore: parseInt(evaluationForm.cultureFitScore)
            },
            recommendation: evaluationForm.recommendation,
            comments: evaluationForm.comments
        };

        // Log the JSON payload
        console.log('Evaluation Submission Payload:', payload);

        try {
            // POST request to backend API
            const response = await fetch(`/api/interviews/${selectedInterview.id}/evaluation`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                const result = await response.json();
                console.log('Backend Response:', result);
                alert("Evaluation submitted successfully! Returning to dashboard.");
                setSelectedInterview(null); // Go back to dashboard

                // Reset form
                setEvaluationForm({
                    technicalScore: '',
                    communicationScore: '',
                    problemSolvingScore: '',
                    cultureFitScore: '',
                    recommendation: '',
                    comments: ''
                });
            } else {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
        } catch (error) {
            console.error('Error submitting evaluation:', error);
            alert(`Error submitting evaluation: ${error.message}`);
        }
    };

    // NEW: Function to handle form input changes
    const handleEvaluationInputChange = (e) => {
        const { name, value } = e.target;
        setEvaluationForm(prev => ({
            ...prev,
            [name]: value
        }));
    };

    if (loading) return <div>Loading Dashboard...</div>;
    if (error) {
        return (
            <div style={{ padding: '20px', color: 'red' }}>
                <h2>Connection Error</h2>
                <p>{error}</p>
                <button onClick={() => window.location.reload()}>Retry</button>
            </div>
        );
    }

    // --- VIEW 1: THE EVALUATION SCREEN ---
    // If an interview is selected, show the CandidateEvaluations component
    if (selectedInterview) {
        return (
            <CandidateEvaluations
                selectedInterview={selectedInterview}
                evaluationForm={evaluationForm}
                onInputChange={handleEvaluationInputChange}
                onSubmit={handleEvaluationSubmit}
                onBack={() => setSelectedInterview(null)}
            />
        );
    }

    // --- VIEW 2: THE MAIN DASHBOARD ---
    return (
        <div style={{ padding: '20px', fontFamily: 'sans-serif' }}>
            <h1>Hiring Manager Dashboard</h1>

            {/* Dashboard Stats */}
            <DashboardStats stats={stats} />

            {/* Two-Column Interview Lists */}
            <div style={{ display: 'flex', gap: '20px' }}>
                <div style={{ flex: 1 }}>
                    <TodaysInterviews 
                        interviews={interviews} 
                        onSelectInterview={setSelectedInterview}
                    />
                </div>

                <div style={{ flex: 1 }}>
                    <PendingFeedback 
                        interviews={interviews} 
                        onSelectInterview={setSelectedInterview}
                    />
                </div>
            </div>

            {/* Additional Interview Lists */}
            <div style={{ display: 'flex', gap: '20px', marginTop: '30px' }}>
                <div style={{ flex: 1 }}>
                    <UpcomingInterviews 
                        interviews={interviews} 
                        onSelectInterview={setSelectedInterview}
                    />
                </div>

                <div style={{ flex: 1 }}>
                    <CompletedInterviews 
                        interviews={interviews} 
                        onSelectInterview={setSelectedInterview}
                    />
                </div>
            </div>
        </div>
    );
}