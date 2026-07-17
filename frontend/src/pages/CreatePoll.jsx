import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { pollApi } from '../api/client'
import QRCodeBadge from '../components/QRCodeBadge.jsx'

const POLL_TYPES = [
  { value: 'SingleChoice', label: 'Single choice (up to 6 options)' },
  { value: 'YesNo', label: 'Yes / No' },
  { value: 'Rating', label: 'Star rating (1-5)' },
  { value: 'OpenText', label: 'Open text answer' },
]

export default function CreatePoll() {
  const isLoggedIn = Boolean(localStorage.getItem('psb_token'))
  const navigate = useNavigate()

  const [question, setQuestion] = useState('')
  const [type, setType] = useState('SingleChoice')
  const [options, setOptions] = useState(['', ''])
  const [expiresInMinutes, setExpiresInMinutes] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)
  const [created, setCreated] = useState(null)

  if (!isLoggedIn) {
    return (
      <div className="card" style={{ maxWidth: 460 }}>
        <div className="eyebrow">Sign in required</div>
        <h2>Creating a poll needs an account</h2>
        <p style={{ color: 'var(--text-dim)' }}>
          Voting stays anonymous, but the creator dashboard (closing polls, seeing
          all your links) is protected behind login.
        </p>
        <div style={{ display: 'flex', gap: 10 }}>
          <Link to="/login" className="btn btn-primary">Log in</Link>
          <Link to="/register" className="btn btn-ghost">Create account</Link>
        </div>
      </div>
    )
  }

  function updateOption(index, value) {
    const next = [...options]
    next[index] = value
    setOptions(next)
  }

  function addOption() {
    if (options.length < 6) setOptions([...options, ''])
  }

  function removeOption(index) {
    if (options.length > 2) setOptions(options.filter((_, i) => i !== index))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const payload = {
        question,
        type,
        options: type === 'SingleChoice' ? options.filter((o) => o.trim()) : [],
        expiresInMinutes: expiresInMinutes ? Number(expiresInMinutes) : null,
      }
      const result = await pollApi.create(payload)
      setCreated(result)
    } catch (err) {
      setError(err.response?.data?.message || 'Could not create the poll. Check your inputs and try again.')
    } finally {
      setSubmitting(false)
    }
  }

  if (created) {
    return (
      <div className="card" style={{ maxWidth: 480 }}>
        <div className="eyebrow">Poll created</div>
        <h2>Share it now</h2>
        <div className="share-box" style={{ marginBottom: 20 }}>{created.shareUrl}</div>
        <div style={{ marginBottom: 20 }}>
          <QRCodeBadge code={created.code} />
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button className="btn btn-primary" onClick={() => navigate(`/poll/${created.code}/results`)}>
            Watch live results
          </button>
          <button className="btn btn-ghost" onClick={() => setCreated(null)}>Create another</button>
        </div>
      </div>
    )
  }

  return (
    <div className="card" style={{ maxWidth: 540 }}>
      <div className="eyebrow">New poll</div>
      <h2>Ask something</h2>
      <form onSubmit={handleSubmit}>
        <div className="field">
          <label htmlFor="question">Question</label>
          <input
            id="question"
            type="text"
            required
            maxLength={300}
            placeholder="What should we build next?"
            value={question}
            onChange={(e) => setQuestion(e.target.value)}
          />
        </div>

        <div className="field">
          <label htmlFor="type">Poll type</label>
          <select id="type" value={type} onChange={(e) => setType(e.target.value)}>
            {POLL_TYPES.map((t) => (
              <option key={t.value} value={t.value}>{t.label}</option>
            ))}
          </select>
        </div>

        {type === 'SingleChoice' && (
          <div className="field">
            <label>Options (2-6)</label>
            {options.map((opt, i) => (
              <div key={i} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                <input
                  type="text"
                  placeholder={`Option ${i + 1}`}
                  maxLength={120}
                  value={opt}
                  onChange={(e) => updateOption(i, e.target.value)}
                />
                {options.length > 2 && (
                  <button type="button" className="btn btn-ghost" onClick={() => removeOption(i)}>✕</button>
                )}
              </div>
            ))}
            {options.length < 6 && (
              <button type="button" className="btn btn-ghost" onClick={addOption}>+ Add option</button>
            )}
          </div>
        )}

        <div className="field">
          <label htmlFor="expires">Closes after (minutes, optional)</label>
          <input
            id="expires"
            type="number"
            min="1"
            placeholder="Leave blank for no expiry"
            value={expiresInMinutes}
            onChange={(e) => setExpiresInMinutes(e.target.value)}
          />
        </div>

        {error && <p className="error-text">{error}</p>}

        <button className="btn btn-primary btn-block" type="submit" disabled={submitting}>
          {submitting ? 'Creating…' : 'Create poll'}
        </button>
      </form>
    </div>
  )
}
