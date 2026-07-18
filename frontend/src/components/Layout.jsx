import { Outlet } from 'react-router-dom'
import Navbar from './Navbar.jsx'
import Footer from './Footer.jsx' // Ensure you have created this file from the previous step

export default function Layout() {
  return (
    // Flexbox container that takes at least the full height of the viewport
    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      
      {/* Top Navigation Bar */}
      <Navbar />

      {/* 
        Main content area using your existing "shell" class for centering and padding.
        The "flex: 1" style forces this section to grow, pushing the Footer to the very bottom.
      */}
      <main className="shell" style={{ flex: 1, width: '100%' }}>
        {/* The Outlet acts as a placeholder for your page components (Home, Login, etc.) */}
        <Outlet />
      </main>

      {/* Footer component at the bottom */}
      <Footer />
      
    </div>
  )
}