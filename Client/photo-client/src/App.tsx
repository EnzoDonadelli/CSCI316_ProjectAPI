import React from 'react'
import { Routes, Route, Link, useNavigate } from 'react-router-dom'
import { Container, AppBar, Toolbar, Typography, Button } from '@mui/material'
import Home from './pages/Home'
import Feed from './pages/Feed'
import Login from './pages/Login'
import SignUp from './pages/SignUp'
import User from './pages/User'
import AlbumPage from './pages/Album'
import Chat from './pages/Chat'
import ProtectedRoute from './components/ProtectedRoute'
import { useAppDispatch, useAppSelector } from './store/hooks'
import { logout } from './store/authSlice'
import { useEffect } from 'react'
import api from './api/axios'
import { setUser } from './store/userSlice'

export default function App() {
  const navigate = useNavigate()
  const dispatch = useAppDispatch()
  const auth = useAppSelector(s => (s as any).auth)

  const onLogout = () => {
    dispatch(logout())
    navigate('/login')
  }

  const currentUser = useAppSelector(s => (s as any).user?.user)

  useEffect(() => {
    // if we have a token but no user info, try to fetch profile
    const token = localStorage.getItem('token')
    if (token && !currentUser) {
      api.get('/api/auth/profile')
        .then(res => dispatch(setUser(res.data)))
        .catch(() => {})
    }
  }, [dispatch, currentUser])

  return (
    <div>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            PhotoApp
          </Typography>
          <Button color="inherit" component={Link} to="/">Home</Button>
          <Button color="inherit" component={Link} to="/feed">Feed</Button>
          {auth?.token ? (
            <>
              {currentUser?.id && (
                <Button color="inherit" component={Link} to={`/users/${currentUser.id}`}>My Profile</Button>
              )}
              <Button color="inherit" onClick={onLogout}>Logout</Button>
            </>
          ) : (
            <Button color="inherit" component={Link} to="/login">Login</Button>
          )}
        </Toolbar>
      </AppBar>
      <Container sx={{ mt: 3 }}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/feed" element={<ProtectedRoute><Feed /></ProtectedRoute>} />
          <Route path="/login" element={<Login />} />
          <Route path="/signup" element={<SignUp />} />
          <Route path="/users/:id" element={<User />} />
          <Route path="/albums/:id" element={<AlbumPage />} />
          <Route path="/chat/:id" element={<ProtectedRoute><Chat /></ProtectedRoute>} />
        </Routes>
      </Container>
    </div>
  )
}
