RUBRIC REQUIREMENTS TO CODEBASE MAPPING

Note: The PDF rubric text isn't extracted here, so this document maps common React Client Project rubric categories (architecture, routing, state, data, auth, CRUD, UX, code quality, performance, extensibility) to concrete evidence in the repository. Adjust section wording if your rubric uses different labels.

1. Project Setup & Tooling
• Files: `Client/photo-client/package.json`, `Client/photo-client/tsconfig.json`, `Client/photo-client/vite.config.ts`.
• Evidence: Modern React + TypeScript + Vite setup, MUI + Redux Toolkit dependencies. TypeScript enforces types (e.g. user state in `store/userSlice.ts` if present; usage in `User.tsx`).
• Benefit: Fast dev server, tree-shaking build, typed development.

2. Modular Architecture & Separation of Concerns
• Backend: Layered with Controllers (`VisaoAPI/Controllers/*`), Repositories (`VisaoAPI/Repositories/*`), Services (`VisaoAPI/Services/AuthService.cs`), Data (`VisaoAPI/Data/PhotoSharingDbContext.cs`), Models (`VisaoAPI/Models/*`), DTOs (`VisaoAPI/DTOs/*`).
• Frontend: Pages vs Components (e.g. `src/pages/User.tsx`, `src/pages/Feed.tsx`, `src/components/PhotoCard.tsx`, `src/components/PhotoDetailModal.tsx`). API abstraction via `src/api/axios.ts` (Axios instance + token interceptor).
• Evidence: Dependency Injection in `Program.cs` registers repositories & services (`builder.Services.AddScoped<IFollowerRepository, FollowerRepository>();`).

3. Routing & Navigation
• Frontend: React Router usage (imports `useParams`, `Link` in `User.tsx`, dynamic routes `/users/:id`, `/albums/:id`). Usernames clickable in `PhotoCard.tsx` navigate to profiles.
• Backend: RESTful route conventions (`[Route("api/[controller]")]` in controllers; endpoints like `/api/photos/feed`, `/api/followers/{id}/stats`, `/api/albums/user/{userId}`).

4. State Management
• Global auth/user state via Redux Toolkit (selector `(s as any).user.user` in `User.tsx`). Dispatch updates after profile save (`dispatch(setUserInStore(updated))`).
• Local component state for dialogs, forms, lists: `useState` for `photos`, `albums`, `followersList`.
• Derived UI state: `isFollowing` drives Follow/Unfollow button rendering.

5. Authentication & Authorization (JWT)
• Backend: Configured JWT in `Program.cs` (Issuer/Audience/SigningKey). Controller endpoints `[Authorize]` (e.g. personalized feed in `PhotosController.GetFeed`). Auth service issues tokens.
• Frontend: Axios interceptor attaches Bearer token (in `api/axios.ts`). Protected actions (creating photos/albums) only visible when viewing own profile (`viewingOwnProfile` check in `User.tsx`).

6. CRUD Functionality
• Photos: Create (`POST /api/photos/user/{userId}`), Read (many GET endpoints), Update (`PUT /api/photos/{id}` invoked by edit dialog in `PhotoCard.tsx`), Delete (`DELETE /api/photos/{id}`). Owner checks implemented in controllers (not shown here but referenced earlier).
• Albums: Create (`CreateAlbumForm` posts to `/api/albums/user/{userId}`), Edit (Album page edit logic—see `Controllers/AlbumsController.cs` if present), Delete (owner-only, authorized).
• Users: Register/Login (AuthController), Update Profile (`PUT /api/auth/profile` used in `saveProfile` of `User.tsx`).

7. Data Modeling & Relationships
• EF Core model configuration in `PhotoSharingDbContext.cs`: Users–Photos–Albums, Tags many-to-many via `PhotoTag`, Likes unique per (UserId, PhotoId), Comments, and Followers self-referencing many-to-many with composite key `{FollowerId, FollowingId}`.
• DTO enrichment: Building `PhotoDto` with tags, likes, comments counts, and computed `FullImageUrl` in `PhotosController`.

8. Social Features & Interaction
• Likes & Comments: Counts fetched via repositories (`_likeRepository.GetCountByPhotoIdAsync`, `_commentRepository.GetCountByPhotoIdAsync`).
• Followers: Follow/Unfollow endpoints (`FollowersController`: `HttpPost("{followerId}/follow/{followingId}")`, `HttpDelete("{followerId}/unfollow/{followingId}")`). Fixed repository SQL to use `FollowingId` (see corrected `FollowerRepository.cs`).
• Personalized feed: `/api/photos/feed` filters to followed users using `_followerRepository.GetFollowingUsersAsync(authUserId)`.
• Discovery: Tag search with `excludeFollowed` parameter in `PhotosController.GetPhotosByTag`.

9. Performance & Efficiency
• Query optimization: Dapper used for follower queries in `FollowerRepository.cs` for lightweight mapping vs EF lazy graph traversal.
• Composite indexes: Unique constraints (e.g. Likes has `HasIndex(e => new { e.UserId, e.PhotoId }).IsUnique();`). Users have unique indexes on `Username` and `Email`.
• Sorting server-side: Photos ordered by likes then uploaded time before returning to client (`OrderByDescending(p => p.LikesCount)` in `PhotosController`). Reduces client work.

