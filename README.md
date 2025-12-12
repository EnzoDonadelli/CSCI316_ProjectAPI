# CSCI316 Photo Sharing Platform

Full-stack photo sharing project consisting of an ASP.NET Core Web API (VisaoAPI) and a React + TypeScript client (photo-client). Users can register, authenticate with JWT, create albums and photos, follow other users, and view a personalized photo feed.

## High-Level Architecture

Backend (VisaoAPI):
- ASP.NET Core 8 Web API
- Entity Framework Core for primary data modeling & migrations (DbContext: `PhotoSharingDbContext`)
- Dapper used in targeted repositories for lightweight queries (e.g., follower relationships)
- Layered design: Controllers -> Services (Auth) -> Repositories -> Data Models/DTOs
- JWT authentication & authorization; ownership checks on protected mutations
- Static image serving from repository `EXTRAS` folder or `wwwroot/images`

Frontend (Client/photo-client):
- React 18 + TypeScript + Vite
- Material UI (MUI) for UI components and theming
- Redux Toolkit for global user/auth/photo state
- Axios instance with token interceptor for authenticated API calls

## Key Features
- User Registration & Login (JWT issuance, token validation)
- User Profile Management (name, bio, profile picture) via `/api/auth/profile`
- Photo CRUD with tags and like/comment counts enrichment
- Album CRUD with owner-only edit/delete and per-user listing
- Follow / Unfollow system enabling a social graph
- Personalized Feed: photos only from followed users (`/api/photos/feed`)
- Tag-based discovery with optional exclusion of followed creators
- Sorting photos by like counts (feed and profiles)
- Follower & Following lists with navigation between user profiles
- Owner-only controls (edit/delete, create content visibility restricted when viewing others)

## Data Model Summary
Entities:
- User, Photo, Album, Tag, PhotoTag (many-to-many), Like, Comment, Follower (self-referencing many-to-many)

`PhotoSharingDbContext` configures:
- Composite keys: `PhotoTag(PhotoId, TagId)`, `Follower(FollowerId, FollowingId)`
- Unique indices: Username, Email (User); (UserId, PhotoId) (Like)
- Foreign key relationships with controlled delete behaviors (NoAction / SetNull)

## Selected Endpoints
AuthController:
- POST `/api/auth/register`
- POST `/api/auth/login`
- GET `/api/auth/profile` (auth)
- PUT `/api/auth/profile` (auth) – Update full name, bio, profile pic
- GET `/api/auth/validate-token` (auth)

PhotosController:
- GET `/api/photos` – All photos ordered by likes desc, then date
- GET `/api/photos/tag/{tagName}?excludeFollowed=true` – Discovery excluding followed users
- GET `/api/photos/feed` (auth) – Personalized feed of followed users
- POST `/api/photos/user/{userId}` – Create photo for user
- PUT/DELETE `/api/photos/{photoId}` – Update/Delete (ownership enforced)

AlbumsController (representative):
- GET `/api/albums/user/{userId}` – User’s albums
- POST `/api/albums/user/{userId}` – Create album
- PUT/DELETE `/api/albums/{albumId}` – Edit/Delete (owner-only)

FollowersController:
- POST `/api/followers/{followerId}/follow/{followingId}`
- DELETE `/api/followers/{followerId}/unfollow/{followingId}`
- GET `/api/followers/{userId}/followers` & `/following` – Lists
- GET `/api/followers/{followerId}/follows/{followingId}` – Status
- GET `/api/followers/{userId}/stats` – Counts

## Image Handling
- Raw filenames stored (e.g., "BIRDS (1).jpg")
- `Program.cs` configures static file providers to serve `EXTRAS` at `/extras` and `/images`
- `PhotosController.BuildFullImageUrl` constructs an absolute URL unless already absolute or a data URI

## Personalization Logic
- Follower relationships determine visibility in the feed
- Tag discovery mode optionally filters out creators you already follow
- Likes-based server-side ordering ensures consistent ranking across client views

