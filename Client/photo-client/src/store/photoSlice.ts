import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import api from '../api/axios'

export const fetchPhotos = createAsyncThunk('photos/fetch', async () => {
  const res = await api.get('/api/photos')
  // Normalize response: API may return an array or an object like { value: [...], Count: n }
  const data = Array.isArray(res.data) ? res.data : (res.data.value ?? res.data)
  return data
})

type Photo = {
  photoId: number
  title?: string
  username?: string
}

const photosSlice = createSlice({
  name: 'photos',
  initialState: { photos: [] as Photo[], loading: false },
  reducers: {},
  extraReducers: builder => {
    builder.addCase(fetchPhotos.pending, state => { state.loading = true })
    builder.addCase(fetchPhotos.fulfilled, (state, action) => {
      state.photos = action.payload
      state.loading = false
    })
    builder.addCase(fetchPhotos.rejected, state => { state.loading = false })
  }
})

export default photosSlice.reducer
