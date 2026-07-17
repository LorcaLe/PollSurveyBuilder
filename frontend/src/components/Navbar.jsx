import { Link, useNavigate } from 'react-router-dom'

export default function Navbar() {
  const navigate = useNavigate()
  const isLoggedIn = Boolean(localStorage.getItem('psb_token'))

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
