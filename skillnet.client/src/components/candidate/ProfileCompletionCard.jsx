import { useNavigate } from 'react-router-dom';
import DashboardCard from './DashboardCard';

const getCompletionTone = (percentage) => {
    if (percentage <= 25) return 'red';
    if (percentage <= 50) return 'orange';
    if (percentage <= 75) return 'yellow';
    return 'green';
};

const ProfileCompletionCard = ({ completion, showAction = true }) => {
    const navigate = useNavigate();
    const percentage = Math.min(100, Math.max(0, completion?.completionPercentage || 0));
    const level = completion?.completionLevel || 0;
    const completedSections = completion?.completedSections || [];
    const missingSections = completion?.missingSections || [];
    const tone = getCompletionTone(percentage);

    return (
        <DashboardCard className={`completion-card completion-card--${tone}`}>
            <div className="completion-card__topline">
                <div>
                    <span className="candidate-eyebrow">Profile strength</span>
                    <h2>Complete your professional story</h2>
                </div>
                <div className="completion-score" aria-label={`${percentage}% complete`}>
                    <strong>{percentage}%</strong>
                    <span>Level {level}</span>
                </div>
            </div>

            <div
                className="completion-progress"
                role="progressbar"
                aria-valuemin="0"
                aria-valuemax="100"
                aria-valuenow={percentage}
            >
                <span style={{ width: `${percentage}%` }} />
            </div>

            <div className="completion-sections">
                <div>
                    <h3>Completed</h3>
                    <div className="section-chips">
                        {completedSections.length > 0
                            ? completedSections.map(section => (
                                <span className="section-chip section-chip--complete" key={section}>✓ {section}</span>
                            ))
                            : <span className="muted-copy">No sections completed yet.</span>}
                    </div>
                </div>
                <div>
                    <h3>Still to add</h3>
                    <div className="section-chips">
                        {missingSections.length > 0
                            ? missingSections.map(section => (
                                <span className="section-chip" key={section}>{section}</span>
                            ))
                            : <span className="muted-copy">Everything is ready.</span>}
                    </div>
                </div>
            </div>

            {showAction && !completion?.isComplete && (
                <button className="candidate-button candidate-button--primary" onClick={() => navigate('/candidate/profile')}>
                    Complete Profile
                </button>
            )}
        </DashboardCard>
    );
};

export default ProfileCompletionCard;
