import { useEffect, useState } from 'react'
import styles from './DashboardPage.module.scss'
import { getStats } from '../../api/api'
import AddNodeForm from '../../components/addNodeForm/AddNodeForm'

type Stats = {
  blockCount: number
  recordCount: number
  workingTime: number
  friendNodeCount: number
  friendNode: string[]
}

const DashboardPage = () => {
  const [stats, setStats] = useState<Stats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchStats = async () => {
    try {
      const res = await getStats()
      if (res.success) {
        setStats(res.result)
      } else {
        setError(res.result || 'Błąd podczas pobierania statystyk')
      }
    } catch (err) {
      setError('Serwer nie odpowiada')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchStats()
  }, [])

  return (
    <div className={styles.container}>
      <h1 className={styles.title}>📊 Panel statystyk węzła</h1>

      {loading ? (
        <p>Ładowanie...</p>
      ) : error ? (
        <p className={styles.error}>{error}</p>
      ) : stats ? (
        <>
          <div className={styles.statsGrid}>
            <div className={styles.card}>
              <h2>🔗 Bloki</h2>
              <p>{stats.blockCount}</p>
            </div>
            <div className={styles.card}>
              <h2>📚 Rekordy</h2>
              <p>{stats.recordCount}</p>
            </div>
            <div className={styles.card}>
              <h2>⏳ Czas działania</h2>
              <p>{formatTime(stats.workingTime)}</p>
            </div>
            <div className={styles.card}>
              <h2>👥 Sąsiedzi ({stats.friendNodeCount})</h2>
              {stats.friendNode.length ? (
                <ul>
                  {stats.friendNode.map((node, idx) => (
                    <li key={idx}>{node}</li>
                  ))}
                </ul>
              ) : (
                <p>Brak sąsiadów 😢</p>
              )}
            </div>
          </div>

          {/* 👇 Dodajemy formularz ręcznego dodawania węzła */}
          <AddNodeForm onNodeAdded={fetchStats} />
        </>
      ) : (
        <p>Nieznany błąd 😐</p>
      )}
    </div>
  )
}

const formatTime = (seconds: number) => {
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return `${m} min ${s} sek`
}

export default DashboardPage
