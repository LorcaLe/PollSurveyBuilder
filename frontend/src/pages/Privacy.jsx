import { Link } from 'react-router-dom'

export default function Privacy() {
  return (
    // Wrap the content in a card, centered with a maximum width for optimal readability
    <div className="card" style={{ margin: '40px auto', maxWidth: 800 }}>
      
      {/* Page Header */}
      <h1>Privacy Policy</h1>
      <p style={{ color: 'var(--text-dim)', marginBottom: 30 }}>Last updated: July 2026</p>

      <p>Your privacy is important to us. This Privacy Policy explains how Ballot collects, uses, and protects your personal information.</p>

      {/* Section 1 */}
      <h3 style={{ marginTop: 24 }}>1. Information We Collect</h3>
      <ul style={{ paddingLeft: 20, marginBottom: 20 }}>
        <li style={{ marginBottom: 8 }}><strong>Account Information:</strong> When you register, we collect your Display Name, Email Address, and Password (which is securely hashed).</li>
        <li style={{ marginBottom: 8 }}><strong>Voting Data:</strong> When you vote on a poll, we may collect anonymous session data, device identifiers, or use Local Storage to prevent duplicate voting and ensure the integrity of the poll results.</li>
        <li style={{ marginBottom: 8 }}><strong>Usage Data:</strong> We may collect basic diagnostic data (such as browser type or access times) to monitor and improve the app&apos;s performance.</li>
      </ul>

      {/* Section 2 */}
      <h3>2. How We Use Your Information</h3>
      <p>We use the collected information for the following purposes:</p>
      <ul style={{ paddingLeft: 20, marginBottom: 20 }}>
        <li style={{ marginBottom: 8 }}>To provide, operate, and maintain the Ballot application.</li>
        <li style={{ marginBottom: 8 }}>To authenticate users and secure creator dashboards.</li>
        <li style={{ marginBottom: 8 }}>To ensure poll accuracy by preventing multiple votes from the same user.</li>
      </ul>

      {/* Section 3 */}
      <h3>3. Data Storage and Security</h3>
      <p>We use an authentication token (JWT) stored in your browser&apos;s Local Storage to keep you logged in securely. We implement reasonable security measures to protect your personal information, but please remember that no method of transmission over the Internet is 100% secure.</p>

      {/* Section 4 */}
      <h3>4. Sharing of Information</h3>
      <p>We do not sell, trade, or rent your personal identification information to others. Voting stays anonymous to the public; only aggregated results are displayed on the live polling pages.</p>

      {/* Section 5 */}
      <h3>5. Contact Us</h3>
      <p>If you have any questions about this Privacy Policy, please contact us at: <strong>support@ballot.app</strong>.</p>
      
      {/* Navigation back to home */}
      <div style={{ marginTop: 40, textAlign: 'center' }}>
        <Link to="/" className="btn btn-ghost">Back to Home</Link>
      </div>
    </div>
  )
}