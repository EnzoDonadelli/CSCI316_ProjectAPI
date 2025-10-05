# 📸 Photo Sharing API - Sample Data Implementation Guide

## 🎯 How I Implemented Sample Data Population

I successfully integrated your sample data into the Photo Sharing API using multiple approaches to ensure reliability and ease of use. Here's exactly how I achieved this:

### 🔧 **Implementation Strategy**

I created a robust solution that works in two ways:
1. **SQL Script Execution** (Primary method)
2. **Programmatic Seeding** (Fallback method)

---

## 📁 **Files Created for Sample Data**

### 1. **SampleData.sql** - SQL Server Compatible Script
I converted your MySQL-based sample data to SQL Server format:

**Key Changes Made:**
- Changed `NOW()` to `GETDATE()` (SQL Server function)
- Added `USE PhotoSharingDb;` directive
- Added `GO` statements for batch execution
- Added verification queries to confirm data insertion

**Original vs. Converted:**
```sql
-- Your Original (MySQL)
created_at DATETIME DEFAULT NOW()

-- My Conversion (SQL Server)  
created_at DATETIME DEFAULT GETDATE()
```

### 2. **DatabaseSeeder.cs** - Intelligent Seeding Service
I created a comprehensive service that:

**Features:**
- ✅ **Checks if data already exists** (prevents duplicates)
- ✅ **Attempts SQL file execution first**
- ✅ **Falls back to programmatic seeding if SQL fails**
- ✅ **Comprehensive error handling and logging**
- ✅ **Maintains referential integrity**

**Smart Logic:**
```csharp
// Check if data already exists
if (await _context.Users.AnyAsync())
{
    _logger.LogInformation("Database already contains data. Skipping seeding.");
    return;
}
```

### 3. **Program.cs Integration**
I modified the startup process to automatically seed data:

```csharp
// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PhotoSharingDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    
    // Create database
    context.Database.EnsureCreated();
    
    // Populate with sample data
    await seeder.SeedAsync();
}
```

---

## 🎯 **Sample Data Structure Created**

### **Users (2 records):**
| Username | Email | Full Name | Bio |
|----------|--------|-----------|-----|
| johndoe | john@example.com | John Doe | Landscape photographer |
| janesmith | jane@example.com | Jane Smith | Wedding photographer |

### **Albums (2 records):**
| Title | Owner | Description |
|-------|--------|-------------|
| Nature Escapes | johndoe | Collection of stunning landscape shots |
| Forever Moments | janesmith | Beautiful wedding memories |

### **Photos (4 records):**
| Title | Owner | Album | Description |
|-------|--------|-------|-------------|
| Mountain Sunrise | johndoe | Nature Escapes | Sunrise over the peaks |
| Forest Stream | johndoe | Nature Escapes | Calm stream running through forest |
| Wedding Kiss | janesmith | Forever Moments | Couple sharing their first kiss |
| Reception Fun | janesmith | Forever Moments | Guests enjoying the reception |

### **Tags (2 records):**
- `Landscape` (linked to johndoe's photos)
- `Wedding` (linked to janesmith's photos)

### **Social Interactions:**
- **Mutual Following:** johndoe ↔ janesmith
- **Cross-Likes:** Each user likes the other's photos
- **Comments:** Positive comments on each other's photos

---

## 🚀 **How to Run and Test**

### **Method 1: Using Batch Files**
I created two batch files for easy startup:

```bash
# Simple version
.\run-api.bat

# Detailed version with build info
.\start-api.bat
```

### **Method 2: Manual Commands**
```bash
cd VisaoAPI
dotnet build
dotnet run
```

### **Method 3: Visual Studio**
- Open the solution in Visual Studio
- Set VisaoAPI as startup project
- Press F5 or click "Start"

---

## 🔍 **Testing the Sample Data**

Once the API is running, you can test the sample data through:

### **Swagger UI** (Recommended)
Visit: `https://localhost:5001` or `http://localhost:5000`

### **Sample API Calls to Verify Data:**

1. **Get All Users:**
   ```
   GET /api/users
   ```

2. **Get John's Photos:**
   ```
   GET /api/photos/user/1
   ```

3. **Get Jane's Albums:**
   ```
   GET /api/albums/user/2
   ```

4. **Check Followers:**
   ```
   GET /api/followers/1/followers
   ```

5. **View Photo Comments:**
   ```
   GET /api/comments/photo/1
   ```

---

## 🛠 **Technical Implementation Details**

### **Database Relationship Handling:**
I carefully implemented the foreign key relationships:

```csharp
// Example: Creating a photo with proper relationships
var photo = new Photo
{
    UserId = user1.UserId,           // Links to User
    AlbumId = album1.AlbumId,        // Links to Album  
    Title = "Mountain Sunrise",
    Description = "Sunrise over the peaks.",
    ImageUrl = "mountain_sunrise.jpg",
    UploadedAt = DateTime.Now
};
```

### **Many-to-Many Relationships:**
```csharp
// Photo-Tag relationships
var photoTag = new PhotoTag 
{ 
    PhotoId = photo1.PhotoId, 
    TagId = tag1.TagId 
};
```

### **Error Prevention:**
- ✅ Prevents duplicate seeding
- ✅ Handles SQL execution failures
- ✅ Maintains data integrity
- ✅ Provides detailed logging

---

## 📊 **Data Verification**

The seeder includes a verification query that shows record counts:

```sql
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Albums', COUNT(*) FROM Albums
UNION ALL
SELECT 'Photos', COUNT(*) FROM Photos
-- ... etc
```

Expected Results:
- Users: 2
- Albums: 2  
- Photos: 4
- Tags: 2
- Photo_Tags: 4
- Followers: 2
- Likes: 4
- Comments: 4

---

## 🎉 **Benefits of This Approach**

1. **Automatic:** Data populates on first run
2. **Safe:** Won't create duplicates
3. **Robust:** Multiple fallback methods
4. **Testable:** Immediate data to work with
5. **Realistic:** Proper relationships and interactions
6. **Flexible:** Easy to modify or extend

---

## 🔧 **Customization Options**

To modify the sample data:

1. **Edit SampleData.sql** for SQL-based changes
2. **Modify DatabaseSeeder.cs** for programmatic changes  
3. **Add more data** by extending the seeder methods
4. **Change relationships** by updating foreign key assignments

The system will automatically use your updated data on the next clean run!