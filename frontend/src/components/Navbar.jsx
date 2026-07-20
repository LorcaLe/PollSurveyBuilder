import { Link, useNavigate } from 'react-router-dom'
import { useState, useEffect } from 'react'

export default function Navbar() {
  const navigate = useNavigate()
  const token = localStorage.getItem('psb_token')
  const isLoggedIn = Boolean(token)

  const [userName, setUserName] = useState('')

  useEffect(() => {
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]))

        console.log("Token Payload:", payload)

        const name = payload.displayName
          || payload.name
          || payload.unique_name
          || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
          || 'User'

        setUserName(name)
      } catch (error) {
        setUserName('User')
      }
    }
  }, [token])

  function logout() {
    localStorage.removeItem('psb_token')
    navigate('/')
  }

  return (
    <div className="topbar">
      <Link to="/" className="brand">
        <span className="stub" />
        Ballot
      </Link>
      <div className="nav-links">
        {isLoggedIn ? (
          <>

            <span style={{ color: 'var(--violet)', fontWeight: 600, marginRight: '10px' }}>
              Hi, {userName}!
            </span>

            <Link to="/dashboard">My polls</Link>
            <Link to="/create">New poll</Link>
            <button className="btn btn-ghost" onClick={logout}>Log out</button>
          </>
        ) : (
          <>
            <Link to="/login">Log in</Link>
            <Link to="/create" className="btn btn-primary">Create a poll</Link>
          </>
        )}
      </div>
    </div>
  )
}