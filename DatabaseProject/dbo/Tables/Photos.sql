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