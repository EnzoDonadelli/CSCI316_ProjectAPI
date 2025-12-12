Photo Client (React + TypeScript + Vite)

Overview
- Modern React client for the VisaoAPI. Uses Material UI for components and Redux Toolkit for global auth/user state.
- Features include: login/registration, profile editing (name, bio, avatar), album + photo creation/editing, like/comment counts, personalized feed from followed users, tag discovery excluding followed users, and follower/following lists with navigation.

Prerequisites
- Node.js 18+ and npm
- Running VisaoAPI backend (see project root README for backend setup)

Environment
- Base URL is configured via `VITE_API_URL`. If omitted, defaults to `http://localhost:5000`.
- Create a `.env` file in this folder to override:

    VITE_API_URL=http://localhost:5000

Install and Run (PowerShell)
- cd "Client/photo-client"
- npm install
- npm run dev

Key Paths
- `src/api/axios.ts`: Axios instance with Bearer token interceptor reading `localStorage.token`.
- `src/pages/User.tsx`: Profile page, edit profile dialog, follow/unfollow, albums and photos listing.
- `src/pages/Feed.tsx`: Personalized feed using `/api/photos/feed` and tag discovery with `excludeFollowed=true`.
- `src/components/PhotoCard.tsx`: Photo display with owner-only edit/delete, clickable usernames to open profiles.
- `src/store/*`: Redux slices (e.g., `photoSlice.ts` fetching photos).

Notes
- Ensure CORS is enabled on the API (it is configured globally in this project) and that JWT is valid; the interceptor attaches the token automatically.
