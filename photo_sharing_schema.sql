
CREATE TABLE Users (
    user_id INT PRIMARY KEY IDENTITY(1,1),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    bio TEXT,
    profile_pic VARCHAR(255),
    created_at DATETIME DEFAULT GETDATE()
);

CREATE TABLE Albums (
    album_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    title VARCHAR(150) NOT NULL,
    description TEXT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

CREATE TABLE Photos (
    photo_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    album_id INT,
    title VARCHAR(150),
    description TEXT,
    image_url VARCHAR(255) NOT NULL,
    uploaded_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (album_id) REFERENCES Albums(album_id)
);

CREATE TABLE Tags (
    tag_id INT PRIMARY KEY IDENTITY(1,1),
    tag_name VARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE Photo_Tags (
    photo_id INT NOT NULL,
    tag_id INT NOT NULL,
    PRIMARY KEY (photo_id, tag_id),
    FOREIGN KEY (photo_id) REFERENCES Photos(photo_id),
    FOREIGN KEY (tag_id) REFERENCES Tags(tag_id)
);

CREATE TABLE Likes (
    like_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    photo_id INT NOT NULL,
    liked_at DATETIME DEFAULT GETDATE(),
    UNIQUE (user_id, photo_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (photo_id) REFERENCES Photos(photo_id)
);

CREATE TABLE Comments (
    comment_id INT PRIMARY KEY IDENTITY(1,1),
    photo_id INT NOT NULL,
    user_id INT NOT NULL,
    comment_text TEXT NOT NULL,
    commented_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (photo_id) REFERENCES Photos(photo_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

CREATE TABLE Followers (
    follower_id INT NOT NULL,
    following_id INT NOT NULL,
    followed_at DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (follower_id, following_id),
    FOREIGN KEY (follower_id) REFERENCES Users(user_id),
    FOREIGN KEY (following_id) REFERENCES Users(user_id)
);
