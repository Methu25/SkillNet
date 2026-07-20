import { useParams } from 'react-router-dom';
import RecruiterPlaceholderPage from '../../components/recruiter/RecruiterPlaceholderPage';

const JobDetails = () => {
    const { id } = useParams();
    return <RecruiterPlaceholderPage eyebrow={`Job #${id}`} title="Job details" description="Review the complete job post, status, and available management actions." />;
};

export default JobDetails;
