const RecruiterPlaceholderPage = ({ eyebrow, title, description, action, children }) => (
    <section className="recruiter-page">
        <div className="recruiter-page-heading">
            <div><span className="recruiter-eyebrow">{eyebrow}</span><h2>{title}</h2><p>{description}</p></div>
            {action}
        </div>
        <div className="recruiter-placeholder-card">
            <span className="recruiter-placeholder-mark">SN</span>
            <h3>Foundation ready</h3>
            <p>{children || 'Page functionality will be added in the next implementation phase.'}</p>
        </div>
    </section>
);

export default RecruiterPlaceholderPage;
