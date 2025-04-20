import axios from 'axios'
import { API_URL } from './config'

const api = axios.create({
  baseURL: `${API_URL}/api`,
  timeout: 5000,
})

export const getStats = async () => {
  const res = await api.get('/getStats')
  return res.data
}

export const addNode = async (ip: string, port: string) => {
    const res = await api.get(`/addNewNode?ip=${ip}&port=${port}`) // 👈
    return res.data
  }
  

export const addRecord = async (key: string, value: string) => {
  const res = await api.post('/addRecord', { Key: key, Value: value })
  return res.data
}

export const getFriendIp = async () => {
  const res = await api.get('/getFriendIp')
  return res.data
}

export const sendPing = async () => {
  const res = await api.get('/sendMessage')
  return res.data
}

export const sendTestBlock = async () => {
  const res = await api.get('/SendBlock')
  return res.data
}

