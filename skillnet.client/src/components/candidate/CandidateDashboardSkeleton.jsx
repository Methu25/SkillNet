const CandidateDashboardSkeleton = () => (
    <div className="candidate-dashboard candidate-dashboard--loading" aria-label="Loading candidate dashboard">
        <div className="skeleton skeleton--header" />
        <div className="skeleton skeleton--wide" />
        <div className="candidate-grid">
            <div className="skeleton skeleton--card" />
            <div className="skeleton skeleton--card" />
            <div className="skeleton skeleton--card" />
            <div className="skeleton skeleton--card" />
        </div>
    </div>
);

export default CandidateDashboardSkeleton;

