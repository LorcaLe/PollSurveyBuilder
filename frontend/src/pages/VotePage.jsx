import { useEffect, useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { pollApi } from '../api/client'

export default function VotePage() {
  const { code } = useParams()
  const navigate = useNavigate()

  const [poll, setPoll] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [selectedOptionId, setSelectedOptionId] = useState(null)
  const [textAnswer, setTextAnswer] = useState('')
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    pollApi
      .getForVoting(code)
      .then((data) => { if (!cancelled) setPoll(data) })
      .catch(() => { if (!cancelled) setError('This poll link is invalid or has been removed.') })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [code])

  async function submitVote() {
    setSubmitting(true)
    setError(null)
    try {
      const payload = poll.type === 'OpenText'
        ? { textAnswer }
        : { optionId: selectedOptionId }
      await pollApi.vote(code, payload)
      navigate(`/poll/${code}/results`)
    } catch (err) {
      const message = err.response?.data?.message
        || (Array.isArray(err.response?.data) ? err.response.data.join(' ') : null)
        || 'Could not submit your vote.'
      setError(message)
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <p className="helper">Loading poll…</p>
  if (error && !poll) {
    return (
      <div className="card" style={{ maxWidth: 460 }}>
        <h2>Poll not found</h2>
        <p style={{ color: 'var(--text-dim)' }}>{error}</p>
        <Link to="/" className="btn btn-ghost">Back home</Link>
      </div>
    )
  }

  if (poll.alreadyVoted) {
    return (
      <div className="card" style={{ maxWidth: 460 }}>
        <div className="eyebrow">Already counted</div>
        <h2>{poll.question}</h2>
        <p style={{ color: 'var(--text-dim)' }}>{"You've already voted on this poll from this browser."}</p>
        <button className="btn btn-primary" onClick={() => navigate(`/poll/${code}/results`)}>
          See live results
        </button>
      </div>
    )
  }

  if (!poll.isOpen) {
    return (
      <div className="card" style={{ maxWidth: 460 }}>
        <div className="eyebrow">Closed</div>
        <h2>{poll.question}</h2>
        <p style={{ color: 'var(--text-dim)' }}>This poll is no longer accepting votes.</p>
        <button className="btn btn-primary" onClick={() => navigate(`/poll/${code}/results`)}>
          See final results
        </button>
      </div>
    )
  }

  const canSubmit = poll.type === 'OpenText' ? textAnswer.trim().length > 0 : selectedOptionId != null

  return (
    <div className="card" style={{ maxWidth: 520 }}>
      <div className="eyebrow">
        <span className="pulse-dot" style={{ marginRight: 6 }} /> Live poll
      </div>
      <h2>{poll.question}</h2>

      {poll.type === 'OpenText' ? (
        <div className="field">
          <label htmlFor="answer">Your answer</label>
          <textarea
            id="answer"
            rows={4}
            maxLength={1000}
            value={textAnswer}
            onChange={(e) => setTextAnswer(e.target.value)}
            placeholder="Type your answer…"
          />
        </div>
      ) : poll.type === 'Rating' ? (
        <div style={{ display: 'flex', gap: 8, margin: '18px 0' }}>
          {poll.options.map((opt, i) => (
            <button
              type="button"
              key={opt.id}
              onClick={() => setSelectedOptionId(opt.id)}
              className="btn"
              style={{
                background: selectedOptionId === opt.id ? 'var(--gold)' : 'var(--panel-raised)',
                color: selectedOptionId === opt.id ? '#241c05' : 'var(--text)',
                fontSize: '1.3rem',
                padding: '10px 16px',
              }}
              aria-label={`${i + 1} star`}
            >
              ★
            </button>
          ))}
        </div>
      ) : (
        <div style={{ margin: '18px 0' }}>
          {poll.options.map((opt) => (
            <div
              key={opt.id}
              className={`ticket ${selectedOptionId === opt.id ? 'selected' : ''}`}
              onClick={() => setSelectedOptionId(opt.id)}
              role="radio"
              aria-checked={selectedOptionId === opt.id}
              tabIndex={0}
            >
              {opt.text}
            </div>
          ))}
        </div>
      )}

      {error && <p className="error-text">{error}</p>}

      <button
        className="btn btn-primary btn-block"
        disabled={!canSubmit || submitting}
        onClick={submitVote}
      >
        {submitting ? 'Submitting…' : 'Submit vote'}
      </button>
    </div>
  )
}
