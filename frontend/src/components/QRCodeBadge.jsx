import { useEffect, useState } from 'react'
import { pollApi } from '../api/client'

export default function QRCodeBadge({ code }) {
  const [dataUrl, setDataUrl] = useState(null)

  useEffect(() => {
    let cancelled = false
    pollApi.getQrCode(code).then((res) => {
      if (!cancelled) setDataUrl(res.dataUrl)
    })
    return () => { cancelled = true }
  }, [code])

  if (!dataUrl) return null

  return (
    <div style={{ textAlign: 'center' }}>
      <img
        src={dataUrl}
        alt={`QR code linking to poll ${code}`}
        style={{ width: 160, height: 160, borderRadius: 8, background: '#fff', padding: 8 }}
      />
      <p className="helper">Scan to vote from a phone</p>
    </div>
  )
}
