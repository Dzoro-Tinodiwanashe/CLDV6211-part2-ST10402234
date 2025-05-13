-- Database creation
USE EventEaseDatabase;

-- Drop dependent tables first
DROP TABLE IF EXISTS Booking;
DROP TABLE IF EXISTS Event;
DROP TABLE IF EXISTS Venue;

-- Create the Venue Table
DROP TABLE IF EXISTS Venue;
CREATE TABLE Venue (
    venueID INT IDENTITY(1,1) PRIMARY KEY,
    venueName VARCHAR(250),
    location VARCHAR(250),
    capacity INT,
    imageUrl VARCHAR(500) -- Increased size to hold longer URLs
);

-- Event table
DROP TABLE IF EXISTS Event;
CREATE TABLE Event (
    eventID INT IDENTITY(1,1) PRIMARY KEY,
    eventName VARCHAR(250),
    eventDate DATE,
    description VARCHAR(250),
    imageUrl VARCHAR(500), -- Increased size to hold longer URLs
    venueID INT,
    FOREIGN KEY (venueID) REFERENCES Venue (venueID)
);

-- Booking table, associative linking Venue and Event
DROP TABLE IF EXISTS Booking;
CREATE TABLE Booking (
    bookingID INT IDENTITY(1,1) PRIMARY KEY,
    eventID INT,
    venueID INT,
    FOREIGN KEY (eventID) REFERENCES Event (eventID),
    FOREIGN KEY (venueID) REFERENCES Venue (venueID),
    bookingDate DATE
);

-- Insert sample data into Venue
INSERT INTO Venue (VenueName, Location, Capacity, ImageUrl)
VALUES ('The Grand Ballroom', 'Johannesburg', 200, 'imageurl1.jpg');

-- Insert sample data into Event
INSERT INTO Event (EventName, Description, ImageUrl, EventDate, VenueID)
VALUES ('Wedding Party', 'A celebration for the unity of Mr & Mrs Jacobbs', 'eventimage.jpg', '2025-05-01', 1);

-- Insert sample data into Booking
INSERT INTO Booking (EventID, VenueID, BookingDate)
VALUES (1, 1, '2025-04-07');

-- Select data from tables for verification
SELECT * FROM Venue;
SELECT * FROM Event;
SELECT * FROM Booking;
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Venue';
SELECT * FROM Event WHERE EventID = 4;
