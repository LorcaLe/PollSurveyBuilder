import { Link } from 'react-router-dom'

export default function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer 
      style={{ 
        padding: '40px 24px 24px', 
        textAlign: 'center', 
        borderTop: '1px solid var(--border)',
        marginTop: 'auto' // Pushes the footer to the bottom of its flex container
      }}
    >
      {/* Brand Logo - Centered */}
      <div className="brand" style={{ justifyContent: 'center', marginBottom: '20px' }}>
        <div className="stub"></div>
        Ballot
      </div>

      {/* Navigation Links */}
      <div 
        style={{ 
          display: 'flex', 
          justifyContent: 'center', 
          gap: '24px', 
          marginBottom: '20px', 
          flexWrap: 'wrap' // Allows links to wrap to the next line on very small screens
        }}
      >
        <Link to="/terms" style={{ color: 'var(--text-dim)', fontSize: '0.9rem' }}>
          Terms of Service
        </Link>
        <Link to="/privacy" style={{ color: 'var(--text-dim)', fontSize: '0.9rem' }}>
          Privacy Policy
        </Link>
        <Link to="/report" style={{ color: 'var(--text-dim)', fontSize: '0.9rem' }}>
          Report Abuse
        </Link>
      </div>

      {/* Copyright Information */}
      <p style={{ color: 'var(--text-dim)', fontSize: '0.85rem', margin: 0 }}>
        © {currentYear} Ballot. All rights reserved.
      </p>
    </footer>
  )
}