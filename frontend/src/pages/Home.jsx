import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'

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
      <h1 style={{ fontSize: '2.4rem', maxWidth: 560 }}>
        Ask a question. Watch the bars fill in <span style={{ color: 'var(--gold)' }}>live</span>.
      </h1>
      <p style={{ color: 'var(--text-dim)', maxWidth: 480, marginBottom: 28 }}>
        {"Create a poll, share the link or QR code, and see every vote land on the results page the instant it's cast — powered by SignalR."}
      </p>

      <div style={{ display: 'flex', gap: 12, marginBottom: 40, flexWrap: 'wrap' }}>
        <Link to="/create" className="btn btn-primary">Create a poll</Link>
        <a href="#join" className="btn btn-ghost">Have a code? Jump to a poll ↓</a>
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
