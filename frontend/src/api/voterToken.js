const STORAGE_KEY = 'psb_voter_token'

export function getVoterToken() {
  let token = localStorage.getItem(STORAGE_KEY)
  if (!token) {
    token = crypto.randomUUID()
    localStorage.setItem(STORAGE_KEY, token)
  }
  return token
}