Where the app's photos come from and how to choose which appear

Overview
--------
This project shows photos in the web UI that are backed by database rows (the `Photos` table) and image files served by the API static-file middleware. There are three ways a photo's image can be provided:

1. Full absolute URL (external URL or data: URL)
2. Local filename stored in the DB (served from the repository `EXTRAS` folder via `/images/{filename}`)
3. Fallback: if a photo record lacks a usable image, the client shows a picsum placeholder

Key files & places to look
--------------------------
- EXTRAS/ (repo root) — a folder with example image files that the seeder imports and links into DB rows.
- VisaoAPI/Program.cs — static file middleware configuration and the idempotent importer that seeds EXTRAS images into the DB on startup when run with the seeder path.
- VisaoAPI/Controllers/PhotosController.cs — API endpoints for photos, and the server-side logic that builds `FullImageUrl` for each PhotoDto.
- VisaoAPI/DTOs/PhotoDTOs.cs — the response DTO includes both `ImageUrl` (the stored DB value) and `FullImageUrl` (an absolute URL the client should use).
- Client/photo-client/src/components/PhotoCard.tsx — client rendering logic: prefers `fullImageUrl`, falls back to `imageUrl` translated to `/images/{encoded-filename}`, then falls back to a picsum placeholder.

How the app decides which image to show for a photo
-------------------------------------------------
When the API returns a `PhotoDto` it includes:
- `ImageUrl` — the raw stored value from the database (this might be a filename, a relative path, or an external URL)
- `FullImageUrl` — computed by the server as follows:
  - If `ImageUrl` already looks like an absolute URL (starts with `http://`, `https://`) or is a `data:` URL, `FullImageUrl` is the same value.
  - Otherwise `FullImageUrl` is constructed as an absolute URL pointing to the API static file route: `https://{HOST}/images/{Uri.EscapeDataString(ImageUrl)}`.

The client uses `fullImageUrl` when present. If not present it will try to build a `/images/{encoded-filename}` URL itself. If all else fails it shows a picsum placeholder.

How you can control which photos are displayed
---------------------------------------------
You have several options depending on whether you want to change existing DB rows, add new photos, or control which files are served:

A. Use the EXTRAS folder + seeder (recommended for local/demo data)
- Place image files you want into the repo's `EXTRAS/` folder. Keep file names readable (avoid leading/trailing spaces, prefer simple names).
- Start the API using the seeder script (this project includes convenience scripts):
  - Windows: run `START_API_WITH_DATA.bat` or `run-api.bat` (these scripts call the ASP.NET app which runs the idempotent importer in Program.cs).
  - Or run directly: `dotnet run --project VisaoAPI/VisaoAPI.csproj --urls "http://localhost:5000"`
- The importer will create users/albums and create `Photo` rows with `ImageUrl` equal to the filename in `EXTRAS`.
- After the server is running you can verify: GET `http://localhost:5000/api/photos` and check `fullImageUrl` values. Example: `http://localhost:5000/images/WINDOW%20(7).jpg`

B. Create or update Photo rows via the API
- Create a photo (returns a PhotoDto with FullImageUrl):
  - POST /api/photos/user/{userId}
  - Body (JSON, CreatePhotoDto):
    {
      "albumId": optionalNumber,
      "title": "My Photo",
      "description": "...",
      "imageUrl": "WINDOW (7).jpg",   // filename in EXTRAS OR an absolute URL OR data: URL
      "tags": ["mountains","snow"]
    }
  - Example with curl (HTTP API):
    curl -X POST "http://localhost:5000/api/photos/user/2" \
      -H "Content-Type: application/json" \
      -d "{ \"title\": \"From EXTRAS\", \"imageUrl\": \"WINDOW (7).jpg\" }"
- Update an existing photo (PUT /api/photos/{id}) to set `ImageUrl` to a different filename or URL.

