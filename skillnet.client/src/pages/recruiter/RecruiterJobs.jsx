import { Link } from 'react-router-dom';
import RecruiterPlaceholderPage from '../../components/recruiter/RecruiterPlaceholderPage';

const RecruiterJobs = () => (
    <RecruiterPlaceholderPage eyebrow="Job management" title="Your jobs" description="Create, review, publish, and close your job posts from one place." action={<Link className="recruiter-primary-action" to="/recruiter/jobs/create">Create job</Link>} />
);

export default RecruiterJobs;
