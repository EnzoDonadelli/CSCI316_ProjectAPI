import React, { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { Box, Typography, Grid, Breadcrumbs, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Stack } from '@mui/material'
import api from '../api/axios'
import PhotoCard from '../components/PhotoCard'
import PhotoDetailModal from '../components/PhotoDetailModal'
import { useAppSelector } from '../store/hooks'

export default function AlbumPage() {
  const { id } = useParams()
  const albumId = id ? parseInt(id) : null

  const [album, setAlbum] = useState<any | null>(null)
  const [photos, setPhotos] = useState<any[]>([])
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const currentUser = useAppSelector(s => (s as any).user.user)
  const isOwner = !!(currentUser?.id && album?.userId && currentUser.id === album.userId)

  const [editOpen, setEditOpen] = useState(false)
  const [form, setForm] = useState({ title: '', description: '' })

  useEffect(() => {
    if (!albumId) return
    const load = async () => {
      try {
        const a = await api.get(`/api/albums/${albumId}`)
        const d = a.data || {}
        setAlbum({
          albumId: d.albumId ?? d.AlbumId,
          title: d.title ?? d.Title,
          description: d.description ?? d.Description,
          userId: d.userId ?? d.UserId,
          username: d.username ?? d.Username
        })
      } catch {}
      try {
        const p = await api.get(`/api/albums/${albumId}/photos`)
        const normalized = (p.data || []).map((ph: any) => ({
          photoId: ph.photoId ?? ph.PhotoId,
          title: ph.title ?? ph.Title,
          username: ph.username ?? ph.Username,
          imageUrl: ph.imageUrl ?? ph.ImageUrl,
          fullImageUrl: ph.fullImageUrl ?? ph.FullImageUrl,
          userId: ph.userId ?? ph.UserId,
          likesCount: ph.likesCount ?? ph.LikesCount,
          commentsCount: ph.commentsCount ?? ph.CommentsCount,
          albumId: ph.albumId ?? ph.AlbumId
        }))
        setPhotos(normalized)
      } catch {}
    }
    load()
  }, [albumId])

  return (
    <Box>
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link to="/feed">Feed</Link>
        {album?.userId && <Link to={`/users/${album.userId}`}>{album.username || 'Author'}</Link>}
        <Typography color="text.primary">Album</Typography>
      </Breadcrumbs>
      <Typography variant="h5" sx={{ fontWeight: 700 }}>{album?.title || 'Album'}</Typography>
      {album?.description && (
        <Typography variant="body2" sx={{ color: 'text.secondary', mb: 3 }}>{album.description}</Typography>
      )}
      {isOwner && (
        <Box sx={{ mb: 2 }}>
          <Button
            variant="outlined"
            onClick={() => {
              setForm({ title: album?.title || '', description: album?.description || '' })
              setEditOpen(true)
            }}
          >Edit album</Button>
        </Box>
      )}

      <Grid container spacing={2}>
        {photos.map(ph => (
          <Grid item xs={12} sm={6} md={4} key={ph.photoId}>
            <PhotoCard
              photo={ph}
              ownerUserId={album?.userId}
              onClick={() => setSelectedId(ph.photoId)}
              onDeleted={(pid) => setPhotos(prev => prev.filter(p => p.photoId !== pid))}
              onUpdated={async () => {
                try {
                  const p = await api.get(`/api/albums/${albumId}/photos`)
                  const normalized = (p.data || []).map((ph2: any) => ({
                    photoId: ph2.photoId ?? ph2.PhotoId,
                    title: ph2.title ?? ph2.Title,
                    username: ph2.username ?? ph2.Username,
                    imageUrl: ph2.imageUrl ?? ph2.ImageUrl,
                    fullImageUrl: ph2.fullImageUrl ?? ph2.FullImageUrl,
                    userId: ph2.userId ?? ph2.UserId,
                    likesCount: ph2.likesCount ?? ph2.LikesCount,
                    commentsCount: ph2.commentsCount ?? ph2.CommentsCount,
                    albumId: ph2.albumId ?? ph2.AlbumId
                  }))
                  setPhotos(normalized)
                } catch {}
              }}
            />
          </Grid>
        ))}
      </Grid>

      <PhotoDetailModal open={!!selectedId} photoId={selectedId} onClose={() => setSelectedId(null)} initial={undefined} />

      {/* Edit album dialog */}
      <Dialog open={editOpen} onClose={() => setEditOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Edit album</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label="Title" value={form.title} onChange={e => setForm({ ...form, title: e.target.value })} fullWidth />
            <TextField label="Description" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} fullWidth multiline rows={3} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={async () => {
              if (!albumId) return
              try {
                await api.put(`/api/albums/${albumId}`, {
                  title: form.title || undefined,
                  description: form.description || undefined
                })
                setAlbum((prev: any) => prev ? { ...prev, title: form.title, description: form.description } : prev)
                setEditOpen(false)
              } catch (err: any) {
                alert(err?.response?.data || err.message || 'Failed to update album')
              }
            }}
          >Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
