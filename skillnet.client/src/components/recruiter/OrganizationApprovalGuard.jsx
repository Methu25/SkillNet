import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useRecruiter } from '../../context/RecruiterContext';

const OrganizationApprovalGuard = () => {
    const { organization, approvalStatus, loading, error, refreshOrganization } = useRecruiter();
    const location = useLocation();

    if (loading) {
        return <div className="recruiter-route-state"><span className="recruiter-spinner" />Checking organization access...</div>;
    }

    if (error) {
        return (
            <div className="recruiter-route-state recruiter-route-state--error">
                <strong>We could not verify your organization.</strong>
                <span>{error}</span>
                <button type="button" onClick={refreshOrganization}>Try again</button>
            </div>
        );
    }

    if (!organization || approvalStatus === 'Draft' || approvalStatus === 'Rejected') {
        return <Navigate to="/recruiter/setup" replace state={{ from: location.pathname }} />;
    }

    if (approvalStatus === 'Pending') {
        return <Navigate to="/recruiter/pending" replace state={{ from: location.pathname }} />;
    }

    if (approvalStatus !== 'Approved') {
        return <Navigate to="/recruiter/setup" replace />;
    }

    return <Outlet />;
};

export default OrganizationApprovalGuard;
