const DashboardCard = ({ title, action, className = '', children }) => (
    <section className={`candidate-card ${className}`.trim()}>
        {(title || action) && (
            <div className="candidate-card__heading">
                {title && <h2>{title}</h2>}
                {action}
            </div>
        )}
        {children}
    </section>
);

export default DashboardCard;

