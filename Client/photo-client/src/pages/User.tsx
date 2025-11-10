import React, { useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { Typography, Box, Avatar, Grid, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField } from '@mui/material'
import PhotoCard from '../components/PhotoCard'
import PhotoDetailModal from '../components/PhotoDetailModal'
import { useState } from 'react'
import api from '../api/axios'
import { useAppDispatch, useAppSelector } from '../store/hooks'
import { setUser as setUserInStore } from '../store/userSlice'

export default function User() {
  const { id } = useParams()
  const currentUser = useAppSelector(s => (s as any).user.user)
  const dispatch = useAppDispatch()

  const [user, setUser] = useState<any | null>(null)
  const [photos, setPhotos] = useState<any[]>([])
  const [albums, setAlbums] = useState<any[]>([])

  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [openAlbumDialog, setOpenAlbumDialog] = useState(false)
  const [openPhotoDialog, setOpenPhotoDialog] = useState(false)
  const [openEdit, setOpenEdit] = useState(false)
  const [editFullName, setEditFullName] = useState('')
  const [editBio, setEditBio] = useState('')
  const [editProfilePic, setEditProfilePic] = useState('')
  const [stats, setStats] = useState<{ followers: number; following: number }>({ followers: 0, following: 0 })
  const [isFollowing, setIsFollowing] = useState<boolean | null>(null)
  const [followBusy, setFollowBusy] = useState(false)
  const [openFollowers, setOpenFollowers] = useState(false)
  const [openFollowing, setOpenFollowing] = useState(false)
  const [followersList, setFollowersList] = useState<any[]>([])
  const [followingList, setFollowingList] = useState<any[]>([])

  // Inline form to create an album
  function CreateAlbumForm({ userId, onCreated }: { userId: number; onCreated?: () => void }) {
    const [title, setTitle] = useState('')
    const [description, setDescription] = useState('')
    const [loading, setLoading] = useState(false)

    const submit = async () => {
      if (!title || title.length < 1) return alert('Please provide an album title')
      setLoading(true)
      try {
        await api.post(`/api/albums/user/${userId}`, { title, description })
        setTitle('')
        setDescription('')
        onCreated && onCreated()
      } catch (e: any) {
        alert(e?.response?.data || e.message || 'Error creating album')
      } finally {
        setLoading(false)
      }
    }

    return (
      <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', mt: 1 }}>
        <input value={title} onChange={e=>setTitle(e.target.value)} placeholder="Album title" style={{padding:8,flex:1}} />
        <input value={description} onChange={e=>setDescription(e.target.value)} placeholder="Description (optional)" style={{padding:8,flex:2}} />
        <button onClick={submit} disabled={loading} style={{padding:'8px 12px'}}>Create</button>
      </Box>
    )
  }

  // Inline form to create a photo (supports image URL or file->dataURL)
  function CreatePhotoForm({ userId, albums, onCreated }: { userId: number; albums: any[]; onCreated?: () => void }) {
    const [title, setTitle] = useState('')
    const [description, setDescription] = useState('')
    const [albumId, setAlbumId] = useState<number | null>(albums?.[0]?.albumId ?? null)
    const [imageMode, setImageMode] = useState<'url'|'file'>('file')
    const [imageUrl, setImageUrl] = useState('')
    const [filePreviews, setFilePreviews] = useState<Array<{ name: string; dataUrl: string }>>([])
    const [tagsStr, setTagsStr] = useState('')
    const [loading, setLoading] = useState(false)

    const readFileAsDataUrl = (f: File) => new Promise<string>((res, rej) => {
      const reader = new FileReader()
      reader.onerror = () => rej(new Error('Failed to read file'))
      reader.onload = () => res(reader.result as string)
      reader.readAsDataURL(f)
    })

    const onFilesChange = async (files?: FileList | null) => {
      if (!files || files.length === 0) return setFilePreviews([])
      try {
        const arr = Array.from(files)
        const mapped = await Promise.all(arr.map(async f => ({ name: f.name, dataUrl: await readFileAsDataUrl(f) })))
        setFilePreviews(mapped)
      } catch (e) {
        alert('Failed to read file(s)')
      }
    }

    const removePreview = (name: string) => setFilePreviews(p => p.filter(x => x.name !== name))

    const submit = async () => {
      // if in URL mode, we can upload a single URL as one photo; in file mode allow multiple
      const tags = tagsStr ? tagsStr.split(',').map(s => s.trim()).filter(Boolean) : []
      if (imageMode === 'url') {
        if (!imageUrl) return alert('Please provide an image URL')
        setLoading(true)
        try {
          const payload = { albumId: albumId ?? undefined, title, description, imageUrl, tags }
          await api.post(`/api/photos/user/${userId}`, payload)
          setTitle(''); setDescription(''); setImageUrl(''); setTagsStr('')
          onCreated && onCreated()
        } catch (e: any) {
          alert(e?.response?.data || e.message || 'Error creating photo')
        } finally { setLoading(false) }
        return
      }

      if (filePreviews.length === 0) return alert('Please choose one or more image files')
      setLoading(true)
      try {
        // upload sequentially to avoid huge parallel spikes; backend creates one photo per request
        for (const file of filePreviews) {
          const payload = {
            albumId: albumId ?? undefined,
            title: title || file.name,
            description,
            imageUrl: file.dataUrl,
            tags
          }
          await api.post(`/api/photos/user/${userId}`, payload)
        }
        setTitle(''); setDescription(''); setFilePreviews([]); setTagsStr('')
        onCreated && onCreated()
      } catch (e: any) {
        alert(e?.response?.data || e.message || 'Error creating photo(s)')
      } finally { setLoading(false) }
    }

    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        <input value={title} onChange={e => setTitle(e.target.value)} placeholder="Photo title (optional)" />
        <input value={description} onChange={e => setDescription(e.target.value)} placeholder="Description (optional)" />
        <select value={albumId ?? ''} onChange={e => setAlbumId(e.target.value ? parseInt(e.target.value) : null)}>
          <option value="">No album</option>
          {albums.map(a => <option key={a.albumId} value={a.albumId}>{a.title}</option>)}
        </select>

        <Box>
          <label style={{ marginRight: 8 }}>
            <input type="radio" checked={imageMode === 'url'} onChange={() => setImageMode('url')} /> Image URL
          </label>
          <label>
            <input type="radio" checked={imageMode === 'file'} onChange={() => setImageMode('file')} /> Upload files
          </label>
        </Box>

        {imageMode === 'url' ? (
          <input value={imageUrl} onChange={e => setImageUrl(e.target.value)} placeholder="https://... or data:image/..." />
        ) : (
          <input type="file" multiple accept="image/*" onChange={e => onFilesChange(e.target.files)} />
        )}

        {filePreviews.length > 0 && (
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 1 }}>
            {filePreviews.map(f => (
              <Box key={f.name} sx={{ width: 120, border: '1px solid #ddd', p: 1, position: 'relative' }}>
                <img src={f.dataUrl} alt={f.name} style={{ width: '100%', height: 80, objectFit: 'cover' }} />
                <Typography variant="caption" sx={{ display: 'block', wordBreak: 'break-all' }}>{f.name}</Typography>
                <button onClick={() => removePreview(f.name)} style={{ position: 'absolute', right: 4, top: 4 }}>x</button>
              </Box>
            ))}
          </Box>
        )}

        <input value={tagsStr} onChange={e => setTagsStr(e.target.value)} placeholder="tags (comma separated)" />

        <Box>
          <button onClick={submit} disabled={loading}>{loading ? 'Uploading...' : 'Upload'}</button>
        </Box>
      </Box>
    )
  }
  useEffect(() => {
    const load = async () => {
      // decide target user id: route param or current logged-in user
      const targetId = id ? parseInt(id) : currentUser?.id
      if (!targetId) return

      try {
        // If we're viewing our own profile and have the user in store, reuse it
        if (currentUser && targetId === currentUser.id) {
          // reuse normalized currentUser from store
          setUser({ ...currentUser, id: currentUser.id })
        } else {
          const res = await api.get(`/api/users/${targetId}`)
          const d = res.data || {}
          // normalize backend DTO (userId / UserId) -> id
          const normalizedUser = {
            id: d.userId ?? d.UserId ?? d.id,
            username: d.username ?? d.Username,
            fullName: d.fullName ?? d.FullName,
            bio: d.bio ?? d.Bio,
            profilePic: d.profilePic ?? d.ProfilePic,
            email: d.email ?? d.Email,
            createdAt: d.createdAt ?? d.CreatedAt
          }
          setUser(normalizedUser)
        }

  // fetch photos by user
  const p = await api.get(`/api/photos/user/${targetId}`)
        // normalize & already sorted by API (likes desc). If API doesn't sort, enforce it here.
        const normalized = (p.data || []).map((ph: any) => ({
          photoId: ph.photoId ?? ph.PhotoId,
          title: ph.title ?? ph.Title,
          username: ph.username ?? ph.Username,
          imageUrl: ph.imageUrl ?? ph.ImageUrl,
          likesCount: ph.likesCount ?? ph.LikesCount ?? 0,
          commentsCount: ph.commentsCount ?? ph.CommentsCount ?? 0
        }))
        normalized.sort((a: any, b: any) => (b.likesCount - a.likesCount) || (b.photoId - a.photoId))
        setPhotos(normalized)
        // fetch follower stats
        try {
          const s = await api.get(`/api/followers/${targetId}/stats`)
          setStats({ followers: s.data?.followersCount ?? s.data?.FollowersCount ?? 0, following: s.data?.followingCount ?? s.data?.FollowingCount ?? 0 })
        } catch {}

        // fetch follow status (only when viewing another user's profile and logged in)
        try {
          if (currentUser && currentUser.id !== targetId) {
            // enable button immediately while loading status
            if (isFollowing === null) setIsFollowing(false)
            const r = await api.get(`/api/followers/${currentUser.id}/follows/${targetId}`)
            const follows = r.data === true || r.data === 'true' || r.data?.follows === true || r.data?.Follows === true
            setIsFollowing(!!follows)
          } else {
            setIsFollowing(null)
          }
        } catch {
          // if status check fails, keep it enabled as not-following so the user can try
          setIsFollowing(false)
        }

        // fetch albums by user
        try {
          const a = await api.get(`/api/albums/user/${targetId}`)
          const normalizedAlbums = (a.data || []).map((al: any) => ({
            albumId: al.albumId ?? al.AlbumId,
            title: al.title ?? al.Title,
            description: al.description ?? al.Description
          }))
          setAlbums(normalizedAlbums)
        } catch (e) {
          // ignore
        }
      } catch (err) {
        // ignore errors for now; UI will show empty state
        console.error('Error loading user page', err)
      }
    }

    load()
  }, [id, currentUser])

  // Keep edit form in sync with loaded user
  useEffect(() => {
    if (user) {
      setEditFullName(user.fullName ?? '')
      setEditBio(user.bio ?? '')
      setEditProfilePic(user.profilePic ?? '')
    }
  }, [user])

  const saveProfile = async () => {
    try {
      const payload: any = {
        fullName: editFullName || undefined,
        bio: editBio, // allow empty string to clear
        profilePic: editProfilePic // allow empty string to clear
      }
      await api.put('/api/auth/profile', payload)
      const updated = { ...(user || {}), fullName: editFullName, bio: editBio, profilePic: editProfilePic }
      setUser(updated)
      // If editing own profile, update global store
      if (currentUser && (currentUser.id === updated.id || currentUser.id === updated.userId || currentUser.id === updated.UserId)) {
        dispatch(setUserInStore(updated))
      }
      setOpenEdit(false)
    } catch (e: any) {
      alert(e?.response?.data?.message || e.message || 'Failed to update profile')
    }
  }

  const targetUserId = (user?.id ?? user?.userId ?? user?.UserId) as number | undefined
  const viewingOwnProfile = !!(currentUser && targetUserId && currentUser.id === targetUserId)

  const refreshFollowState = async () => {
    if (!targetUserId) return
    try {
      const s = await api.get(`/api/followers/${targetUserId}/stats`)
      setStats({ followers: s.data?.followersCount ?? s.data?.FollowersCount ?? 0, following: s.data?.followingCount ?? s.data?.FollowingCount ?? 0 })
    } catch {}
    try {
      if (currentUser && currentUser.id !== targetUserId) {
        const r = await api.get(`/api/followers/${currentUser.id}/follows/${targetUserId}`)
        const follows = r.data === true || r.data === 'true' || r.data?.follows === true || r.data?.Follows === true
        setIsFollowing(!!follows)
      }
    } catch {}
    // if followers dialog is open, refresh list to reflect DB
    try {
      if (openFollowers) {
        const r = await api.get(`/api/followers/${targetUserId}/followers`)
        setFollowersList(r.data || [])
      }
    } catch {}
  }

  const handleFollow = async () => {
    if (!currentUser || !targetUserId) return
    setFollowBusy(true)
    try {
      await api.post(`/api/followers/${currentUser.id}/follow/${targetUserId}`)
      await refreshFollowState()
    } catch (e: any) {
      const msg = e?.response?.data || e.message || 'Failed to follow user'
      if (typeof msg === 'string' && msg.toLowerCase().includes('already')) {
        // already following; sync state from server
        await refreshFollowState()
      } else {
        alert(msg)
      }
    } finally {
      setFollowBusy(false)
    }
  }

  const handleUnfollow = async () => {
    if (!currentUser || !targetUserId) return
    setFollowBusy(true)
    try {
      await api.delete(`/api/followers/${currentUser.id}/unfollow/${targetUserId}`)
      await refreshFollowState()
    } catch (e: any) {
      const msg = e?.response?.data || e.message || 'Failed to unfollow user'
      if (typeof msg === 'string' && msg.toLowerCase().includes('not found')) {
        // relationship already gone; sync
        await refreshFollowState()
      } else {
        alert(msg)
      }
    } finally {
      setFollowBusy(false)
    }
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 3 }}>
        <Avatar src={user?.profilePic} sx={{ width: 96, height: 96 }} />
        <Box>
          <Typography variant="h5">{user?.fullName ?? 'Unknown'}</Typography>
          <Typography variant="subtitle2" sx={{ color: 'text.secondary' }}>@{user?.username ?? 'unknown'}</Typography>
          <Typography sx={{ mt: 1 }}>{user?.bio}</Typography>
          {viewingOwnProfile && (
            <Button size="small" sx={{ mt: 1 }} variant="outlined" onClick={() => setOpenEdit(true)}>Edit Profile</Button>
          )}
          {!viewingOwnProfile && currentUser && targetUserId && (
            <Box sx={{ mt: 1 }}>
              {isFollowing ? (
                <Button size="small" variant="outlined" color="secondary" disabled={followBusy} onClick={handleUnfollow}>
                  {followBusy ? 'Unfollowing...' : 'Unfollow'}
                </Button>
              ) : (
                <Button size="small" variant="contained" disabled={followBusy} onClick={handleFollow}>
                  {followBusy ? 'Following...' : 'Follow'}
                </Button>
              )}
            </Box>
          )}
          <Box sx={{ mt: 1, display: 'flex', gap: 2 }}>
            <Button size="small" onClick={async () => { setOpenFollowers(true); try { const r = await api.get(`/api/followers/${user?.id ?? user?.userId ?? user?.UserId}/followers`); setFollowersList(r.data || []) } catch {} }}>
              {stats.followers} Followers
            </Button>
            <Button size="small" onClick={async () => { setOpenFollowing(true); try { const r = await api.get(`/api/followers/${user?.id ?? user?.userId ?? user?.UserId}/following`); setFollowingList(r.data || []) } catch {} }}>
              {stats.following} Following
            </Button>
          </Box>
        </Box>
      </Box>

      <Typography variant="h6" sx={{ mb: 2 }}>Albums</Typography>

      <Grid container spacing={2} sx={{ mb: 3 }}>
        {albums.map(album => (
          <Grid item xs={12} sm={6} md={4} key={album.albumId}>
            <Box
              component={Link}
              to={`/albums/${album.albumId}`}
              sx={{ border: '1px solid #eee', p: 2, borderRadius: 1, textDecoration: 'none', display: 'block', '&:hover': { bgcolor: '#fafafa' } }}
            >
              <Typography sx={{ fontWeight: 700 }}>{album.title}</Typography>
              <Typography variant="body2" sx={{ color: 'text.secondary' }}>{album.description}</Typography>
            </Box>
          </Grid>
        ))}
      </Grid>

      <Typography variant="h6" sx={{ mb: 2 }}>Photos</Typography>

      <Grid container spacing={2}>
        {photos.map(photo => (
          <Grid item xs={12} sm={6} md={4} key={photo.photoId}>
            <PhotoCard
              photo={photo}
              ownerUserId={user?.id ?? user?.userId ?? user?.UserId}
              onClick={() => setSelectedId(photo.photoId)}
              onDeleted={(id) => setPhotos(prev => prev.filter(p => p.photoId !== id))}
              onUpdated={async () => {
                const uid = (user?.id ?? user?.userId ?? user?.UserId)
                if (!uid) return
                try {
                  const p = await api.get(`/api/photos/user/${uid}`)
                  const normalized = (p.data || []).map((ph: any) => ({
                    photoId: ph.photoId ?? ph.PhotoId,
                    title: ph.title ?? ph.Title,
                    username: ph.username ?? ph.Username,
                    imageUrl: ph.imageUrl ?? ph.ImageUrl,
                    likesCount: ph.likesCount ?? ph.LikesCount ?? 0,
                    commentsCount: ph.commentsCount ?? ph.CommentsCount ?? 0
                  }))
                  normalized.sort((a: any, b: any) => (b.likesCount - a.likesCount) || (b.photoId - a.photoId))
                  setPhotos(normalized)
                } catch {}
              }}
            />
          </Grid>
        ))}
      </Grid>

      {/* if viewing own profile, show forms to create album/photo */}
      {currentUser && user && (currentUser.id === user.id || currentUser.id === user.userId || currentUser.id === user.UserId) && (
        <Box sx={{ mt: 4, display: 'flex', gap: 2 }}>
          <Button variant="contained" onClick={() => setOpenAlbumDialog(true)}>Create album</Button>
          <Button variant="contained" onClick={() => setOpenPhotoDialog(true)}>Add photo</Button>

          <Dialog open={openAlbumDialog} onClose={() => setOpenAlbumDialog(false)} fullWidth maxWidth="sm">
            <DialogTitle>Create album</DialogTitle>
            <DialogContent>
              <CreateAlbumForm userId={currentUser.id} onCreated={() => {
                // refresh albums and close
                api.get(`/api/albums/user/${currentUser.id}`).then(r => setAlbums(r.data || [])).catch(()=>{})
                setOpenAlbumDialog(false)
              }} />
            </DialogContent>
            <DialogActions>
              <Button onClick={() => setOpenAlbumDialog(false)}>Cancel</Button>
            </DialogActions>
          </Dialog>

          <Dialog open={openPhotoDialog} onClose={() => setOpenPhotoDialog(false)} fullWidth maxWidth="md">
            <DialogTitle>Add photo</DialogTitle>
            <DialogContent>
              <CreatePhotoForm userId={currentUser.id} albums={albums} onCreated={() => {
                api.get(`/api/photos/user/${currentUser.id}`).then(r=>setPhotos(r.data||[])).catch(()=>{})
                api.get(`/api/albums/user/${currentUser.id}`).then(r=>setAlbums(r.data||[])).catch(()=>{})
                setOpenPhotoDialog(false)
              }} />
            </DialogContent>
            <DialogActions>
              <Button onClick={() => setOpenPhotoDialog(false)}>Cancel</Button>
            </DialogActions>
          </Dialog>
        </Box>
      )}
      <PhotoDetailModal open={!!selectedId} photoId={selectedId ?? null} onClose={() => setSelectedId(null)} initial={undefined} />

      {/* Edit Profile Dialog */}
      <Dialog open={openEdit} onClose={() => setOpenEdit(false)} fullWidth maxWidth="sm">
        <DialogTitle>Edit Profile</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField label="Full Name" value={editFullName} onChange={e => setEditFullName(e.target.value)} fullWidth />
            <TextField label="Bio" value={editBio} onChange={e => setEditBio(e.target.value)} fullWidth multiline rows={3} />
            <TextField label="Profile Picture URL or data URI" value={editProfilePic} onChange={e => setEditProfilePic(e.target.value)} fullWidth />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenEdit(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveProfile}>Save</Button>
        </DialogActions>
      </Dialog>

      {/* Followers Dialog */}
      <Dialog open={openFollowers} onClose={() => setOpenFollowers(false)} fullWidth maxWidth="xs">
        <DialogTitle>Followers</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mt: 1 }}>
            {followersList.map(u => (
              <Box key={u.userId ?? u.UserId} sx={{ display: 'flex', alignItems: 'center', gap: 1, cursor: 'pointer' }} onClick={() => { setOpenFollowers(false); window.location.href = `/users/${u.userId ?? u.UserId}` }}>
                <Avatar src={u.profilePic ?? u.ProfilePic} sx={{ width: 28, height: 28 }} />
                <Typography>{u.fullName ?? u.FullName ?? u.username ?? u.Username}</Typography>
              </Box>
            ))}
          </Box>
        </DialogContent>
      </Dialog>

      {/* Following Dialog */}
      <Dialog open={openFollowing} onClose={() => setOpenFollowing(false)} fullWidth maxWidth="xs">
        <DialogTitle>Following</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mt: 1 }}>
            {followingList.map(u => (
              <Box key={u.userId ?? u.UserId} sx={{ display: 'flex', alignItems: 'center', gap: 1, cursor: 'pointer' }} onClick={() => { setOpenFollowing(false); window.location.href = `/users/${u.userId ?? u.UserId}` }}>
                <Avatar src={u.profilePic ?? u.ProfilePic} sx={{ width: 28, height: 28 }} />
                <Typography>{u.fullName ?? u.FullName ?? u.username ?? u.Username}</Typography>
              </Box>
            ))}
          </Box>
        </DialogContent>
      </Dialog>
    </Box>
  )
}
