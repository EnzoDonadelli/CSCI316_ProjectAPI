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
    dispatch(fetchPhotos())
  }, [dispatch])

  useEffect(() => {
    setLocalPhotos(photos)
  }, [photos])

  const onSearch = async () => {
    const q = searchTerm.trim()
    if (!q) {
      setLocalPhotos(photos)
      return
    }

    try {
      const res = await api.get(`/api/photos/tag/${encodeURIComponent(q)}?limit=50`)
      // API returns an object { value: [...], Count: n } in some responses or an array; normalize
      const data = Array.isArray(res.data) ? res.data : (res.data.value ?? res.data)
      setLocalPhotos(data)
    } catch (err) {
      console.error('search error', err)
    }
  }

  const clearSearch = () => {
    setSearchTerm('')
    setLocalPhotos(photos)
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
            <PhotoCard photo={p} onClick={() => { setSelectedId(p.photoId) }} />
          </Grid>
        ))}
      </Grid>

      <PhotoDetailModal open={!!selectedId} photoId={selectedId ?? null} onClose={() => setSelectedId(null)} initial={undefined} />
    </Box>
  )
}

// add selectedId state at top
