import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { pollApi } from '../api/client'

export default function Dashboard() {
  const navigate = useNavigate()
  const [polls, setPolls] = useState(null)
  const [error, setError] = useState(null)
  const [closingCode, setClosingCode] = useState(null)

  useEffect(() => {
    if (!localStorage.getItem('psb_token')) {
      navigate('/login')
      return
    }
    load()
  }, [])

  function load() {
    pollApi.mine()
      .then(setPolls)
      .catch(() => setError('Could not load your polls. Try logging in again.'))
  }

  async function handleClose(code) {
    setClosingCode(code)
    try {
      await pollApi.close(code)
      load()
    } finally {
      setClosingCode(null)
    }
  }

  return (
    <div>
      <div className="eyebrow">Dashboard</div>
      <h2>Your polls</h2>

      {error && <p className="error-text">{error}</p>}

      {!polls && !error && <p className="helper">Loading…</p>}

      {polls && polls.length === 0 && (
        <div className="card" style={{ maxWidth: 420 }}>
          <p style={{ color: 'var(--text-dim)' }}>{"You haven't created any polls yet."}</p>
          <Link to="/create" className="btn btn-primary">Create your first poll</Link>
        </div>
      )}

      {polls && polls.length > 0 && (
        <div className="card">
          <table className="table">
            <thead>
              <tr>
                <th>Question</th>
                <th>Status</th>
                <th>Votes</th>
                <th>Created</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {polls.map((p) => (
                <tr key={p.code}>
                  <td>
                    <Link to={`/poll/${p.code}/results`}>{p.question}</Link>
                    <div className="helper mono">/{p.code}</div>
                  </td>
                  <td>
                    <span className="status-chip">
                      {p.isOpen ? <><span className="pulse-dot" /> open</> : 'closed'}
                    </span>
                  </td>
                  <td className="mono">{p.totalVotes}</td>
                  <td className="helper">{new Date(p.createdAt).toLocaleDateString()}</td>
                  <td>
                    {p.isOpen && (
                      <button
                        className="btn btn-ghost"
                        disabled={closingCode === p.code}
                        onClick={() => handleClose(p.code)}
                      >
                        {closingCode === p.code ? 'Closing…' : 'Close'}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
