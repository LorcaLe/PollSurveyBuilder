import { Link } from 'react-router-dom'

export default function Terms() {
  return (
    // Wrap the content in a card, centered with a maximum width for optimal readability
    <div className="card" style={{ margin: '40px auto', maxWidth: 800 }}>
      
      {/* Page Header */}
      <h1>Terms of Service</h1>
      <p style={{ color: 'var(--text-dim)', marginBottom: 30 }}>Last updated: July 2026</p>

      {/* Section 1 */}
      <h3>1. Acceptance of Terms</h3>
      <p>By accessing and using Ballot (&quot;we&quot;, &quot;our&quot;, or &quot;us&quot;), you agree to be bound by these Terms of Service. If you do not agree to these terms, please do not use our application.</p>

      {/* Section 2 */}
      <h3>2. User Accounts</h3>
      <p>To create polls, you must register for an account. You are responsible for maintaining the confidentiality of your password (which must meet our security requirements) and are fully responsible for all activities that occur under your account.</p>

      {/* Section 3 */}
      <h3>3. Prohibited Conduct</h3>
      <p>When using Ballot, you agree NOT to:</p>
      <ul style={{ paddingLeft: 20, marginBottom: 20 }}>
        <li style={{ marginBottom: 8 }}>Create polls that contain hate speech, harassment, explicit content, or promote illegal activities.</li>
        <li style={{ marginBottom: 8 }}>Use automated scripts, bots, or any other unauthorized means to manipulate poll results (e.g., spamming votes).</li>
        <li style={{ marginBottom: 8 }}>Impersonate any person or entity or misrepresent your affiliation with a person or entity.</li>
      </ul>

      {/* Section 4 */}
      <h3>4. Content Ownership</h3>
      <p>You retain ownership of the polls you create. However, by creating a poll on Ballot, you grant us a license to host, display, and share that content globally to facilitate the voting process.</p>

      {/* Section 5 */}
      <h3>5. Termination</h3>
      <p>We reserve the right to suspend or terminate your account and remove your polls at any time, without notice, if we determine that you have violated these Terms of Service.</p>

      {/* Section 6 */}
      <h3>6. Limitation of Liability</h3>
      <p>Ballot is provided on an &quot;as is&quot; and &quot;as available&quot; basis. We do not guarantee that the service will be uninterrupted or error-free. We shall not be liable for any indirect, incidental, or consequential damages arising from your use of the service.</p>
      
      {/* Navigation back to home */}
      <div style={{ marginTop: 40, textAlign: 'center' }}>
        <Link to="/" className="btn btn-ghost">Back to Home</Link>
      </div>
    </div>
  )
}