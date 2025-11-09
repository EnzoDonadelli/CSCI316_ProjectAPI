CREATE TABLE Photo_Tags (
    photo_id INT NOT NULL,
    tag_id INT NOT NULL,
    PRIMARY KEY (photo_id, tag_id),
    FOREIGN KEY (photo_id) REFERENCES Photos(photo_id),
    FOREIGN KEY (tag_id) REFERENCES Tags(tag_id)
);