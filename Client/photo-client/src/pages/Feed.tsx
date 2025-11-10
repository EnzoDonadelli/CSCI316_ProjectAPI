import React, { useEffect, useState } from 'react'
import { useAppDispatch, useAppSelector } from '../store/hooks'
import { fetchPhotos } from '../store/photoSlice'
import { Typography, Stack, Card, Grid, TextField, InputAdornment, IconButton, Box, Button } from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import PhotoCard from '../components/PhotoCard'
import PhotoDetailModal from '../components/PhotoDetailModal'
import api from '../api/axios'

export default function Feed() {
  const dispatch = useAppDispatch()
  const { photos, loading } = useAppSelector(s => s.photos)
  const [localPhotos, setLocalPhotos] = useState<any[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedId, setSelectedId] = useState<number | null>(null)

  useEffect(() => {
    // Personalized feed (followed users). Fallback to all photos if endpoint fails.
    const load = async () => {
      try {
        const res = await api.get('/api/photos/feed')
        const data = Array.isArray(res.data) ? res.data : (res.data.value ?? res.data)
        const sorted = [...data].sort((a: any, b: any) => {
          const la = (a.likesCount ?? a.LikesCount ?? 0)
          const lb = (b.likesCount ?? b.LikesCount ?? 0)
          if (lb !== la) return lb - la
          return (b.photoId ?? b.PhotoId ?? 0) - (a.photoId ?? a.PhotoId ?? 0)
        })
        setLocalPhotos(sorted)
      } catch {
        dispatch(fetchPhotos())
      }
    }
    load()
  }, [dispatch])

  useEffect(() => {
    // photos may already be sorted by API; enforce likes desc fallback
    const sorted = [...photos].sort((a: any, b: any) => {
      const la = (a.likesCount ?? a.LikesCount ?? 0)
      const lb = (b.likesCount ?? b.LikesCount ?? 0)
      if (lb !== la) return lb - la
      // fallback stable ordering by photoId desc
      return (b.photoId ?? b.PhotoId ?? 0) - (a.photoId ?? a.PhotoId ?? 0)
    })
    setLocalPhotos(sorted)
  }, [photos])

  const onSearch = async () => {
    const q = searchTerm.trim()
    if (!q) {
      setLocalPhotos(photos)
      return
    }

    try {
      // Discovery mode: exclude photos from followed users
      const res = await api.get(`/api/photos/tag/${encodeURIComponent(q)}?limit=50&excludeFollowed=true`)
      // API returns an object { value: [...], Count: n } in some responses or an array; normalize
      const data = Array.isArray(res.data) ? res.data : (res.data.value ?? res.data)
      const sorted = [...data].sort((a: any, b: any) => {
        const la = (a.likesCount ?? a.LikesCount ?? 0)
        const lb = (b.likesCount ?? b.LikesCount ?? 0)
        if (lb !== la) return lb - la
        return (b.photoId ?? b.PhotoId ?? 0) - (a.photoId ?? a.PhotoId ?? 0)
      })
      setLocalPhotos(sorted)
    } catch (err) {
      console.error('search error', err)
    }
  }

  const clearSearch = () => {
    setSearchTerm('')
      const sorted = [...photos].sort((a: any, b: any) => {
        const la = (a.likesCount ?? a.LikesCount ?? 0)
        const lb = (b.likesCount ?? b.LikesCount ?? 0)
        if (lb !== la) return lb - la
        return (b.photoId ?? b.PhotoId ?? 0) - (a.photoId ?? a.PhotoId ?? 0)
      })
      setLocalPhotos(sorted)
  }

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>Feed</Typography>

      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <TextField
          placeholder="Search by tag (e.g. Landscape, Wedding)"
          value={searchTerm}
          onChange={e => setSearchTerm(e.target.value)}
          fullWidth
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon sx={{ color: 'rgba(255,255,255,0.7)' }} />
              </InputAdornment>
            )
          }}
        />
        <Button variant="contained" color="primary" onClick={onSearch}>Search</Button>
        <Button variant="outlined" color="inherit" onClick={clearSearch}>Clear</Button>
      </Box>

      {loading && <Typography>Loading...</Typography>}

      <Grid container spacing={2}>
        {localPhotos.map(p => (
          <Grid item xs={12} sm={6} md={4} lg={3} key={p.photoId}>
            <PhotoCard
              photo={p}
              onClick={() => { setSelectedId(p.photoId) }}
              onDeleted={(id) => {
                setLocalPhotos(prev => prev.filter(ph => ph.photoId !== id))
              }}
              onUpdated={async () => {
                // Refresh current view (respect search if active)
                if (searchTerm.trim()) await onSearch()
                else dispatch(fetchPhotos())
              }}
            />
          </Grid>
        ))}
      </Grid>

      <PhotoDetailModal open={!!selectedId} photoId={selectedId ?? null} onClose={() => setSelectedId(null)} initial={undefined} />
    </Box>
  )
}

// add selectedId state at top
