import { Outlet } from 'react-router-dom'
import Navbar from './Navbar.jsx'
import Footer from './Footer.jsx' 

export default function Layout() {
  return (

    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      

      <Navbar />


      <main className="shell" style={{ flex: 1, width: '100%' }}>

        <Outlet />
      </main>


      <Footer />
      
    </div>
  )
}