import { useNavigate } from 'react-router-dom'
import { useState } from 'react'
import styles from './ConnectPage.module.scss'
import { HOME_ROUTE } from '../../routes/paths'
import { addNode, sendPing } from '../../api/api'

const ConnectPage = () => {
  const navigate = useNavigate()
  const [ip, setIp] = useState('')
  const [port, setPort] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleConnect = async () => {
    try {
      setLoading(true)
      const res = await addNode(ip, port)
      if (res.success) {
        navigate(`/${HOME_ROUTE}`)
      } else {
        setError(res.result)
      }
    } catch (err) {
      setError('Błąd połączenia')
    } finally {
      setLoading(false)
    }
  }

  const handleStartAlone = () => {
    navigate(`/${HOME_ROUTE}`)
  }

  const checkConnection = async () => {
    try {
      const res = await sendPing()
      if (!res.success) {
        setError('Serwer nie odpowiada')
      }
    } catch {
      setError('Serwer nie działa')
    }
  }

  return (
    <div className={styles.container}>
      <h1 className={styles.title}>Połączenie z siecią</h1>

      <div className={styles.form}>
        <input
          type="text"
          placeholder="IP (np. 127.0.0.1)"
          value={ip}
          onChange={(e) => setIp(e.target.value)}
        />
        <input
          type="text"
          placeholder="Port TCP (np. 8080)"
          value={port}
          onChange={(e) => setPort(e.target.value)}
        />

        <button onClick={handleConnect} disabled={loading}>
          {loading ? 'Łączenie...' : 'Połącz się'}
        </button>

        <button className={styles.secondary} onClick={handleStartAlone}>
          Jestem pierwszym węzłem
        </button>

        <button className={styles.ping} onClick={checkConnection}>
          Sprawdź połączenie z serwerem
        </button>

        {error && <div className={styles.error}>{error}</div>}
      </div>
    </div>
  )
}

export default ConnectPage

