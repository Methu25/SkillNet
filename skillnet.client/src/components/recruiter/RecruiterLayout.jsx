import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { RecruiterProvider } from '../../context/RecruiterContext';
import RecruiterHeader from './RecruiterHeader';
import RecruiterSidebar from './RecruiterSidebar';
import '../../styles/recruiter.css';

const RecruiterLayout = () => {
    const [navigationOpen, setNavigationOpen] = useState(false);

    return (
        <RecruiterProvider>
            <div className="recruiter-shell">
                <RecruiterSidebar open={navigationOpen} onClose={() => setNavigationOpen(false)} />
                <div className="recruiter-workspace">
                    <RecruiterHeader onMenu={() => setNavigationOpen(true)} />
                    <main className="recruiter-main"><Outlet /></main>
                </div>
            </div>
        </RecruiterProvider>
    );
};

export default RecruiterLayout;
