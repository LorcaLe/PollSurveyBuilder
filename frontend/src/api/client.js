import axios from 'axios'

// Vite exposes env vars prefixed with VITE_ - set VITE_API_URL in frontend/.env
// (see .env.example). Falls back to the local dev API port.
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5080'

const client = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true, // sends/receives the voter_token cookie
})

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('psb_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export const authApi = {
  register: (payload) => client.post('/api/auth/register', payload).then((r) => r.data),
  login: (payload) => client.post('/api/auth/login', payload).then((r) => r.data),
}

export const pollApi = {
  create: (payload) => client.post('/api/polls', payload).then((r) => r.data),
  getForVoting: (code) => client.get(`/api/polls/${code}`).then((r) => r.data),
  getResults: (code) => client.get(`/api/polls/${code}/results`).then((r) => r.data),
  getQrCode: (code) => client.get(`/api/polls/${code}/qrcode`).then((r) => r.data),
  close: (code) => client.post(`/api/polls/${code}/close`).then((r) => r.data),
  mine: () => client.get('/api/polls/mine').then((r) => r.data),
  vote: (code, payload) => client.post(`/api/polls/${code}/vote`, payload).then((r) => r.data),
}

export default client
