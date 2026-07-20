import ProtectedRoute from '../ProtectedRoute';

const RecruiterRoute = ({ children }) => (
    <ProtectedRoute allowedRoles={['Recruiter']}>
        {children}
    </ProtectedRoute>
);

export default RecruiterRoute;
