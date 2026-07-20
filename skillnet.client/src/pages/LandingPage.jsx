import React from 'react';
import { Link } from 'react-router-dom';
import './LandingPage.css';

const LandingPage = () => {
    return (
        <div className="landing-body">
            <nav className="landing-nav">
                <Link to="/" className="landing-logo">Skill<span>Net</span>.</Link>
                <div className="landing-nav-links">
                    <a href="#features">Platform</a>
                    <a href="#features">For Candidates</a>
                    <a href="#features">For Employers</a>
                </div>
                <div className="landing-nav-actions">
                    <Link to="/login" className="btn-login">Log In</Link>
                    <Link to="/register" className="btn-primary-orange">Get Started</Link>
                </div>
            </nav>

            <section className="landing-hero">
                <div className="landing-hero-content">
                    <div className="landing-hero-badge">AI-Powered Recruitment</div>
                    <h1>The Future of Hiring is Intelligent.</h1>
                    <p>SkillNet connects top talent with industry-leading roles. Experience seamless resume parsing, intelligent candidate matching, and a streamlined workflow for recruiters and applicants alike.</p>
                    <div className="landing-hero-buttons">
                        <Link to="/register" className="btn-primary-orange">Find a Job</Link>
                        <Link to="/register" className="btn-secondary-orange">Hire Talent</Link>
                    </div>
                </div>

                <div className="landing-hero-visual">
                    <div className="landing-floating-badge">
                        ✨ 98% AI Match Accuracy
                    </div>
                    <div className="landing-mockup-card">
                        <div className="landing-mockup-header">
                            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                                <div className="landing-mockup-avatar"></div>
                                <div>
                                    <div className="landing-mockup-bar" style={{ width: '120px', marginBottom: '5px' }}></div>
                                    <div className="landing-mockup-bar" style={{ width: '80px', height: '8px' }}></div>
                                </div>
                            </div>
                            <div className="btn-primary-orange" style={{ padding: '0.4rem 1rem', fontSize: '0.8rem', cursor: 'default' }}>Matched</div>
                        </div>
                        <div className="landing-mockup-bar orange"></div>
                        <div className="landing-mockup-bar" style={{ width: '90%' }}></div>
                        <div className="landing-mockup-bar" style={{ width: '85%' }}></div>
                        <br />
                        <div className="landing-mockup-bar" style={{ width: '40%', background: 'var(--light-orange)' }}></div>
                    </div>
                </div>
            </section>

            <section id="features" className="landing-features">
                <h2>Built for Modern Talent Acquisition</h2>
                <div className="landing-features-grid">
                    <div className="landing-feature-card">
                        <div className="landing-feature-icon">01</div>
                        <h3>Smart Candidate Profiles</h3>
                        <p>Upload a resume and let our system instantly extract skills, education, and experience to build a comprehensive digital portfolio.</p>
                    </div>
                    <div className="landing-feature-card">
                        <div className="landing-feature-icon">02</div>
                        <h3>AI-Driven Job Matching</h3>
                        <p>Stop manually sifting through applications. Our algorithm ranks candidates based on exact skill gaps and experience requirements.</p>
                    </div>
                    <div className="landing-feature-card">
                        <div className="landing-feature-icon">03</div>
                        <h3>Role-Based Dashboards</h3>
                        <p>Dedicated, secure portals designed specifically for Candidates, Recruiters, Hiring Managers, and System Administrators.</p>
                    </div>
                </div>
            </section>
        </div>
    );
};

export default LandingPage;
