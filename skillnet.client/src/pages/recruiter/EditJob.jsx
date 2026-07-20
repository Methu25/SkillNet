import { useParams } from 'react-router-dom';
import RecruiterJobForm from '../../components/recruiter/RecruiterJobForm';

const EditJob = () => {
    const { id } = useParams();
    return <RecruiterJobForm jobId={Number(id)} />;
};

export default EditJob;
