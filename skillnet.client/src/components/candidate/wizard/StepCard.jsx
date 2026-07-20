const StepCard = ({ title, description, children }) => (
    <section className="wizard-step-card">
        <div className="wizard-step-card__heading">
            <h2>{title}</h2>
            <p>{description}</p>
        </div>
        {children}
    </section>
);

export default StepCard;

