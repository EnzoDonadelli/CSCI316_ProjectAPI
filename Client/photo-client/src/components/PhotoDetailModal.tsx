import React, { useEffect, useState } from 'react'
import { Dialog, DialogContent, DialogTitle, Typography, Box, Chip, Stack, Divider, IconButton, Button, TextField, List, ListItem } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import api from '../api/axios'

type Comment = {
  commentId?: number
  userId?: number
  username?: string
  commentText?: string
  commentedAt?: string
}

type PhotoDetail = {
  photoId: number
  title?: string
  username?: string
  description?: string
  imageUrl?: string
  tags?: string[]
  likesCount?: number
  commentsCount?: number
}

export default function PhotoDetailModal({ open, onClose, photoId, initial }: { open: boolean; onClose: () => void; photoId: number | null; initial?: Partial<PhotoDetail> }) {
  const [photo, setPhoto] = useState<PhotoDetail | null>(initial ? (initial as PhotoDetail) : null)
  const [comments, setComments] = useState<Comment[]>([])
  const [loading, setLoading] = useState(false)
  const [liked, setLiked] = useState<boolean>(false)
  const [likeLoading, setLikeLoading] = useState(false)
  const [newComment, setNewComment] = useState('')

  useEffect(() => {
    if (!open || !photoId) return

    let cancelled = false

    const load = async () => {
      setLoading(true)
      try {
        // Try to fetch full photo details
        const p = await api.get(`/api/photos/${photoId}`)
        if (!cancelled) setPhoto(Array.isArray(p.data) ? p.data[0] : p.data.value ?? p.data)

        // Try to fetch comments. Try a couple common endpoints gracefully.
        try {
          const c1 = await api.get(`/api/photos/${photoId}/comments`)
          setComments(Array.isArray(c1.data) ? c1.data : (c1.data.value ?? c1.data))
        } catch (e1) {
          try {
            const c2 = await api.get(`/api/comments/photo/${photoId}`)
            setComments(Array.isArray(c2.data) ? c2.data : (c2.data.value ?? c2.data))
          } catch (e2) {
            // fallback: empty
            setComments([])
          }
        }
        // Check whether demo user (id=1) liked this photo
        try {
          const resp = await api.get(`/api/likes/photo/${photoId}/user/1/check`)
          if (resp && resp.data !== undefined) setLiked(Boolean(resp.data))
        } catch (e) {
          // ignore
        }
      } catch (err) {
        // If photo detail endpoint not available, keep initial
        console.warn('Could not load photo details', err)
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    load()

    return () => { cancelled = true }
  }, [open, photoId])

  const imgSrc = photo?.imageUrl
    ? (photo.imageUrl.startsWith('http') ? photo.imageUrl : `${(api.defaults.baseURL || '').replace(/\/$/, '')}/images/${encodeURIComponent(photo.imageUrl)}`)
    : (photo ? `https://picsum.photos/seed/${photo.photoId}/1200/800` : undefined)

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box>
          <Typography variant="h6">{photo?.title ?? 'Photo'}</Typography>
          <Typography variant="caption" sx={{ color: 'text.secondary' }}>By {photo?.username ?? 'unknown'}</Typography>
          {photo?.imageUrl && (
            <Typography variant="caption" sx={{ color: 'text.secondary', display: 'block' }}>{photo.imageUrl}</Typography>
          )}
        </Box>
        <IconButton onClick={onClose}><CloseIcon sx={{ color: 'text.primary' }} /></IconButton>
      </DialogTitle>
      <DialogContent>
        <Box sx={{ display: 'flex', gap: 3, flexDirection: { xs: 'column', md: 'row' } }}>
          <Box sx={{ flex: 2 }}>
            {imgSrc && (
              <Box component="img" src={imgSrc} alt={photo?.title ?? 'photo'} sx={{ width: '100%', borderRadius: 1 }} />
            )}
            <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
              {photo?.tags?.map(t => <Chip key={t} label={t} size="small" sx={{ bgcolor: 'rgba(255,255,255,0.06)', color: 'white' }} />)}
            </Stack>
          </Box>

          <Box sx={{ flex: 1 }}>
            <Typography variant="subtitle1" sx={{ mb: 1 }}>Details</Typography>
            <Typography variant="body2" sx={{ mb: 2 }}>{photo?.description ?? 'No description'}</Typography>

            <Divider sx={{ my: 2 }} />

            <Stack direction="row" spacing={2} alignItems="center">
              <Typography variant="subtitle2">Likes</Typography>
              <Typography variant="body2">{photo?.likesCount ?? 0}</Typography>
              <Button variant={liked ? 'contained' : 'outlined'} color="primary" size="small" onClick={async () => {
                if (!photoId) return
                setLikeLoading(true)
                try {
                  if (!liked) {
                    const r = await api.post(`/api/likes/photo/${photoId}/user/1`)
                    // optimistic: increment
                    setPhoto(prev => prev ? { ...prev, likesCount: (prev.likesCount ?? 0) + 1 } : prev)
                    setLiked(true)
                  } else {
                    await api.delete(`/api/likes/photo/${photoId}/user/1`)
                    setPhoto(prev => prev ? { ...prev, likesCount: Math.max(0, (prev.likesCount ?? 1) - 1) } : prev)
                    setLiked(false)
                  }
                } catch (e) {
                  console.error('like failed', e)
                } finally { setLikeLoading(false) }
              }}>{liked ? 'Liked' : 'Like'}</Button>
            </Stack>

            <Divider sx={{ my: 2 }} />

            <Typography variant="subtitle2">Comments</Typography>
            {loading && <Typography>Loading comments...</Typography>}
            {!loading && comments.length === 0 && <Typography variant="body2">No comments yet.</Typography>}
            <List sx={{ maxHeight: 220, overflow: 'auto', mb: 1 }}>
              {comments.map(c => (
                <ListItem key={c.commentId} sx={{ bgcolor: 'rgba(255,255,255,0.03)', borderRadius: 1, mb: 1 }}>
                  <Box>
                    <Typography variant="subtitle2">{c.username ?? 'user'}</Typography>
                    <Typography variant="body2">{c.commentText}</Typography>
                  </Box>
                </ListItem>
              ))}
            </List>

            <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
              <TextField placeholder="Add a comment" fullWidth size="small" value={newComment} onChange={e => setNewComment(e.target.value)} />
              <Button variant="contained" onClick={async () => {
                if (!photoId || !newComment.trim()) return
                try {
                  const payload = { commentText: newComment.trim() }
                  const res = await api.post(`/api/comments/photo/${photoId}/user/1`, payload)
                  const created = res.data
                  // normalize response shape
                  const createdObj = Array.isArray(created) ? created[0] : (created.value ?? created)
                  setComments(prev => [createdObj, ...prev])
                  setPhoto(prev => prev ? { ...prev, commentsCount: (prev.commentsCount ?? 0) + 1 } : prev)
                  setNewComment('')
                } catch (e) {
                  console.error('comment failed', e)
                }
              }}>Post</Button>
            </Stack>
          </Box>
        </Box>
      </DialogContent>
    </Dialog>
  )
}
