CREATE TABLE Followers (
    follower_id INT NOT NULL,
    following_id INT NOT NULL,
    followed_at DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (follower_id, following_id),
    FOREIGN KEY (follower_id) REFERENCES Users(user_id),
    FOREIGN KEY (following_id) REFERENCES Users(user_id)
);