const ProfileFormSection = ({ title, description, children }) => (
    <section className="profile-form-section">
        <header>
            <h2>{title}</h2>
            <p>{description}</p>
        </header>
        <div className="profile-form-fields">{children}</div>
    </section>
);

export default ProfileFormSection;
