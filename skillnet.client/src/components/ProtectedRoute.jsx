import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const ProtectedRoute = ({ children, allowedRoles }) => {
    const { user, loading } = useAuth();

    if (loading) {
        return (
            <div style={{ textAlign: 'center', marginTop: '100px', fontSize: '18px' }}>
                Loading session...
            </div>
        );
    }

    if (!user) {
        return <Navigate to="/login" replace />;
    }

    if (allowedRoles && allowedRoles.length > 0) {
        const userRoles = user.roles || [];
        const hasRole = allowedRoles.some(role => userRoles.includes(role));
        if (!hasRole) {
            return <Navigate to="/access-denied" replace />;
        }
    }

    return children;
};

export default ProtectedRoute;
