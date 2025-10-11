# Photo Sharing API - 5 Minute Presentation

## 🎯 Project Overview (30 seconds)

**What**: Complete RESTful API for a photo sharing social platform  
**Tech Stack**: ASP.NET Core 8, SQL Server, JWT Authentication, Dapper ORM  
**Architecture**: Clean layered architecture with Repository pattern  

---

## 🔑 Key Features (1 minute)

### 1. **JWT Authentication System**
- Secure user registration and login
- Password hashing with BCrypt
- Token-based authentication for all protected endpoints

### 2. **Core Functionality**
- **Users**: Registration, profiles, following system
- **Photos**: Upload, organize, tag, like, comment
- **Albums**: Create collections, manage photo organization
- **Social Features**: Follow users, like photos, add comments

### 3. **API Documentation**
- Swagger UI with JWT authentication support
- Comprehensive endpoint documentation
- Interactive testing interface

---

## 🏗️ Architecture Highlights (1.5 minutes)

### **Clean Architecture Implementation**
```
Controllers → Services → Repositories → Database
```

### **Repository Pattern with Dapper**
- **Before**: Controllers directly using Entity Framework
- **After**: Clean separation with repository interfaces

```csharp
// Clean controller code
[HttpGet]
public async Task<ActionResult<IEnumerable<PhotoDto>>> GetMyPhotos()
{
    var photos = await _photoRepository.GetByUserIdAsync(userId);
    return Ok(photos.Select(p => new PhotoDto { ... }));
}
```

### **Benefits Achieved**
✅ **Testable**: Interface-based design enables easy unit testing  
✅ **Maintainable**: Clear separation of concerns  
✅ **Performant**: Optimized SQL queries with Dapper  
✅ **Secure**: Parameterized queries prevent SQL injection  

---

## 🔒 Security Implementation (1 minute)

### **JWT Authentication Flow**
```csharp
[HttpPost("login")]
public async Task<ActionResult<AuthResultDto>> Login(LoginDto loginDto)
{
    // 1. Validate credentials with BCrypt
    // 2. Generate JWT token
    // 3. Return token + user info
}
```

### **Protected Endpoints**
```csharp
[Authorize] // JWT required
[HttpPost]
public async Task<ActionResult> CreatePhoto(CreatePhotoDto dto)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // Only authenticated users can create photos
}
```

### **Authorization Levels**
- **Public**: Registration, login
- **Authenticated**: View photos, create content
- **Owner-only**: Edit/delete own content

---

## 🚀 Technical Excellence (1 minute)

### **Database Design**
- **Persistent Database**: Data survives application restarts
- **Proper Relationships**: Users ↔ Photos ↔ Albums ↔ Comments ↔ Likes
- **Sample Data**: Pre-seeded with test users and content

### **API Quality**
```csharp
// Example: Clean error handling
try
{
    var result = await _photoRepository.CreateAsync(photo);
    return CreatedAtAction(nameof(GetPhoto), new { id = result.PhotoId }, photoDto);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating photo");
    return StatusCode(500, "An error occurred while creating the photo");
}
```

### **Development Experience**
- **Swagger Integration**: Interactive API documentation
- **Logging**: Comprehensive error tracking
- **Configuration**: Environment-based settings

---

## 🎮 Live Demo (1 minute)

### **API Endpoints Showcase**
1. **Authentication**: `POST /api/auth/register` & `POST /api/auth/login`
2. **Photos**: `GET /api/myphotos` (requires JWT)
3. **Albums**: `GET /api/albums` & `POST /api/albums`
4. **Social**: `GET /api/photos` (public feed)

### **Swagger UI Demo**
- Navigate to: `http://localhost:5241/swagger`
- Show JWT authorization button
- Test authenticated endpoints
- Demonstrate error handling

### **Database Persistence**
- Show data survives application restarts
- Demonstrate user sessions maintained
- Display proper relational data

---

## 📊 Project Metrics

| Metric | Value |
|--------|-------|
| **Total Endpoints** | 25+ RESTful endpoints |
| **Controllers** | 8 specialized controllers |
| **Database Tables** | 9 normalized tables |
| **Authentication** | JWT with BCrypt hashing |
| **Architecture** | 3-layer with Repository pattern |
| **Documentation** | Complete Swagger integration |

---

## 🎯 Key Takeaways (30 seconds)

### **What Makes This API Special**
1. **Production-Ready**: Proper authentication, error handling, logging
2. **Clean Code**: Repository pattern, dependency injection, SOLID principles
3. **Scalable**: Interface-based design allows easy testing and modifications
4. **Secure**: JWT authentication, password hashing, SQL injection prevention
5. **User-Friendly**: Comprehensive API documentation with Swagger

### **Perfect For**
- Social media applications
- Photo sharing platforms
- Learning modern API development
- Demonstrating clean architecture principles

---

## 🚀 Quick Start
```bash
git clone [repository]
cd CSCI316_ProjectAPI/VisaoAPI
dotnet run
# Navigate to: http://localhost:5241/swagger
```

**Live API**: Ready to test with pre-seeded data including users `alice`, `bob`, and `charlie` (all with password `password123`)

---

*This API demonstrates modern .NET development practices with clean architecture, proper authentication, and comprehensive documentation - perfect for real-world applications.*