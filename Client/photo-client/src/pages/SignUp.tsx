import React, { useState } from 'react'
import { Typography, TextField, Button, Stack, Alert } from '@mui/material'
import { useNavigate } from 'react-router-dom'
import { useAppDispatch, useAppSelector } from '../store/hooks'
import { register } from '../store/authSlice'

export default function SignUp() {
  const navigate = useNavigate()
  const dispatch = useAppDispatch()
  const auth = useAppSelector(s => (s as any).auth)

  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fullName, setFullName] = useState('')

  const onCreate = async () => {
    // basic client-side validation to avoid sending invalid payloads
    if (!username || username.length < 3) return alert('Username must be at least 3 characters')
    if (!email || !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) return alert('Please enter a valid email')
    if (!password || password.length < 6) return alert('Password must be at least 6 characters')

    try {
      // include fullName in registration payload
      const token = await dispatch(register({ username, email, password, fullName })).unwrap()
      // if registration returned a token and user is set, navigate to feed
      if (token) navigate('/feed')
      else navigate('/login')
    } catch (err: any) {
      // handled by auth slice; show generic fallback
      if (err?.message) alert(err.message)
    }
  }

  return (
    <Stack spacing={2} maxWidth={400}>
      <Typography variant="h5">Sign Up</Typography>
      {auth?.error && <Alert severity="error">{auth.error}</Alert>}
  <TextField label="Username" value={username} onChange={e => setUsername(e.target.value)} />
  <TextField label="Full name" value={fullName} onChange={e => setFullName(e.target.value)} />
  <TextField label="Email" value={email} onChange={e => setEmail(e.target.value)} />
  <TextField label="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} />
  <Button variant="contained" onClick={onCreate} disabled={auth?.loading}>Create account</Button>
    </Stack>
  )
}
