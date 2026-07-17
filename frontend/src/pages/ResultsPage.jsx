import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { pollApi } from '../api/client'
import { connectToPollResults } from '../api/pollHub.js'
import PollBars from '../components/PollBar.jsx'

export default function ResultsPage() {
  const { code } = useParams()
  const [results, setResults] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    pollApi
      .getResults(code)
      .then((data) => { if (!cancelled) setResults(data) })
      .catch(() => { if (!cancelled) setError('This poll has no results yet, or does not exist.') })

    const disconnect = connectToPollResults(code, (updated) => setResults(updated))
    return () => { cancelled = true; disconnect() }
  }, [code])

  if (error && !results) {
    return (
      <div className="card" style={{ maxWidth: 460 }}>
        <h2>Not found</h2>
        <p style={{ color: 'var(--text-dim)' }}>{error}</p>
        <Link to="/" className="btn btn-ghost">Back home</Link>
      </div>
    )
  }

  if (!results) return <p className="helper">Loading results…</p>

  return (
    <div className="card">
      <div className="eyebrow">
        {results.isOpen ? (
          <><span className="pulse-dot" style={{ marginRight: 6 }} /> Live results</>
        ) : (
          'Final results'
        )}
      </div>
      <h2>{results.question}</h2>
      <p className="helper" style={{ marginBottom: 22 }}>
        {results.totalVotes} vote{results.totalVotes === 1 ? '' : 's'} so far
      </p>

      {results.type === 'OpenText' ? (
        <div>
          {results.openTextAnswers.length === 0 && (
            <p className="helper">No answers yet.</p>
          )}
          {results.openTextAnswers.map((answer, i) => (
            <div key={i} className="ticket" style={{ cursor: 'default' }}>{answer}</div>
          ))}
        </div>
      ) : (
        <PollBars options={results.options} />
      )}

      <div style={{ marginTop: 24, display: 'flex', gap: 10 }}>
        <Link to={`/poll/${code}`} className="btn btn-ghost">Back to voting page</Link>
      </div>
    </div>
  )
}
