import React, { useState } from 'react'
import { Typography, TextField, Button, Stack, Alert } from '@mui/material'
import { useNavigate } from 'react-router-dom'
import { useAppDispatch, useAppSelector } from '../store/hooks'
import { login } from '../store/authSlice'

export default function Login() {
  const navigate = useNavigate()
  const dispatch = useAppDispatch()
  const auth = useAppSelector(s => (s as any).auth)

  const [usernameOrEmail, setUsernameOrEmail] = useState('')
  const [password, setPassword] = useState('')

  const onSignIn = async () => {
    try {
      const res = await dispatch(login({ usernameOrEmail, password })).unwrap()
      // login thunk stores token and profile; navigate to feed
      navigate('/feed')
    } catch (err) {
      // handled below via auth.error
    }
  }

  return (
    <Stack spacing={2} maxWidth={400}>
      <Typography variant="h5">Login</Typography>
      {auth?.error && <Alert severity="error">{auth.error}</Alert>}
      <TextField label="Username or Email" value={usernameOrEmail} onChange={e => setUsernameOrEmail(e.target.value)} />
      <TextField label="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} />
      <Button variant="contained" onClick={onSignIn} disabled={auth?.loading}>Sign in</Button>
      <Button variant="text" onClick={() => navigate('/signup')}>Create an account</Button>
    </Stack>
  )
}
