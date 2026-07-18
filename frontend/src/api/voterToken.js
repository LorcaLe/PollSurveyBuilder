// Anonymous "one vote per browser" identity. Previously this relied on a
// server-set cookie, but Safari/iOS blocks third-party cookies (ITP) between
// the frontend and API when they live on different domains in production -
// no cookie configuration can reliably work around that. A token the client
// generates once and stores itself sidesteps the whole problem: it's app
// data (localStorage), not a cross-site cookie, so no browser blocks it.
const STORAGE_KEY = 'psb_voter_token'

export function getVoterToken() {
  let token = localStorage.getItem(STORAGE_KEY)
  if (!token) {
    token = crypto.randomUUID()
    localStorage.setItem(STORAGE_KEY, token)
  }
  return token
}