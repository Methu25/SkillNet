import { useParams } from 'react-router-dom';
import RecruiterPlaceholderPage from '../../components/recruiter/RecruiterPlaceholderPage';

const EditJob = () => {
    const { id } = useParams();
    return <RecruiterPlaceholderPage eyebrow={`Job #${id}`} title="Edit job" description="Update the job information, requirements, skills, and deadline." />;
};

export default EditJob;
