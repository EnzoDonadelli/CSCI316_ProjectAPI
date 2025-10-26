import React from 'react'
import { Card, CardActionArea, Box, Typography, Chip } from '@mui/material'
import api from '../api/axios'

type Photo = {
  photoId: number
  title?: string
  username?: string
  imageUrl?: string
  uploadedAt?: string
  tags?: string[]
  likesCount?: number
  commentsCount?: number
}

export default function PhotoCard({ photo, onClick }: { photo: Photo; onClick?: () => void }) {
  // Prefer backend-served image URL when the API provides a filename.
  const imgSrc = photo.imageUrl
    ? (photo.imageUrl.startsWith('http') ? photo.imageUrl : `${(api.defaults.baseURL || '').replace(/\/$/, '')}/images/${encodeURIComponent(photo.imageUrl)}`)
    : `https://picsum.photos/seed/${photo.photoId}/800/500`

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
            <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.85)' }}>
              By {photo.username ?? 'unknown'}
            </Typography>
          </Box>

          <Box sx={{ position: 'absolute', right: 12, bottom: 12, display: 'flex', gap: 1 }}>
            <Chip label={`❤️ ${photo.likesCount ?? 0}`} size="small" sx={{ bgcolor: 'rgba(0,0,0,0.5)', color: 'white' }} />
            <Chip label={`💬 ${photo.commentsCount ?? 0}`} size="small" sx={{ bgcolor: 'rgba(0,0,0,0.5)', color: 'white' }} />
          </Box>
        </Box>
      </CardActionArea>
    </Card>
  )
}
