import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ApiError } from '../api/apiClient';
import { recruiterApi } from '../api/recruiterApi';

/* eslint-disable react-refresh/only-export-components */

const RecruiterContext = createContext(null);

export const RecruiterProvider = ({ children }) => {
    const [organization, setOrganization] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const refreshOrganization = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            setOrganization(await recruiterApi.getOrganization());
        } catch (requestError) {
            if (requestError instanceof ApiError && requestError.status === 404) {
                setOrganization(null);
            } else {
                setError(requestError.message || 'Organization status could not be loaded.');
            }
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        // The initial organization request synchronizes route access with the backend.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        refreshOrganization();
    }, [refreshOrganization]);

    const value = useMemo(() => ({
        organization,
        approvalStatus: organization?.approvalStatus || null,
        loading,
        error,
        refreshOrganization,
        setOrganization
    }), [organization, loading, error, refreshOrganization]);

    return <RecruiterContext.Provider value={value}>{children}</RecruiterContext.Provider>;
};

export const useRecruiter = () => {
    const context = useContext(RecruiterContext);
    if (!context) throw new Error('useRecruiter must be used within RecruiterProvider.');
    return context;
};
