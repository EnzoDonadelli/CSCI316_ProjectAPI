# 📸 Photo Sharing API - Complete Project Guide

## 🎯 **Project Overview**

This is a **RESTful Web API** built with **ASP.NET Core 8** that provides a complete backend for a photo sharing social media application. Think of it like the backend for Instagram or Flickr - it manages users, photos, albums, likes, comments, and social interactions.

---

## 🏗️ **Project Architecture & Folder Structure**

### **Root Level Structure**
```
CSCI316_ProjectAPI/
├── VisaoAPI/                    ← Main Web API project
├── Repository/                  ← Data access layer (unused in current implementation)
├── ServiceLayer/               ← Business logic layer (unused in current implementation)
├── DatabaseProject/            ← SQL Server database project
└── DogApiExample/              ← Example project (unrelated)
```

### **VisaoAPI - Main Project Structure**
```
VisaoAPI/
├── Controllers/                ← API Endpoints (REST endpoints)
├── Models/                     ← Database entities (data structures)
├── DTOs/                       ← Data Transfer Objects (API request/response formats)
├── Data/                       ← Database context and configuration
├── Services/                   ← Business logic services
├── Properties/                 ← Project settings
├── Program.cs                  ← Application startup configuration
├── appsettings.json           ← Configuration settings
└── VisaoAPI.csproj            ← Project dependencies and settings
```

---

## 📚 **Key Concepts & Terms Explained**

### **🎮 Controllers**
**What it is:** Controllers are the "traffic controllers" of your API. They receive HTTP requests, process them, and return responses.

**Think of it like:** A restaurant waiter who takes your order, tells the kitchen what to make, and brings you your food.

**In this project:**
- `UsersController` - Handles everything about users (create, get, update, delete users)
- `PhotosController` - Manages photo operations (upload, view, edit photos)
- `AlbumsController` - Handles photo albums
- `CommentsController` - Manages comments on photos
- `LikesController` - Handles photo likes
- `FollowersController` - Manages user following relationships
- `TagsController` - Handles photo tags

### **🗄️ Models (Database Entities)**
**What it is:** Models represent the structure of your data - what gets stored in the database.

**Think of it like:** The blueprint of a house - it defines what rooms (properties) the house (object) will have.

**In this project:**
- `User` - Represents a person using the app
- `Photo` - Represents a single photo with metadata
- `Album` - Represents a collection of photos
- `Comment` - Represents a comment on a photo
- `Like` - Represents when someone likes a photo
- `Tag` - Represents hashtags or labels for photos
- `Follower` - Represents following relationships between users

### **📦 DTOs (Data Transfer Objects)**
**What it is:** DTOs are simplified versions of your models used for API communication. They control what data goes in and out of your API.

**Think of it like:** A menu at a restaurant - it shows customers what they can order (input) and what they'll receive (output), but not the kitchen's internal processes.

**Why use them:**
- **Security:** Hide sensitive data (like password hashes)
- **Performance:** Only send necessary data
- **Flexibility:** API structure can differ from database structure

### **🔗 DbContext (Data Layer)**
**What it is:** The bridge between your C# code and the SQL Server database.

**Think of it like:** A translator who speaks both English and Spanish, allowing two people to communicate.

**In this project:**
- `PhotoSharingDbContext` - Manages all database operations and relationships

### **🛠️ Services**
**What it is:** Services contain business logic - the "rules" and complex operations of your application.

**Think of it like:** The kitchen in a restaurant - where the actual work happens according to recipes and procedures.

---

## 🔄 **How the API Works - Complete Flow**

### **1. Application Startup Process**

```csharp
// Program.cs - The entry point of your application
var builder = WebApplication.CreateBuilder(args);

// Step 1: Configure Services (Dependency Injection)
builder.Services.AddDbContext<PhotoSharingDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Step 2: Build the application
var app = builder.Build();

// Step 3: Configure HTTP Pipeline
app.UseSwagger();        // Enable Swagger documentation
app.UseSwaggerUI();      // Enable Swagger UI interface
app.UseAuthorization();  // Enable authorization checks
app.MapControllers();    // Map controller routes

// Step 4: Seed sample data (your addition)
// Create users, photos, albums, etc.

// Step 5: Start the web server
app.Run();
```

### **2. HTTP Request Processing Flow**

When someone makes an API call, here's what happens:

```
1. HTTP Request arrives → 2. Routing → 3. Controller → 4. Database → 5. Response
```

**Example: Getting all photos**

1. **Request:** `GET /api/photos`
2. **Routing:** ASP.NET Core finds `PhotosController.GetPhotos()`
3. **Controller Processing:**
   ```csharp
   public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotos()
   {
       var photos = await _context.Photos
           .Include(p => p.User)        // Join with Users table
           .Include(p => p.Album)       // Join with Albums table
           .Include(p => p.PhotoTags)   // Join with Tags
           .Select(p => new PhotoDto    // Convert to DTO
           {
               PhotoId = p.PhotoId,
               Username = p.User.Username,
               Title = p.Title,
               // ... more properties
           })
           .ToListAsync();              // Execute query
       
       return Ok(photos);               // Return HTTP 200 with data
   }
   ```
4. **Database:** Entity Framework translates this to SQL and executes it
5. **Response:** JSON data is returned to the client

### **3. Database Relationships in Action**

Your API handles complex relationships:

```csharp
// One-to-Many: One User has Many Photos
public class User
{
    public int UserId { get; set; }
    public ICollection<Photo> Photos { get; set; }  // Navigation property
}

public class Photo
{
    public int PhotoId { get; set; }
    public int UserId { get; set; }     // Foreign key
    public User User { get; set; }      // Navigation property
}

// Many-to-Many: Photos can have Many Tags, Tags can be on Many Photos
public class PhotoTag
{
    public int PhotoId { get; set; }    // Composite key part 1
    public int TagId { get; set; }      // Composite key part 2
    public Photo Photo { get; set; }
    public Tag Tag { get; set; }
}
```

---

## 🚀 **API Functionality Breakdown**

### **👥 User Management**
**Purpose:** Handle user accounts and profiles

**Endpoints:**
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get specific user
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user profile
- `DELETE /api/users/{id}` - Delete user

**Key Features:**
- Password hashing for security
- Profile information (bio, profile picture)
- User creation with validation

### **📷 Photo Management**
**Purpose:** Handle photo uploads, viewing, and metadata

**Endpoints:**
- `GET /api/photos` - List all photos
- `GET /api/photos/{id}` - Get specific photo
- `GET /api/photos/user/{userId}` - Get photos by specific user
- `POST /api/photos/user/{userId}` - Upload new photo
- `PUT /api/photos/{id}` - Update photo details
- `DELETE /api/photos/{id}` - Delete photo

**Key Features:**
- Photo metadata (title, description, upload date)
- User ownership tracking
- Album association
- Tag management

### **📂 Album Management**
**Purpose:** Organize photos into collections

**Endpoints:**
- `GET /api/albums` - List all albums
- `GET /api/albums/{id}` - Get specific album
- `GET /api/albums/{id}/photos` - Get photos in album
- `POST /api/albums/user/{userId}` - Create new album
- `PUT /api/albums/{id}` - Update album
- `DELETE /api/albums/{id}` - Delete album

### **💬 Social Features**

#### **Comments System**
- `GET /api/comments/photo/{photoId}` - Get photo comments
- `POST /api/comments/photo/{photoId}/user/{userId}` - Add comment
- `DELETE /api/comments/{id}` - Delete comment

#### **Likes System**
- `GET /api/likes/photo/{photoId}` - Get photo likes
- `POST /api/likes/photo/{photoId}/user/{userId}` - Like photo
- `DELETE /api/likes/photo/{photoId}/user/{userId}` - Unlike photo

#### **Following System**
- `POST /api/followers/{followerId}/follow/{followingId}` - Follow user
- `DELETE /api/followers/{followerId}/unfollow/{followingId}` - Unfollow user
- `GET /api/followers/{userId}/followers` - Get followers
- `GET /api/followers/{userId}/following` - Get following

### **🏷️ Tagging System**
**Purpose:** Categorize and search photos

**Endpoints:**
- `GET /api/tags` - Get all tags
- `GET /api/tags/{tagName}/photos` - Find photos by tag
- `GET /api/tags/popular` - Get most used tags

---

## 🛢️ **Database Schema Explained**

### **Core Tables:**
1. **Users** - User accounts and profiles
2. **Albums** - Photo collections
3. **Photos** - Individual photos with metadata
4. **Tags** - Hashtags/labels
5. **Comments** - Photo comments
6. **Likes** - Photo likes
7. **Followers** - User relationships
8. **Photo_Tags** - Many-to-many link between photos and tags

### **Relationship Types:**
- **One-to-Many:** User → Photos, User → Albums
- **Many-to-Many:** Photos ↔ Tags (via Photo_Tags)
- **Self-Referencing:** Users ↔ Users (via Followers)

---

## ⚙️ **Configuration & Setup**

### **Dependencies (VisaoAPI.csproj):**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
```

### **Database Connection (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PhotoSharingDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### **Sample Data Auto-Creation:**
The API automatically creates sample data on startup:
- 2 Users (johndoe, janesmith)
- 2 Albums with descriptions
- 4 Photos with titles and descriptions
- Tags, likes, comments, and follow relationships

---

## 🔍 **Testing Your API**

### **Swagger UI:**
Access `http://localhost:5241` to get an interactive interface where you can:
- View all available endpoints
- Test API calls directly in the browser
- See request/response formats
- Understand parameter requirements

### **Sample Test Scenarios:**
1. **List Users:** `GET /api/users` → See johndoe and janesmith
2. **View Photos:** `GET /api/photos` → See all sample photos
3. **User's Albums:** `GET /api/albums/user/1` → See johndoe's albums
4. **Photo Details:** `GET /api/photos/1` → See full photo information
5. **Social Data:** `GET /api/followers/1/followers` → See who follows johndoe

---

## 🎯 **What Makes This API Professional**

### **✅ Best Practices Implemented:**
- **RESTful Design:** Standard HTTP methods and status codes
- **Entity Framework:** Professional ORM for database operations
- **DTOs:** Proper API data contracts
- **Async/Await:** Non-blocking database operations
- **Dependency Injection:** Loose coupling and testability
- **Swagger Documentation:** Professional API documentation
- **Error Handling:** Graceful error responses
- **Validation:** Input validation and security checks

### **🔒 Security Features:**
- Password hashing (not plain text storage)
- Input validation
- Proper HTTP status codes
- Separation of concerns

### **📈 Performance Features:**
- Async database operations
- Efficient LINQ queries
- Proper data relationships
- Optimized JSON serialization

---

## 🚀 **Next Steps & Extensions**

Your API is production-ready for basic functionality. You could extend it with:

1. **Authentication & Authorization** (JWT tokens)
2. **File Upload** for actual image storage
3. **Pagination** for large datasets
4. **Search & Filtering** capabilities
5. **Caching** for better performance
6. **Rate Limiting** for API protection
7. **Unit Testing** for reliability

**Your Photo Sharing API is a solid foundation for a social media platform!** 🎉