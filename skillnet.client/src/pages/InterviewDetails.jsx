import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';

export default function InterviewDetails() {
  const { id } = useParams();
  const [interview, setInterview] = useState(null);
  const [evaluation, setEvaluation] = useState({
    technicalScore: 0,
    communicationScore: 0,
    problemSolvingScore: 0,
    cultureFitScore: 0,
    recommendation: 'Next Round',
    comments: '',
    interviewerId: 1 // Default or from context
  });
  const [hasExistingEval, setHasExistingEval] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchData() {
      try {
        const intRes = await fetch(`/api/interviews/${id}`);
        if (intRes.ok) setInterview(await intRes.json());

        const evalRes = await fetch(`/api/interviews/${id}/evaluation`);
        if (evalRes.ok) {
          const evalData = await evalRes.json();
          setEvaluation(evalData);
          setHasExistingEval(true);
        }
      } catch (err) {
        console.error("Error fetching details", err);
      } finally {
        setLoading(false);
      }
    }
    fetchData();
  }, [id]);

  const handleEvalChange = (e) => {
    const { name, value } = e.target;
    setEvaluation(prev => ({ ...prev, [name]: value }));
  };

  const submitEvaluation = async (e) => {
    e.preventDefault();
    try {
      const method = hasExistingEval ? 'PUT' : 'POST';
      const res = await fetch(`/api/interviews/${id}/evaluation`, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...evaluation,
          technicalScore: parseInt(evaluation.technicalScore) || 0,
          communicationScore: parseInt(evaluation.communicationScore) || 0,
          problemSolvingScore: parseInt(evaluation.problemSolvingScore) || 0,
          cultureFitScore: parseInt(evaluation.cultureFitScore) || 0,
          interviewerId: evaluation.interviewerId || 1 // Placeholder for auth user id
        })
      });

      if (res.ok) {
        alert("Evaluation submitted successfully!");
        setHasExistingEval(true);
      } else {
        alert("Failed to submit");
      }
    } catch (err) {
      console.error(err);
      alert("Error submitting");
    }
  };

  if (loading) return <p>Loading details...</p>;
  if (!interview) return <p>Interview not found.</p>;

  return (
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      <Link to="/">&larr; Back to Dashboard</Link>

      <h1 style={{ marginTop: '20px' }}>Interview #{interview.interviewId}</h1>
      <div style={{ border: '1px solid #ccc', padding: '15px', marginBottom: '20px', borderRadius: '5px' }}>
        <p><strong>App ID:</strong> {interview.applicationId}</p>
        <p><strong>Type:</strong> {interview.interviewType} (Round {interview.interviewRound})</p>
        <p><strong>Scheduled:</strong> {new Date(interview.scheduledDate).toLocaleString()}</p>
        <p><strong>Status:</strong> {interview.status}</p>
        <p><strong>Location/Link:</strong> {interview.meetingLink || interview.location}</p>
      </div>

      <h2>Candidate Evaluation Form</h2>
      <form onSubmit={submitEvaluation} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        <div>
          <label>Technical Score (1-100): </label>
          <input type="number" name="technicalScore" value={evaluation.technicalScore} onChange={handleEvalChange} required />
        </div>
        <div>
          <label>Communication Score (1-100): </label>
          <input type="number" name="communicationScore" value={evaluation.communicationScore} onChange={handleEvalChange} required />
        </div>
        <div>
          <label>Problem Solving Score (1-100): </label>
          <input type="number" name="problemSolvingScore" value={evaluation.problemSolvingScore} onChange={handleEvalChange} required />
        </div>
        <div>
          <label>Culture Fit Score (1-100): </label>
          <input type="number" name="cultureFitScore" value={evaluation.cultureFitScore} onChange={handleEvalChange} required />
        </div>

        <div>
          <label>Recommendation: </label>
          <select name="recommendation" value={evaluation.recommendation || ''} onChange={handleEvalChange} required>
            <option value="Strong Hire">Strong Hire</option>
            <option value="Hire">Hire</option>
            <option value="Next Round">Next Round</option>
            <option value="Hold">Hold</option>
            <option value="Reject">Reject</option>
          </select>
        </div>

        <div>
          <label>Comments: </label><br/>
          <textarea 
            name="comments" 
            value={evaluation.comments || ''} 
            onChange={handleEvalChange} 
            rows="4" 
            style={{ width: '100%' }}
            required 
          />
        </div>

        <button type="submit" style={{ padding: '10px', background: '#007BFF', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
          {hasExistingEval ? 'Update Evaluation' : 'Submit Evaluation'}
        </button>
      </form>
    </div>
  );
}
