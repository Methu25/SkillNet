const WizardNavigation = ({ canGoBack, isOptional, isFinal, submitting, onBack, onNext, onSkip, onFinish }) => (
    <div className="wizard-navigation">
        <div>
            {canGoBack && <button type="button" className="candidate-button candidate-button--ghost" onClick={onBack}>Previous</button>}
        </div>
        <div>
            {isOptional && !isFinal && <button type="button" className="wizard-skip" onClick={onSkip}>Skip for now</button>}
            {isFinal ? (
                <button type="button" className="candidate-button candidate-button--primary" disabled={submitting} onClick={onFinish}>
                    {submitting ? 'Creating profile…' : 'Create Profile'}
                </button>
            ) : (
                <button type="button" className="candidate-button candidate-button--primary" onClick={onNext}>Next</button>
            )}
        </div>
    </div>
);

export default WizardNavigation;

