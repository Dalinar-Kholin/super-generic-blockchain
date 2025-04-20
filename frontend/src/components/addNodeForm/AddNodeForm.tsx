import { useState } from 'react'
import { addNode } from '../../api/api'
import styles from './AddNodeForm.module.scss'

type Props = {
  onNodeAdded: () => void
}

const AddNodeForm = ({ onNodeAdded }: Props) => {
  const [ip, setIp] = useState('')
  const [port, setPort] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleAdd = async () => {
    setLoading(true)
    setMessage(null)
    try {
      const res = await addNode(ip, port)
      if (res.success) {
        setMessage('✅ Dodano nowy węzeł!')
        setIp('')
        setPort('')
        onNodeAdded?.()
      } else {
        setMessage(`❌ Błąd: ${res.result}`)
      }
    } catch {
      setMessage('❌ Serwer nie odpowiada')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={styles.container}>
      <h2 className={styles.title}>➕ Dodaj nowy węzeł</h2>
      <div className={styles.form}>
        <input
          type="text"
          placeholder="IP (np. 127.0.0.1)"
          value={ip}
          onChange={(e) => setIp(e.target.value)}
        />
        <input
          type="text"
          placeholder="Port TCP (np. 8090)"
          value={port}
          onChange={(e) => setPort(e.target.value)}
        />
        <button onClick={handleAdd} disabled={loading}>
          {loading ? 'Dodawanie...' : 'Dodaj'}
        </button>
      </div>
      {message && <p className={styles.message}>{message}</p>}
    </div>
  )
}

export default AddNodeForm
