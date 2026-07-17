import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authApi } from '../api/client'

export default function Register() {
  const navigate = useNavigate()
  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      const result = await authApi.register({ displayName, email, password })
      localStorage.setItem('psb_token', result.token)
      navigate('/dashboard')
    } catch (err) {
      const messages = err.response?.data
      setError(Array.isArray(messages) ? messages.join(' ') : 'Could not create account. Password needs 8+ chars, upper + lowercase + a digit.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="card" style={{ maxWidth: 400 }}>
      <div className="eyebrow">Get started</div>
      <h2>Create your account</h2>
      <form onSubmit={handleSubmit}>
        <div className="field">
          <label htmlFor="displayName">Display name</label>
          <input id="displayName" type="text" required value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
        </div>
        <div className="field">
          <label htmlFor="email">Email</label>
          <input id="email" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div className="field">
          <label htmlFor="password">Password</label>
          <input id="password" type="password" required value={password} onChange={(e) => setPassword(e.target.value)} />
          <p className="helper">At least 8 characters, with upper and lower case letters and a digit.</p>
        </div>
        {error && <p className="error-text">{error}</p>}
        <button className="btn btn-primary btn-block" type="submit" disabled={submitting}>
          {submitting ? 'Creating…' : 'Create account'}
        </button>
      </form>
      <p className="helper" style={{ marginTop: 14 }}>
        Already have an account? <Link to="/login">Log in</Link>
      </p>
    </div>
  )
}
