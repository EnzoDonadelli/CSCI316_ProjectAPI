# Image Hosting Guide — Make EXTRAS images available as public URLs

Goal: publish the photos currently stored in the `EXTRAS/` folder so each photo has a stable public URL you can store in the database `Photos.ImageUrl` field. Once image URLs are public (or your API serves them directly), the site will use `PhotoDto.FullImageUrl` (server builds it if `ImageUrl` is a filename, or passes an absolute URL through) and the UI will display the real repository photos instead of placeholders.

This document lists multiple ways to achieve this, from the simplest local approach to fully-hosted cloud options. Pick one and follow the step-by-step instructions. I also include small PowerShell scripts to help you update the DB with the resulting URLs.

Summary of recommended options (quick):
- Quick local (dev): Serve `EXTRAS/` directly from ASP.NET at `/images` (edit `Program.cs`) — best for local dev only.
- Tunnel to web (dev/test): Use `ngrok` to expose localhost `/images` to a public URL temporarily.
- Simple hosted static: Upload `EXTRAS` to GitHub Pages (gh-pages) or a static site host and use the raw URLs.
- Production-ready (recommended): Upload to a CDN-backed object store (AWS S3 + CloudFront, or Azure Blob Storage + CDN) or use Cloudinary. Cloudinary is easiest to get started with for image-heavy apps.

Contents
- Quick local dev: Serve EXTRAS from the API (fastest)
- Tunnel (ngrok): expose local server to the internet
- GitHub Pages / GitHub static hosting
- AWS S3 (and AWS CLI) — simple static hosting
- Azure Blob Storage
- Cloudinary (recommended for simplicity)
- Imgur (quick anonymous uploads)
- How to update your DB to point to hosted URLs (PowerShell + SQL snippets)
- How to verify in the API and client

---

## 1) Quick local dev: serve `EXTRAS/` directly at `/images`
Use this if you're only testing locally (no public internet required). This makes your API serve files from the project `EXTRAS/` folder so `fullImageUrl` will resolve to `http://localhost:5000/images/{filename}`.

Edit `Program.cs` (VisaoAPI) and change the static file registration to point to the EXTRAS folder directly (replace/copy into the `try` block used to prepare images). Example snippet you can add *instead of* copying files:

```csharp
// serve EXTRAS directly when available
var extrasPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "EXTRAS"));
if (Directory.Exists(extrasPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(extrasPath),
        RequestPath = "/images"
    });
}
else
{
    // fallback to wwwroot/images if you prefer
    var imagesDir = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "images");
    if (Directory.Exists(imagesDir))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(imagesDir),
            RequestPath = "/images"
        });
    }
}
```

Notes:
- In this approach the API serves the images from your local disk. It works only while the API runs on your machine and is not publicly accessible without additional tooling (see ngrok below).
- If `Photos.ImageUrl` contains just the filename (e.g. `WINDOW (7).jpg`), your server code that builds `FullImageUrl` to `/images/{filename}` will work.

Restart the API and visit `http://localhost:5000/images/<filename>` to verify.

---

## 2) Tunnel to the web with ngrok (dev/test, temporary)
If you want a quick public URL so other devices can reach your local API (including images), expose your local API with ngrok.