## Frontend Overview
Directories:
- `src/pages/` – `User.tsx`, `Feed.tsx`, `Home.tsx`
- `src/components/` – `PhotoCard.tsx`, `PhotoDetailModal.tsx`
- `src/store/` – Redux slices (`photoSlice.ts`, `userSlice.ts`)
- `src/api/axios.ts` – Axios instance with JWT interceptor

Notable Client Flows:
1. Authentication: Login stores token in localStorage, Axios attaches Authorization header.
2. Profile Editing: Dialog submits PUT `/api/auth/profile`, updates Redux and local state.
3. Photo Creation: Supports URL or data URI uploads; sequential posting for multi-file set.
4. Follow/Unfollow: Button triggers POST/DELETE endpoints; UI refreshes status and counts.
5. Feed Loading: Authenticated request to `/api/photos/feed`; fallback to discovery search.

## Social Graph Fix (Follower Repository)
Issue: Initial Dapper queries used column name `FolloweeId` while EF/Data schema defined `FollowingId`.
Resolution: Updated all queries in `FollowerRepository.cs` to use `FollowingId` consistently, corrected INSERT parameter binding, and aligned indentation for raw string literals.
Impact: Follow/unfollow actions, follower counts, feed filtering, and discovery exclusion now reflect correct persisted data.

## Security & Authorization
- JWT validation (issuer, audience, signing key) configured in `Program.cs`
- Owner checks for sensitive mutations (photos, albums) executed in controllers comparing authenticated user ID with resource owner ID
- CORS enabled globally to allow PUT/DELETE preflight requests

## Development Scripts
Client:
```powershell
cd "Client/photo-client"
npm install
npm run dev
```
API:
```powershell
cd "VisaoAPI"
dotnet build
# Run with explicit URL
 dotnet run --urls "http://localhost:5000"
```

## Configuration
Environment Variables (API): `JwtSettings:SecretKey`, `Issuer`, `Audience`, connection string `DefaultConnection`.
Client Env: `VITE_API_URL` in `.env` file for base URL.

## Extensibility & Future Enhancements
- Pagination/infinite scroll for feed and user photo lists
- Notifications (e.g., on new follower or photo like)
- Advanced search (multi-tag, user filters)
- Image hosting externalization (documented in `IMAGE_HOSTING_GUIDE.md`)
- Automated testing harness (unit tests for repositories/controllers)
- Accessibility improvements (alt text, keyboard navigation, aria-live regions)

## Known Gaps
- No automated test suite currently included
- Limited error boundary components on client
- Likes/comments UI details not fully surfaced (counts shown; forms could be expanded)

## Quick File Reference
Backend:
- `VisaoAPI/Program.cs` – DI setup, static files, JWT, CORS
- `VisaoAPI/Data/PhotoSharingDbContext.cs` – EF model config
- `VisaoAPI/Controllers/PhotosController.cs` – Photo endpoints & feed logic
- `VisaoAPI/Controllers/AuthController.cs` – Auth & profile endpoints
- `VisaoAPI/Controllers/FollowersController.cs` – Follower actions & stats
- `VisaoAPI/Repositories/FollowerRepository.cs` – Dapper follower queries (fixed)

Frontend:
- `Client/photo-client/src/api/axios.ts` – Axios setup
- `Client/photo-client/src/pages/User.tsx` – Profile & social interactions
- `Client/photo-client/src/pages/Feed.tsx` – Personalized feed
- `Client/photo-client/src/components/PhotoCard.tsx` – Photo display & owner actions
- `Client/photo-client/src/store/photoSlice.ts` – Photo fetching state

## Presentation Talking Points
1. Architecture: Clean separation (controllers, services, repositories, models, DTOs) improves maintainability.
2. Personalization: Follower graph drives feed and discovery filtering for relevant content.
3. Performance: Server-side ordering and selective joins minimize client computation.
4. Reliability: Follower repository fix demonstrates iterative debugging and schema alignment.
5. UX & Security: Conditional owner controls + JWT auth deliver a secure user experience.
6. Extensibility: Patterns support adding new social or media features quickly.

---
This README provides a cohesive overview for evaluators and stakeholders. Adjust any section with rubric-specific wording as needed.
