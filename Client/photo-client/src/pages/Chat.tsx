import React, { useEffect, useMemo, useRef, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { Box, Typography, TextField, Button, Paper, Avatar } from '@mui/material'
import { useAppSelector } from '../store/hooks'
import api from '../api/axios'

function storageKey(a: number, b: number) {
  const [x, y] = a <= b ? [a, b] : [b, a]
  return `chat:${x}:${y}`
}

export default function Chat() {
  const { id } = useParams()
  const otherId = id ? parseInt(id) : NaN
  const me = useAppSelector(s => (s as any).user?.user)
  const [input, setInput] = useState('')
  const [messages, setMessages] = useState<Array<{ from: number; text: string; at: number }>>([])
  const listRef = useRef<HTMLDivElement | null>(null)
  const [otherUser, setOtherUser] = useState<any | null>(null)

  const key = useMemo(() => {
    if (!me?.id || !otherId) return ''
    return storageKey(me.id, otherId)
  }, [me?.id, otherId])

  // Load other user's display info
  useEffect(() => {
    const load = async () => {
      if (!otherId) return
      try {
        const res = await api.get(`/api/users/${otherId}`)
        const d = res.data || {}
        const normalized = {
          id: d.userId ?? d.UserId ?? d.id,
          username: d.username ?? d.Username,
          fullName: d.fullName ?? d.FullName,
          profilePic: d.profilePic ?? d.ProfilePic
        }
        setOtherUser(normalized)
      } catch {
        setOtherUser(null)
      }
    }
    load()
  }, [otherId])

  useEffect(() => {
    if (!key) return
    try {
      const raw = sessionStorage.getItem(key)
      const arr = raw ? JSON.parse(raw) : []
      setMessages(arr)
    } catch {}
  }, [key])

  useEffect(() => {
    if (listRef.current) {
      listRef.current.scrollTop = listRef.current.scrollHeight
    }
  }, [messages])

  const send = () => {
    if (!me?.id || !otherId || !key) return
    const text = input.trim()
    if (!text) return
    const msg = { from: me.id, text, at: Date.now() }
    const next = [...messages, msg]
    setMessages(next)
    try { sessionStorage.setItem(key, JSON.stringify(next)) } catch {}
    setInput('')
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <Avatar src={otherUser?.profilePic} sx={{ width: 36, height: 36 }} />
        <Box>
          <Typography variant="h6">{otherUser?.fullName ?? otherUser?.username ?? 'Unknown User'}</Typography>
          <Typography variant="caption" sx={{ color: 'text.secondary' }}>Chat (session-based)</Typography>
        </Box>
      </Box>
      <Box sx={{ mb: 2 }}>
        <Button component={Link} to={`/users/${otherId}`}>Back to Profile</Button>
      </Box>
      <Paper variant="outlined" sx={{ height: 400, overflowY: 'auto', p: 2 }} ref={listRef}>
        {messages.map((m, idx) => (
          <Box key={idx} sx={{ display: 'flex', justifyContent: m.from === me?.id ? 'flex-end' : 'flex-start', mb: 1 }}>
            <Box sx={{ bgcolor: m.from === me?.id ? 'primary.main' : 'grey.300', color: m.from === me?.id ? 'primary.contrastText' : 'text.primary', px: 1.5, py: 1, borderRadius: 1, maxWidth: '70%' }}>
              <Typography variant="body2">{m.text}</Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>{new Date(m.at).toLocaleTimeString()}</Typography>
            </Box>
          </Box>
        ))}
      </Paper>
      <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
        <TextField fullWidth value={input} onChange={e => setInput(e.target.value)} placeholder="Type a message" onKeyDown={e => { if (e.key === 'Enter') send() }} />
        <Button variant="contained" onClick={send}>Send</Button>
      </Box>
    </Box>
  )
}
