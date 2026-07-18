import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'

const FEATURES = [
  { icon: '✓', color: '#7c3aed', title: 'Single choice & Yes/No', text: 'Up to 6 options, or a quick yes/no call.' },
  { icon: '★', color: '#ec4899', title: 'Star ratings', text: '1–5 stars, aggregated live as votes land.' },
  { icon: '💬', color: '#fb923c', title: 'Open text answers', text: 'Collect free-form feedback, not just clicks.' },
  { icon: '⚡', color: '#14b8a6', title: 'Real-time results', text: 'SignalR pushes every vote to the results page instantly.' },
  { icon: '▦', color: '#7c3aed', title: 'QR code sharing', text: 'Every poll gets a scannable code, no app needed.' },
  { icon: '🔒', color: '#ec4899', title: 'One vote per browser', text: 'No login for voters — just no double-voting either.' },
]

export default function Home() {
  const [code, setCode] = useState('')
  const navigate = useNavigate()

  function goToPoll(e) {
    e.preventDefault()
    if (code.trim()) navigate(`/poll/${code.trim()}`)
  }

  return (
    <div>
      <div className="eyebrow">Real-time polling, no login for voters</div>
      <h1 style={{ fontSize: '2.6rem', maxWidth: 600 }}>
        Ask a question. Watch the bars fill in <span className="hero-badge">live</span>.
      </h1>
      <p style={{ color: 'var(--text-dim)', maxWidth: 480, marginBottom: 28, fontSize: '1.02rem' }}>
        Create a poll, share the link or QR code, and see every vote land on the results
        page the instant it's cast — powered by SignalR.
      </p>

      <div style={{ display: 'flex', gap: 12, marginBottom: 12, flexWrap: 'wrap' }}>
        <Link to="/create" className="btn btn-primary">Create a poll</Link>
        <a href="#join" className="btn btn-ghost">Have a code? Jump to a poll ↓</a>
      </div>

      <div className="feature-grid">
        {FEATURES.map((f) => (
          <div className="feature-card" key={f.title}>
            <div className="icon-badge" style={{ background: f.color }}>{f.icon}</div>
            <h3>{f.title}</h3>
            <p>{f.text}</p>
          </div>
        ))}
      </div>

      <div className="card" id="join" style={{ maxWidth: 420 }}>
        <div className="eyebrow">Join a poll</div>
        <form onSubmit={goToPoll}>
          <div className="field">
            <label htmlFor="code">Poll code</label>
            <input
              id="code"
              type="text"
              placeholder="e.g. 7fGh2"
              value={code}
              onChange={(e) => setCode(e.target.value)}
            />
          </div>
          <button className="btn btn-primary btn-block" type="submit">Go to poll</button>
        </form>
      </div>
    </div>
  )
}