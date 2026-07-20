import { Link } from 'react-router-dom';
import RecruiterPlaceholderPage from '../../components/recruiter/RecruiterPlaceholderPage';

const RecruiterDashboard = () => (
    <RecruiterPlaceholderPage
        eyebrow="Overview"
        title="Recruiter dashboard"
        description="A clear view of your hiring workspace and active job activity."
        action={<Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create job</Link>}
    />
);

export default RecruiterDashboard;
