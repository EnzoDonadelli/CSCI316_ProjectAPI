import axios from 'axios'

const baseURL = (import.meta.env.VITE_API_URL as string) || 'http://localhost:5000'

const instance = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Attach token from localStorage on each request
instance.interceptors.request.use(config => {
  try {
    const token = localStorage.getItem('token')
    if (token && config.headers) {
      config.headers['Authorization'] = `Bearer ${token}`
    }
  } catch {}
  return config
})

// Optional: handle 401 globally
instance.interceptors.response.use(
  r => r,
  err => {
    if (err.response?.status === 401) {
      // clear token if unauthorized
      try { localStorage.removeItem('token') } catch {}
      // allow specific components to handle further (no redirect here)
    }
    return Promise.reject(err)
  }
)

export default instance
