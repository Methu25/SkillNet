const WizardProgress = ({ steps, currentStep }) => {
    const percentage = ((currentStep + 1) / steps.length) * 100;

    return (
        <div className="wizard-progress" aria-label={`Step ${currentStep + 1} of ${steps.length}`}>
            <div className="wizard-progress__meta">
                <strong>Step {currentStep + 1} of {steps.length}</strong>
                <span>{steps[currentStep]}</span>
            </div>
            <div className="wizard-progress__track"><span style={{ width: `${percentage}%` }} /></div>
            <ol>
                {steps.map((step, index) => (
                    <li
                        className={index === currentStep ? 'is-active' : index < currentStep ? 'is-complete' : ''}
                        key={step}
                    >
                        <span>{index < currentStep ? '✓' : index + 1}</span>
                        <small>{step}</small>
                    </li>
                ))}
            </ol>
        </div>
    );
};

export default WizardProgress;

