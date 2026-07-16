const WizardLayout = ({ children }) => (
    <div className="profile-wizard-shell">
        <header className="wizard-brandbar">
            <span className="candidate-brand">Skill<span>Net</span></span>
            <span>Candidate onboarding</span>
        </header>
        <main className="profile-wizard">
            <div className="wizard-welcome">
                <span className="candidate-eyebrow">Welcome to SkillNet</span>
                <h1>Create your professional profile</h1>
                <p>Start your career journey with a profile that helps recruiters understand your strengths.</p>
            </div>
            {children}
        </main>
    </div>
);

export default WizardLayout;

