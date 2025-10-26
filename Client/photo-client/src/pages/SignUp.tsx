import React from 'react'
import { Typography, TextField, Button, Stack } from '@mui/material'

export default function SignUp() {
  return (
    <Stack spacing={2} maxWidth={400}>
      <Typography variant="h5">Sign Up</Typography>
      <TextField label="Username" />
      <TextField label="Email" />
      <TextField label="Password" type="password" />
      <Button variant="contained">Create account</Button>
    </Stack>
  )
}
