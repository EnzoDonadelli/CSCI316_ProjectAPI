import { createSlice } from '@reduxjs/toolkit'

const userSlice = createSlice({
  name: 'user',
  initialState: { user: null as null | { id?: number; username?: string; fullName?: string; profilePic?: string; email?: string } },
  reducers: {
    setUser(state, action) {
      const p = action.payload as any
      // normalize backend UserDto -> { id, username, fullName, profilePic, email }
      if (!p) {
        state.user = null
        return
      }
      state.user = {
        id: p.UserId ?? p.userId ?? p.id,
        username: p.Username ?? p.username,
        fullName: p.FullName ?? p.fullName,
        profilePic: p.ProfilePic ?? p.profilePic,
        email: p.Email ?? p.email
      }
    },
    clearUser(state) {
      state.user = null
    }
  }
})

export const { setUser, clearUser } = userSlice.actions
export default userSlice.reducer
