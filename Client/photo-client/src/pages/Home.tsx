import React from 'react'
import { Typography, Stack, Button } from '@mui/material'
import { Link } from 'react-router-dom'

export default function Home() {
  return (
    <Stack spacing={2}>
      <Typography variant="h4">Welcome to PhotoApp</Typography>
      <Typography>Explore photos from users and albums.</Typography>
      <Button component={Link} to="/feed" variant="contained">Go to Feed</Button>
    </Stack>
  )
}