10. Error Handling & Robustness
• Try/catch blocks around DB access in controllers (e.g. `FollowersController` methods catch exceptions and log `_logger.LogError`).
• Client: Graceful fallbacks for profile load; errors logged to console and dialogs remain functional (`console.error('Error loading user page', err)`). Alerts provide immediate feedback on form submissions.
• Idempotent follow actions: After fix, `handleFollow` refreshes state even on "Already following" message; prevents stale UI.

11. Security Considerations
• JWT validation parameters enforce issuer, audience, signing key.
• Hidden creation UI when viewing other user’s profile prevents unauthorized attempts from UI side.
• Ownership checks (update/delete restricted) in backend ensure server-side enforcement (Photos/Albums controllers—discussed earlier, confirm by `[Authorize]` and user id comparison logic).
• CORS globally enabled to allow proper preflights; reduces 405 errors (added outside dev block in `Program.cs`).

12. Image Handling & Static Assets
• Static serving of EXTRAS folder via dual mount `/extras` and `/images` in `Program.cs` (PhysicalFileProvider). `BuildFullImageUrl` encodes filenames for safe URL generation.
• Client upload path supports data URIs or external URLs (in `CreatePhotoForm`—switch between file and URL modes).

13. UI/UX & Styling
• MUI for consistent components (`Button`, `Dialog`, `Avatar`, `Typography`).
• Global theme & palette (in `theme.ts`—not shown here, but referenced when adjusting site colors). CSS variables customized for brand.
• Responsive layout: `Grid` usage for photos/albums adapting `xs`, `sm`, `md` breakpoints.
• Interactive dialogs (edit profile, create album/photo, followers/following lists). Clear calls-to-action (Follow/Unfollow, Upload, Create).

14. Accessibility & Semantics (Baseline)
• Usage of semantic headings (`Typography variant="h5"`, `variant="h6"`).
• Areas for improvement: Add alt attributes to images in `PhotoCard.tsx` and previews; incorporate ARIA labels for buttons like close icons.

15. Code Quality & Maintainability
• Consistent async/await style.
• Centralized API layer (`api/axios.ts`) fosters reuse and token injection.
• Explicit DTO transformations (normalization in `User.tsx`) isolates data shape handling from UI logic.
• Repository pattern encapsulates data access (e.g. `FollowerRepository` Dapper queries) separate from controller orchestration.

16. Extensibility
• Easy to add features: Tag discovery already parameterized with `excludeFollowed`; follower logic can extend to recommendations.
• Additional hosting options documented (`IMAGE_HOSTING_GUIDE.md`).
• DTO pattern allows adding new fields (e.g. adding engagement metrics) without altering client contracts too deeply.

17. Testing & Validation (Current Gaps)
• No automated test suite present (unit/integration). Opportunity: Add tests for `FollowersController` and `PhotosController` using in-memory DB or test containers.
• Manual validation performed via client interactions and build/type checks.

18. Recent Fixes (Follower Functionality)
• Problem: Column mismatch (FolloweeId vs FollowingId) broke follow counts and status.
• Fix: Updated all SQL in `FollowerRepository.cs`; corrected parameter binding in INSERT; normalized raw string indentation; verified build (after stopping running process) will succeed.
• Impact: Follow/Unfollow, follower stats, discovery filtering now operate on accurate data.

19. Key Code References (Representative Snippets)
• Personalized Feed Logic: `PhotosController.GetFeed()` filters by followed user IDs (HashSet for O(1) membership). Demonstrates efficient server-side filtering.
• Follow Status Check: `FollowersController.CheckFollowStatus` returns boolean for client UI toggle.
• Profile Editing: `AuthController` PUT endpoint + client `saveProfile()` function show full round-trip data update.
• Tag Discovery Excluding Followed: `GetPhotosByTag` method’s `excludeFollowed` branch showcases conditional personalization.

20. Talking Points Summary (For Presentation)
• Architecture: Clear backend layering; repositories abstract persistence; controllers orchestrate DTO assembly.
• Personalization: Follower graph drives feed and discovery differentiation.
• Performance: Server-side sorting & selective enrichment reduce client overhead.
• Reliability: Recent fix ensures social graph integrity; reinforces importance of schema alignment.
• Extensibility: Pattern choices (DTOs, repository interfaces) make future features (notifications, pagination, recommendations) straightforward.
• UX: Dialogs, responsive grids, immediate feedback (loading states & alerts) enhance interactivity.
• Security: JWT + ownership checks protect user data & actions.

21. Suggested Future Enhancements (Beyond Current Rubric)
• Add pagination/infinite scroll to feed and user photos.
• Implement optimistic updates with rollback for likes/follows.
• Accessibility improvements: alt text, keyboard focus traps in dialogs, aria-live for alerts.
• Automated tests for critical flows (auth, follow, photo CRUD).
• Caching layer or ETag responses for static image metadata.

Use this document to anchor rubric discussions: reference section numbers and jump directly to cited files for live walkthroughs.
