CREATE TABLE Likes (
    like_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    photo_id INT NOT NULL,
    liked_at DATETIME DEFAULT GETDATE(),
    UNIQUE (user_id, photo_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (photo_id) REFERENCES Photos(photo_id)
);