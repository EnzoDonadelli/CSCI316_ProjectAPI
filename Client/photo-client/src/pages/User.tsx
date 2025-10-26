import React, { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { Typography, Box, Avatar, Grid, Card, CardContent } from '@mui/material'
import PhotoCard from '../components/PhotoCard'
import PhotoDetailModal from '../components/PhotoDetailModal'
import { useState } from 'react'

export default function User() {
  const { id } = useParams()

  useEffect(() => {
    // In a real app we'd fetch user and albums by id. For now we show dummy John Doe data.
  }, [id])

  // Dummy John Doe data
  const user = {
    id: 1,
    username: 'johndoe',
    fullName: 'John Doe',
    bio: 'Landscape photographer.',
    profilePic: 'https://picsum.photos/seed/johndoe/120/120'
  }

  const albums = [
    {
      albumId: 1,
      title: 'Nature Escapes',
      description: 'Collection of stunning landscape shots.',
      photos: [
        {
          photoId: 101,
          title: 'Mountain Sunrise',
          username: 'johndoe',
          imageUrl: 'https://picsum.photos/seed/mountain/800/500',
          likesCount: 12,
          commentsCount: 4
        }
      ]
    },
    {
      albumId: 2,
      title: 'City Walks',
      description: 'Street and urban photography.',
      photos: [
        {
          photoId: 102,
          title: 'Neon Night',
          username: 'johndoe',
          imageUrl: 'https://picsum.photos/seed/neon/800/500',
          likesCount: 8,
          commentsCount: 2
        }
      ]
    }
  ]

  const [selectedId, setSelectedId] = useState<number | null>(null)

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 3 }}>
        <Avatar src={user.profilePic} sx={{ width: 96, height: 96 }} />
        <Box>
          <Typography variant="h5">{user.fullName}</Typography>
          <Typography variant="subtitle2" sx={{ color: 'text.secondary' }}>@{user.username}</Typography>
          <Typography sx={{ mt: 1 }}>{user.bio}</Typography>
        </Box>
      </Box>

      <Typography variant="h6" sx={{ mb: 2 }}>Albums</Typography>

      <Grid container spacing={2}>
        {albums.map(album => (
          <Grid item xs={12} md={6} key={album.albumId}>
            <Card>
              <CardContent>
                <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>{album.title}</Typography>
                <Typography variant="body2" sx={{ mb: 2 }}>{album.description}</Typography>
                <Grid container spacing={1}>
                  {album.photos.map(photo => (
                    <Grid item xs={12} sm={6} key={photo.photoId}>
                      <PhotoCard photo={photo} onClick={() => setSelectedId(photo.photoId)} />
                    </Grid>
                  ))}
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
      <PhotoDetailModal open={!!selectedId} photoId={selectedId ?? null} onClose={() => setSelectedId(null)} initial={undefined} />
    </Box>
  )
}