C. Update the database directly (for bulk or scripted changes)
- Open your database and run SQL to change `ImageUrl` for specific Photo rows. Example (SQL Server syntax):
  UPDATE Photos SET ImageUrl = 'WINDOW (7).jpg' WHERE PhotoId = 12;
- After updating, request `GET /api/photos` to confirm `fullImageUrl` is pointing to `/images/WINDOW%20(7).jpg`.

D. Use external URLs or data URLs
- If your `ImageUrl` value is already an absolute URL (e.g., `https://example.com/images/foo.jpg`) the API will return that as `FullImageUrl` and the client will load it directly.
- The current client upload form supports reading local files and sending them as `data:` URLs (base64) in `ImageUrl`. These will be returned as `FullImageUrl` and render in the UI, but this is not recommended for production because it stores large base64 strings in the DB.

Recommended production flow (best practice)
------------------------------------------
1. Add a server endpoint to accept file uploads (multipart/form-data). The endpoint should:
   - Accept uploaded file(s), validate content-type and size
   - Store files in a dedicated folder (e.g., `wwwroot/images` or object storage)
   - Return the stored filename or full URL
2. When creating a Photo resource, use the upload endpoint first to get the stored filename or URL, then POST /api/photos/user/{userId} with the returned filename/URL in `ImageUrl`.
3. Use consistent naming and possibly a hashing/naming scheme to avoid collisions.
4. Add caching headers and content-type when serving static images.

Troubleshooting checklist
-------------------------
- If an image shows a broken URL in the browser:
  - Inspect the PhotoDto: GET /api/photos and check the `fullImageUrl` value.
  - Open that FullImageUrl in the browser directly. If you get 404, check whether the filename exists in `EXTRAS/` and whether the API's static files are configured correctly.
- Confirm static files middleware is enabled (see `VisaoAPI/Program.cs`). It should map the repository `EXTRAS` directory to `/images` request path.
- URL encoding: filenames with spaces or special characters are escaped by the server (e.g., `WINDOW (7).jpg` -> `WINDOW%20(7).jpg`). If you hard-code links, always URL-encode them.
- If you updated the EXTRAS folder while the server is running and new files don't appear, restart the server or re-run the importer (the importer is idempotent and checks for existing Photo records).

Quick verification commands
---------------------------
- List photos (see FullImageUrl):
  GET http://localhost:5000/api/photos

- Get a single user's photos:
  GET http://localhost:5000/api/photos/user/{userId}

- Add a photo (example creating a DB row that points to EXTRAS filename):
  curl -X POST "http://localhost:5000/api/photos/user/2" -H "Content-Type: application/json" -d "{\"title\":\"My EXTRAS photo\",\"imageUrl\":\"WINDOW (7).jpg\"}"

- If you want to set an external URL instead:
  curl -X POST "http://localhost:5000/api/photos/user/2" -H "Content-Type: application/json" -d "{\"title\":\"External photo\",\"imageUrl\":\"https://example.com/my.jpg\"}"

Notes about the client behavior
------------------------------
- The client prefers `PhotoDto.fullImageUrl` when present. If that is missing it attempts to build a `/images/{encoded-filename}` URL from `imageUrl` and the configured `VITE_API_URL`.
- Older records or placeholder records may have external URLs or empty fields; switching them to point to local EXTRAS files will make the client show the repository images.

If you'd like, I can:
- Update the seeder so it overwrites specific Photo records to point to EXTRAS filenames (helpful for demo data).
- Implement a proper file upload endpoint (server) and change the client to use FormData (recommended next step).
- Add an admin endpoint that returns a convenient table (photoId, imageUrl, fullImageUrl) for quick verification.

Where I put this document
-------------------------
Saved as `IMAGE_SOURCES.md` at the repository root.

Questions?
----------
Tell me which workflow you'd prefer for production (server-side uploads + storage, or keeping base64-in-DB as-is for now) and I will implement the next step (upload endpoint + client update) for you.