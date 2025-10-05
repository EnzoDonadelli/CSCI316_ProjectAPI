
-- Sample Data for Photo Sharing & Portfolio App (MySQL 2019 compatible)
Use dbo;
-- Insert Users
INSERT INTO Users (username, email, password_hash, full_name, bio, profile_pic, created_at) VALUES
('johndoe', 'john@example.com', 'hashed_password_123', 'John Doe', 'Landscape photographer.', 'profile1.jpg', NOW()),
('janesmith', 'jane@example.com', 'hashed_password_456', 'Jane Smith', 'Wedding photographer.', 'profile2.jpg', NOW());
GO

-- Insert Albums
INSERT INTO Albums (user_id, title, description, created_at) VALUES
(1, 'Nature Escapes', 'Collection of stunning landscape shots.', NOW()),
(2, 'Forever Moments', 'Beautiful wedding memories.', NOW());
GO

-- Insert Photos
INSERT INTO Photos (user_id, album_id, title, description, image_url, uploaded_at) VALUES
(1, 1, 'Mountain Sunrise', 'Sunrise over the peaks.', 'mountain_sunrise.jpg', NOW()),
(1, 1, 'Forest Stream', 'Calm stream running through forest.', 'forest_stream.jpg', NOW()),
(2, 2, 'Wedding Kiss', 'Couple sharing their first kiss.', 'wedding_kiss.jpg', NOW()),
(2, 2, 'Reception Fun', 'Guests enjoying the reception.', 'reception_fun.jpg', NOW());
GO

-- Insert Tags
INSERT INTO Tags (tag_name) VALUES
('Landscape'),
('Wedding');
GO

-- Link Photos to Tags
INSERT INTO Photo_Tags (photo_id, tag_id) VALUES
(1, 1),
(2, 1),
(3, 2),
(4, 2);
GO

-- Insert Followers (mutual follow)
INSERT INTO Followers (follower_id, following_id, followed_at) VALUES
(1, 2, NOW()),
(2, 1, NOW());
GO

-- Insert Likes (each likes both of otherâ€™s photos)
INSERT INTO Likes (user_id, photo_id, liked_at) VALUES
(1, 3, NOW()),
(1, 4, NOW()),
(2, 1, NOW()),
(2, 2, NOW());
GO

-- Insert Comments (generic positive comments)
INSERT INTO Comments (photo_id, user_id, comment_text, commented_at) VALUES
(3, 1, 'Great job! Beautiful moment.', NOW()),
(4, 1, 'Good view! Love the lighting.', NOW()),
(1, 2, 'Amazing capture! Great job!', NOW()),
(2, 2, 'Beautiful scenery!', NOW());
GO
