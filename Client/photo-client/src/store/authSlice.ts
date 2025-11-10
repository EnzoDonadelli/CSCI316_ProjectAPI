import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import api from '../api/axios'
import { setUser } from './userSlice'

type AuthState = {
  token: string | null
  loading: boolean
  error: string | null
}

const initialState: AuthState = {
  token: typeof window !== 'undefined' ? localStorage.getItem('token') : null,
  loading: false,
  error: null
}

export const login = createAsyncThunk(
  'auth/login',
  async (payload: { usernameOrEmail: string; password: string }, { dispatch, rejectWithValue }) => {
    try {
      const res = await api.post('/api/auth/login', payload)
      const data = res.data
      if (data && data.token) {
        localStorage.setItem('token', data.token)
        // optionally fetch profile
        try {
          const profile = await api.get('/api/auth/profile', { headers: { Authorization: `Bearer ${data.token}` } })
          dispatch(setUser(profile.data))
        } catch {}
        return data.token
      }
      return rejectWithValue(data.message || 'Login failed')
    } catch (err: any) {
      const serverMessage = err.response?.data?.message ?? err.response?.data?.Message
      return rejectWithValue(serverMessage || err.message)
    }
  }
)

export const register = createAsyncThunk(
  'auth/register',
  async (payload: { username: string; email: string; password: string; fullName?: string; bio?: string; profilePic?: string }, { dispatch, rejectWithValue }) => {
    try {
      const res = await api.post('/api/auth/register', payload)
      const data = res.data
      if (data && data.token) {
        localStorage.setItem('token', data.token)
        try {
          // set user from returned payload if available
          if (data.user) dispatch(setUser(data.user))
          else {
            const profile = await api.get('/api/auth/profile', { headers: { Authorization: `Bearer ${data.token}` } })
            dispatch(setUser(profile.data))
          }
        } catch {}
        return data.token
      }
      return rejectWithValue(data.message || 'Registration failed')
    } catch (err: any) {
      const serverMessage = err.response?.data?.message ?? err.response?.data?.Message
      return rejectWithValue(serverMessage || err.message)
    }
  }
)

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout(state) {
      state.token = null
      state.error = null
      localStorage.removeItem('token')
    }
  },
  extraReducers: builder => {
    builder
      .addCase(login.pending, state => {
        state.loading = true
        state.error = null
      })
      .addCase(login.fulfilled, (state, action: PayloadAction<string>) => {
        state.loading = false
        state.token = action.payload
      })
      .addCase(login.rejected, (state, action) => {
        state.loading = false
        state.error = action.payload as string || 'Login failed'
      })
      .addCase(register.pending, state => {
        state.loading = true
        state.error = null
      })
      .addCase(register.fulfilled, state => {
        state.loading = false
      })
      .addCase(register.rejected, (state, action) => {
        state.loading = false
        state.error = action.payload as string || 'Registration failed'
      })
  }
})

export const { logout } = authSlice.actions
export default authSlice.reducer
