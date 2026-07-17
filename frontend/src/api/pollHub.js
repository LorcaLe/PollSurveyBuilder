import * as signalR from '@microsoft/signalr'
import { API_BASE_URL } from './client'

/**
 * Opens one SignalR connection to the PollHub, joins the given poll's group,
 * and calls onUpdate(results) every time the server broadcasts a new tally.
 * Returns a cleanup function - call it on unmount to leave the group and
 * close the connection.
 */
export function connectToPollResults(pollCode, onUpdate) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/polls`)
    .withAutomaticReconnect()
    .build()

  connection.on('resultsUpdated', (results) => {
    onUpdate(results)
  })

  connection
    .start()
    .then(() => connection.invoke('JoinPoll', pollCode))
    .catch((err) => console.error('SignalR connection failed:', err))

  return () => {
    connection.invoke('LeavePoll', pollCode).catch(() => {})
    connection.stop()
  }
}
