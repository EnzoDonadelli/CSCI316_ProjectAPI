import React, { useState } from 'react'
import { Typography, TextField, Button, Stack } from '@mui/material'
import { useNavigate } from 'react-router-dom'

export default function Login() {
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  const onSignIn = () => {
    // For now we don't call the API; redirect to the dummy John Doe profile
    // In a real flow we'd authenticate and store a token
    navigate('/users/1')
  }

  return (
    <Stack spacing={2} maxWidth={400}>
      <Typography variant="h5">Login</Typography>
      <TextField label="Username" value={username} onChange={e => setUsername(e.target.value)} />
      <TextField label="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} />
      <Button variant="contained" onClick={onSignIn}>Sign in</Button>
    </Stack>
  )
}
