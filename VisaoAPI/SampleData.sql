-- SQL Server compatible sample data for Photo Sharing Database
USE PhotoSharingDb;
GO

-- Insert Users
INSERT INTO Users (username, email, password_hash, full_name, bio, profile_pic, created_at) VALUES
('johndoe', 'john@example.com', 'hashed_password_123', 'John Doe', 'Landscape photographer.', 'profile1.jpg', GETDATE()),
('janesmith', 'jane@example.com', 'hashed_password_456', 'Jane Smith', 'Wedding photographer.', 'profile2.jpg', GETDATE());

-- Insert Albums
INSERT INTO Albums (user_id, title, description, created_at) VALUES
(1, 'Nature Escapes', 'Collection of stunning landscape shots.', GETDATE()),
(2, 'Forever Moments', 'Beautiful wedding memories.', GETDATE());

-- Insert Photos
INSERT INTO Photos (user_id, album_id, title, description, image_url, uploaded_at) VALUES
(1, 1, 'Mountain Sunrise', 'Sunrise over the peaks.', 'mountain_sunrise.jpg', GETDATE()),
(1, 1, 'Forest Stream', 'Calm stream running through forest.', 'forest_stream.jpg', GETDATE()),
(2, 2, 'Wedding Kiss', 'Couple sharing their first kiss.', 'wedding_kiss.jpg', GETDATE()),
(2, 2, 'Reception Fun', 'Guests enjoying the reception.', 'reception_fun.jpg', GETDATE());

-- Insert Tags
INSERT INTO Tags (tag_name) VALUES
('Landscape'),
('Wedding');

-- Link Photos to Tags
INSERT INTO Photo_Tags (photo_id, tag_id) VALUES
(1, 1),
(2, 1),
(3, 2),
(4, 2);

-- Insert Followers (mutual follow)
INSERT INTO Followers (follower_id, following_id, followed_at) VALUES
(1, 2, GETDATE()),
(2, 1, GETDATE());

-- Insert Likes (each likes both of other's photos)
INSERT INTO Likes (user_id, photo_id, liked_at) VALUES
(1, 3, GETDATE()),
(1, 4, GETDATE()),
(2, 1, GETDATE()),
(2, 2, GETDATE());

-- Insert Comments (generic positive comments)
INSERT INTO Comments (photo_id, user_id, comment_text, commented_at) VALUES
(3, 1, 'Great job! Beautiful moment.', GETDATE()),
(4, 1, 'Good view! Love the lighting.', GETDATE()),
(1, 2, 'Amazing capture! Great job!', GETDATE()),
(2, 2, 'Beautiful scenery!', GETDATE());

-- Verify the data was inserted correctly
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Albums', COUNT(*) FROM Albums
UNION ALL
SELECT 'Photos', COUNT(*) FROM Photos
UNION ALL
SELECT 'Tags', COUNT(*) FROM Tags
UNION ALL
SELECT 'Photo_Tags', COUNT(*) FROM Photo_Tags
UNION ALL
SELECT 'Followers', COUNT(*) FROM Followers
UNION ALL
SELECT 'Likes', COUNT(*) FROM Likes
UNION ALL
SELECT 'Comments', COUNT(*) FROM Comments;