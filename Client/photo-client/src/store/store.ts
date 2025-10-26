import { configureStore } from '@reduxjs/toolkit'
import photosReducer from './photoSlice'
import userReducer from './userSlice'

export const store = configureStore({
  reducer: {
    photos: photosReducer,
    user: userReducer
  }
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
