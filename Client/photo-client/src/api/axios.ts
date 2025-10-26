import axios from 'axios'

const baseURL = (import.meta.env.VITE_API_URL as string) || 'http://localhost:5000'

export default axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json'
  }
})
