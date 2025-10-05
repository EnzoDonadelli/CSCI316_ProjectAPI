CREATE TABLE Albums (
    album_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    title VARCHAR(150) NOT NULL,
    description TEXT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);