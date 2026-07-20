import * as signalR from '@microsoft/signalr'
import { API_BASE_URL } from './client'

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
