import React, { useState } from 'react'
import { Card, CardActionArea, Box, Typography, Chip, IconButton, Tooltip, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Button, Stack, FormControl, InputLabel, Select, MenuItem } from '@mui/material'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import api from '../api/axios'
import { useAppSelector } from '../store/hooks'

type Photo = {
  photoId: number
  title?: string
  username?: string
  userId?: number
  imageUrl?: string
  uploadedAt?: string
  tags?: string[]
  likesCount?: number
  commentsCount?: number
}

export default function PhotoCard({ photo, onClick, ownerUserId, onDeleted, onUpdated }: { photo: Photo; onClick?: () => void; ownerUserId?: number; onDeleted?: (id: number) => void; onUpdated?: (id: number) => void }) {
  const currentUser = useAppSelector(s => (s as any).user.user)
  const photoOwnerId = ownerUserId ?? (photo as any).userId ?? (photo as any).UserId
  const isOwner = !!(currentUser?.id && photoOwnerId && currentUser.id === photoOwnerId)

  const [editOpen, setEditOpen] = useState(false)
  const [form, setForm] = useState({
    title: photo.title ?? '',
    description: (photo as any).description ?? '',
    tags: Array.isArray(photo.tags) ? (photo.tags as string[]).join(', ') : ''
  })
  const [albums, setAlbums] = useState<Array<{ albumId: number; title: string }>>([])
  const [albumId, setAlbumId] = useState<number | ''>('')
  const [albumsLoading, setAlbumsLoading] = useState(false)

  const handleDelete = async (e: React.MouseEvent) => {
    e.stopPropagation()
    if (!window.confirm('Delete this photo? This cannot be undone.')) return
    try {
      await api.delete(`/api/photos/${photo.photoId}`)
      onDeleted && onDeleted(photo.photoId)
    } catch (err: any) {
      alert(err?.response?.data || err.message || 'Failed to delete photo')
    }
  }

  const handleEditOpen = (e: React.MouseEvent) => {
    e.stopPropagation()
    setForm({
      title: photo.title ?? '',
      description: (photo as any).description ?? '',
      tags: Array.isArray(photo.tags) ? (photo.tags as string[]).join(', ') : ''
    })
    // Initialize album selection if available on photo
    const initialAlbumId = (photo as any).albumId ?? (photo as any).AlbumId
    setAlbumId(initialAlbumId ?? '')
    setEditOpen(true)
    // Lazy-load current user's albums for selection
    if (isOwner && currentUser?.id) {
      setAlbumsLoading(true)
      api.get(`/api/albums/user/${currentUser.id}`)
        .then(res => {
          const list = (res.data || []).map((a: any) => ({
            albumId: a.albumId ?? a.AlbumId,
            title: a.title ?? a.Title
          }))
          setAlbums(list)
        })
        .catch(() => {})
        .finally(() => setAlbumsLoading(false))
    }
  }

  const handleEditSave = async () => {
    try {
      const payload = {
        title: form.title || undefined,
        description: form.description || undefined,
        tags: form.tags ? form.tags.split(',').map(s => s.trim()).filter(Boolean) : [],
        albumId: albumId === '' ? null : albumId
      }
      await api.put(`/api/photos/${photo.photoId}`, payload)
      setEditOpen(false)
      onUpdated && onUpdated(photo.photoId)
    } catch (err: any) {
      alert(err?.response?.data || err.message || 'Failed to update photo')
    }
  }
  // Prefer backend-served image URL when the API provides a filename.
  // prefer server-provided full URL if available
  const imgSrc = (photo as any).fullImageUrl || (
    photo.imageUrl
      ? (
          photo.imageUrl.startsWith('http') || photo.imageUrl.startsWith('data:')
            ? photo.imageUrl
            : `${(api.defaults.baseURL || '').replace(/\/$/, '')}/images/${encodeURIComponent(photo.imageUrl)}`
        )
      : `https://picsum.photos/seed/${photo.photoId}/800/500`
  )

  return (
    <Card sx={{ borderRadius: 2, overflow: 'hidden' }}>
      <CardActionArea sx={{ position: 'relative', height: 240, cursor: onClick ? 'pointer' : 'default' }} onClick={onClick}>
        <Box sx={{ position: 'absolute', inset: 0 }}>
          <Box
            component="img"
            src={imgSrc}
            alt={photo.title ?? 'photo'}
            sx={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }}
            onError={(e: any) => {
              // fallback to picsum if image fails to load
              e.currentTarget.onerror = null
              e.currentTarget.src = `https://picsum.photos/seed/${photo.photoId}/800/500`
            }}
          />
        </Box>

        <Box className="photo-overlay" sx={{ position: 'absolute', inset: 0 }}>
          <Box sx={{ position: 'absolute', left: 12, bottom: 12 }}>
            <Typography variant="subtitle1" sx={{ color: 'white', fontWeight: 700 }}>
              {photo.title ?? 'Untitled'}
            </Typography>
            <Typography
              component="span"
              variant="caption"
              sx={{ color: 'rgba(255,255,255,0.85)', cursor: 'pointer', textDecoration: 'underline' }}
              onClick={(e) => {
                e.stopPropagation()
                const uid = (photo as any).userId ?? (photo as any).UserId
                if (uid) {
                  // navigate without importing useNavigate in this component; fall back to window
                  window.location.href = `/users/${uid}`
                }
              }}
            >
              By {photo.username ?? 'unknown'}
            </Typography>
          </Box>

          <Box sx={{ position: 'absolute', right: 12, bottom: 12, display: 'flex', gap: 1 }}>
            <Chip label={`❤️ ${photo.likesCount ?? 0}`} size="small" sx={{ bgcolor: 'rgba(0,0,0,0.5)', color: 'white' }} />
            <Chip label={`💬 ${photo.commentsCount ?? 0}`} size="small" sx={{ bgcolor: 'rgba(0,0,0,0.5)', color: 'white' }} />
          </Box>

          {isOwner && (
            <Box sx={{ position: 'absolute', right: 8, top: 8, display: 'flex', gap: 1 }} onClick={e => e.stopPropagation()}>
              <Tooltip title="Edit photo">
                <IconButton size="small" sx={{ bgcolor: 'rgba(0,0,0,0.5)', color: 'white' }} onClick={handleEditOpen}>
                  <EditIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Delete photo">
                <IconButton size="small" sx={{ bgcolor: 'rgba(0,0,0,0.5)', color: 'white' }} onClick={handleDelete}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Box>
          )}
        </Box>
      </CardActionArea>

      {/* Edit dialog */}
      <Dialog open={editOpen} onClose={() => setEditOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Edit photo</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label="Title" value={form.title} onChange={e => setForm({ ...form, title: e.target.value })} fullWidth />
            <TextField label="Description" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} fullWidth multiline rows={3} />
            <TextField label="Tags (comma separated)" value={form.tags} onChange={e => setForm({ ...form, tags: e.target.value })} fullWidth />
            {isOwner && (
              <FormControl fullWidth>
                <InputLabel id={`album-select-label-${photo.photoId}`}>Album</InputLabel>
                <Select
                  labelId={`album-select-label-${photo.photoId}`}
                  label="Album"
                  value={albumId}
                  onChange={(e) => setAlbumId(e.target.value as any)}
                  disabled={albumsLoading}
                >
                  <MenuItem value="">No album</MenuItem>
                  {albums.map(a => (
                    <MenuItem key={a.albumId} value={a.albumId}>{a.title}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleEditSave}>Save</Button>
        </DialogActions>
      </Dialog>
    </Card>
  )
}