Steps:
1. Install ngrok (https://ngrok.com/) and sign up for an account if you want stable URLs.
2. Run your API locally (e.g. `dotnet run --project VisaoAPI/VisaoAPI.csproj --urls "http://localhost:5000"`). Ensure `/images/{filename}` works locally.
3. In a separate shell run:

```powershell
ngrok http 5000
```

4. ngrok will show a public URL like `https://abcd-1234.ngrok.io`. Use `https://abcd-1234.ngrok.io/images/{encoded-filename}` as the public image URLs.

Caveats:
- ngrok URLs are ephemeral unless you use a paid plan. Use only for demos and testing.

---

## 3) GitHub Pages / Raw GitHub hosting (good for small static sets)
If your repo is public, you can publish the EXTRAS folder as part of a GitHub Pages site and reference images from there.

Options:
- Push EXTRAS into a `gh-pages` branch and use a static site generator or `ghp-import` to publish.
- Use raw.githubusercontent.com direct links to files in the `main` branch. Example:

```
https://raw.githubusercontent.com/<your-username>/<repo>/main/EXTRAS/WINDOW%20(7).jpg
```

Notes:
- raw.githubusercontent.com sometimes serves with content-type headers that browsers treat as downloads. Using GitHub Pages (which serves HTML/CSS) ensures images are served normally.

Quick steps (GitHub Pages via gh-pages branch):
1. Create a `gh-pages` branch and copy `EXTRAS/` into the branch root (so the files are at `/<filename>`).
2. Enable GitHub Pages for that branch in repository settings.
3. After pages are published, your images will be at `https://<your-username>.github.io/<repo>/<filename>`.

Use those URLs in your DB.

---

## 4) AWS S3 (simple, cheap, and global)
This is the recommended production-like approach for static hosting.

Prereqs: AWS CLI installed and configured with an IAM user that has S3 rights.

PowerShell steps:

```powershell
# 1. create a bucket (replace <bucket-name> with unique name)
aws s3 mb s3://my-photos-example-12345 --region us-east-1

# 2. make it public (for simple public hosting) - better to use CloudFront in prod
aws s3api put-bucket-policy --bucket my-photos-example-12345 --policy '{"Version":"2012-10-17","Statement":[{"Sid":"PublicReadGetObject","Effect":"Allow","Principal":"*","Action":"s3:GetObject","Resource":"arn:aws:s3:::my-photos-example-12345/*"}]}'

# 3. sync EXTRAS into the bucket
aws s3 sync "C:\path\to\repo\EXTRAS" s3://my-photos-example-12345 --acl public-read --exclude "*.db"
```

The public image URL format will be:
```
https://my-photos-example-12345.s3.amazonaws.com/WINDOW%20(7).jpg
```

Consider adding a CDN (CloudFront) in front for performance and HTTPS custom domain.

---

## 5) Azure Blob Storage
PowerShell / Azure CLI approach (recommended if you already use Azure).

1. Create a storage account and container (allow public access) via Azure Portal or CLI:

```powershell
az storage account create --name myphotosstorage --resource-group myResourceGroup --location eastus --sku Standard_LRS
az storage container create --account-name myphotosstorage --name images --public-access blob

# upload all files
az storage blob upload-batch --account-name myphotosstorage -s "C:\path\to\EXTRAS" -d images
```

Public URL:
```
https://<account>.blob.core.windows.net/images/<filename>
```

---

## 6) Cloudinary (easy, developer-friendly) — my recommended option for an image-heavy demo
Cloudinary provides instant image hosting, transformations, and a generous free tier. It's easy to use and integrates well with web apps.

Steps:
1. Sign up for a free Cloudinary account.
2. In the Cloudinary dashboard you have a cloud name and API key/secret. You can use the dashboard to upload many images manually.
3. After uploading, each image has a secure URL like:

```
https://res.cloudinary.com/<cloud_name>/image/upload/v1234567890/WINDOW_7.jpg
```

4. Use those URLs in your DB (set `Photos.ImageUrl` to the full https://... link). Your server will pass this through as `FullImageUrl`.

Programmatic upload example (curl):

```bash
# using unsigned preset or server-side signed upload (recommended) — example unsigned
curl -X POST "https://api.cloudinary.com/v1_1/<cloud_name>/image/upload" \
  -F file=@"WINDOW (7).jpg" \
  -F upload_preset=<your_unsigned_preset>
```

Cloudinary makes it trivial to transform images with URL parameters and serves them via CDN.

---

## 7) Imgur (quick anonymous hosting)
Imgur allows anonymous image uploads and returns a URL. This is fine for quick demos but not ideal for many images or for long-term storage.

Simple curl example (you need a client id):

```bash
curl -H "Authorization: Client-ID <your-client-id>" -F "image=@WINDOW\ (7).jpg" https://api.imgur.com/3/upload
```

The API response contains the `link` (e.g., https://i.imgur.com/abc123.jpg) — store that in the DB.

Caveat: Imgur has rate limits and terms of service to consider.

---

## 8) Update your DB so the site uses the hosted image URLs
Two patterns:
- Set `Photos.ImageUrl` to the absolute public URL (recommended when using S3/Cloudinary/GitHub Pages). The server's `BuildFullImageUrl` is already written to pass-through http(s) or data: URLs, so this will be returned as `FullImageUrl` and used by the client.
- Or leave `Photos.ImageUrl` as a filename and ensure your API serves the filename under `/images/{filename}` (either by serving EXTRAS directly or by copying files to `wwwroot/images`).

Example: PowerShell script to generate SQL `UPDATE` statements mapping each EXTRAS filename to a public S3/Cloudinary URL (modify the prefix):

```powershell
$repoExtras = "C:\Users\enzod\Documents\CSCI Final\CSCI316_ProjectAPI\EXTRAS"
$prefix = "https://my-photos-example-12345.s3.amazonaws.com/"
Get-ChildItem -Path $repoExtras -File -Include *.jpg,*.jpeg,*.png | ForEach-Object {
    $name = $_.Name
    $url = $prefix + [System.Uri]::EscapeDataString($name)
    $sql = "UPDATE Photos SET ImageUrl = '$url' WHERE ImageUrl = '$name';"
    $sql
}
```

Redirect output to a .sql file and run it in your SQL Server against the `PhotoSharingDbContext` database.

For example:
```powershell
. \generate-updates.ps1 > update-image-urls.sql
# then run this SQL with SQL Management Studio or sqlcmd
sqlcmd -S <server> -d <db> -i update-image-urls.sql -U <user> -P <pw>
```

Note: If you prefer to keep `ImageUrl` as a filename, ensure the server builds a proper `FullImageUrl` (the code already supports this) and your server is serving the images at `/images`.

---

## 9) Verify everything works (API + Client)
1. If you changed DB records to absolute URLs, request the API and check `GET /api/photos`. Each photo should include `fullImageUrl` equal to the absolute URL.

2. If you're serving files under `/images` (local EXTRAS or S3/CloudFront), ensure:
   - `GET http://localhost:5000/images/<encoded-filename>` loads the image when served locally, or
   - `https://<cdn-or-bucket>/images/<filename>` loads when hosted remotely.

3. In the client, hard-refresh (Ctrl+F5) and ensure images render. If some still show picsum, inspect the network panel to see which URL was attempted and whether it returned 200 or a 404.

---

## 10) Optional: Add an admin endpoint to quickly inspect stored `ImageUrl` vs computed `FullImageUrl`
Add a small endpoint to `VisaoAPI` like `/api/admin/photo-urls` that returns `PhotoId`, `ImageUrl`, and `FullImageUrl` for quick verification. Example controller method (C#):

```csharp
[HttpGet("/api/admin/photo-urls")]
public async Task<IActionResult> AdminPhotoUrls()
{
    var photos = _context.Photos.Take(500).Select(p => new {
        p.PhotoId,
        p.ImageUrl,
        FullImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? null : (p.ImageUrl.StartsWith("http") || p.ImageUrl.StartsWith("data:") ? p.ImageUrl : $"{Request.Scheme}://{Request.Host}/images/{Uri.EscapeDataString(p.ImageUrl)}")
    }).ToList();
    return Ok(photos);
}
```

This returns exactly what the client will use and is handy for debugging.

---

## Recommendation (short)
- For local dev: either serve EXTRAS directly via `Program.cs` or copy EXTRAS into `wwwroot/images` as you were doing. This is simplest.
- For a public demo: Cloudinary or AWS S3 (with CloudFront) — Cloudinary is easiest to get started with and gives you immediate CDN-backed URLs.
- After uploading images: update `Photos.ImageUrl` to point to the absolute https:// links and then `GET /api/photos` should return `fullImageUrl` (which the client will use as-is).

---

If you want, I can:
- Implement the `Program.cs` change to serve `EXTRAS` directly for local dev and restart the API for you, or
- Add the `/api/admin/photo-urls` endpoint for quick verification, or
- Generate the SQL update file for a chosen host prefix (Cloudinary/S3/GitHub Pages) that maps local filenames to public URLs and optionally run it against your DB (you'd need to provide DB connection or run the SQL locally).

Which option would you like me to do next? If you choose a hosting provider (S3, Azure, Cloudinary, or GitHub Pages) tell me the chosen public prefix and I will generate the SQL `UPDATE` file for you.
