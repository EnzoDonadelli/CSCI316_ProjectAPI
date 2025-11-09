CREATE TABLE Comments (
    comment_id INT PRIMARY KEY IDENTITY(1,1),
    photo_id INT NOT NULL,
    user_id INT NOT NULL,
    comment_text TEXT NOT NULL,
    commented_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (photo_id) REFERENCES Photos(photo_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);