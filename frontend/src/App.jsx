import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout.jsx' 
import Home from './pages/Home.jsx'
import CreatePoll from './pages/CreatePoll.jsx'
import VotePage from './pages/VotePage.jsx'
import ResultsPage from './pages/ResultsPage.jsx'
import Dashboard from './pages/Dashboard.jsx'
import Login from './pages/Login.jsx'
import Register from './pages/Register.jsx'
import Terms from './pages/Terms.jsx'
import Privacy from './pages/Privacy.jsx'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/create" element={<CreatePoll />} />
        <Route path="/poll/:code" element={<VotePage />} />
        <Route path="/poll/:code/results" element={<ResultsPage />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/terms" element={<Terms />} />
        <Route path="/privacy" element={<Privacy />} />
      </Route>
    </Routes>
  )
}