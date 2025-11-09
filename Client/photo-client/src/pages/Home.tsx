import React from 'react'
import { Button } from '@mui/material'
import { useNavigate, Link } from 'react-router-dom'

// Build image URLs from API's static mapping (/extras). Falls back to site root if not set.
const API_BASE = (import.meta as any).env?.VITE_API_URL || ''
const heroImages = [
  `${API_BASE}/extras/MOUNTAINS1.jpg`,
  `${API_BASE}/extras/WINDOW1.jpg`,
  `${API_BASE}/extras/SNOW1.jpg`
]

const featureImages = [
  `${API_BASE}/extras/BIRDS1.jpg`,
  `${API_BASE}/extras/MOUNTAINS2.jpg`,
  `${API_BASE}/extras/WINDOW2.jpg`
]

export default function Home() {
  const navigate = useNavigate()
  return (
    <main>
      {/* Hero (matching mock with two headings and stacked image) */}
      <div className="brand-hero-wrapper">
        <div className="brand-hero-title">Find the photographer you need</div>
        <div className="brand-hero-stack">
          <div className="hero-layer" />
          <div className="hero-layer2" />
          <div className="hero-main"><img src={heroImages[0]} alt="Hero" /></div>
        </div>
        <div className="brand-hero-sub">Be the photographer they need</div>
      </div>

      {/* Feature Sections */}
      <section className="brand-section">
        <div className="brand-section-grid">
          <div className="brand-feature">
            <img src={featureImages[0]} alt="Create Albums" loading="lazy" />
            <h3>Showcase your work</h3>
            <p>Group your photos into cohesive narratives. Edit titles and descriptions any time.</p>
          </div>
          <div className="brand-feature">
            <img src={featureImages[1]} alt="Discover" loading="lazy" />
            <h3>Search for keywords</h3>
            <p>Use tags & keywords to surface exactly what inspires you.</p>
          </div>
          <div className="brand-feature">
            <img src={featureImages[2]} alt="Connect" loading="lazy" />
            <h3>Connect</h3>
            <p>Follow creators, like & comment to build genuine creative relationships.</p>
          </div>
        </div>
      </section>

      {/* CTA */}
      <div className="brand-cta">
        <h2>Start your visual journey</h2>
        <p>Register now and publish your first photo or album in minutes.</p>
        <button className="brand-register-btn" onClick={() => navigate('/signup')}>Create Account</button>
      </div>

      {/* Footer */}
      <footer className="brand-footer">
        <div className="brand-footer-inner">
          <div className="brand-footer-logo">PhotoApp</div>
          <div className="footer-cols">
            <div className="footer-col">
              <div className="footer-col-title">Product</div>
              <a href="#feed" onClick={(e) => { e.preventDefault(); navigate('/feed') }}>Feed</a>
              <Link to="/profile">My Profile</Link>
            </div>
            <div className="footer-col">
              <div className="footer-col-title">Community</div>
              <Link to="/register">Join</Link>
              <a href="#trending" onClick={(e) => { e.preventDefault(); navigate('/feed') }}>Trending</a>
            </div>
            <div className="footer-col">
              <div className="footer-col-title">Resources</div>
              <a href="/IMAGE_HOSTING_GUIDE.md" target="_blank" rel="noopener noreferrer">Image Guide</a>
              <a href="/README.md" target="_blank" rel="noopener noreferrer">Docs</a>
            </div>
          </div>
        </div>
        <div style={{ textAlign:'center', marginTop:'30px', fontSize:'0.75rem', opacity:.75 }}>© {new Date().getFullYear()} PhotoApp. All rights reserved.</div>
      </footer>
    </main>
  )
}
