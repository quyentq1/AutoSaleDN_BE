-- Complete SQL Server Schema for Car Sales Website (with review video upload support)

USE AutoSaleDN
GO

CREATE TABLE Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL, -- Hashed password
    email VARCHAR(100) NOT NULL UNIQUE,
    fullName VARCHAR(100) NOT NULL,
    mobile VARCHAR(15),
    role VARCHAR(20) NOT NULL CHECK (Role IN ('Guest', 'Customer', 'Seller', 'Admin')),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

CREATE TABLE CarManufacturers (
    manufacturer_id INT PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(255) NOT NULL UNIQUE
);

CREATE TABLE CarModels (
    model_id INT PRIMARY KEY IDENTITY(1,1),
    manufacturer_id INT FOREIGN KEY REFERENCES CarManufacturers(manufacturer_id),
    name VARCHAR(255) NOT NULL
);

CREATE TABLE CarListings (
    listing_id INT PRIMARY KEY IDENTITY(1,1),
    model_id INT FOREIGN KEY REFERENCES CarModels(model_id),
    user_id INT FOREIGN KEY REFERENCES Users(user_id),
    year INT,
    mileage INT,
    price DECIMAL(10, 2),
    location VARCHAR(255),
    condition VARCHAR(20) CHECK (condition IN ('Excellent', 'Good', 'Fair')),
    listing_status VARCHAR(20) DEFAULT 'active' CHECK (listing_status IN ('active', 'sold', 'rented', 'requested')),
    date_posted DATETIME DEFAULT GETDATE(),
    date_updated DATETIME DEFAULT GETDATE(),
    certified BIT DEFAULT 0,
    vin VARCHAR(255),
    description VARCHAR(MAX),
    RentSell VARCHAR(4) CHECK (RentSell IN ('Rent', 'Sell'))
);

CREATE TABLE CarSpecifications (
    specification_id INT PRIMARY KEY IDENTITY(1,1),
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    engine VARCHAR(255),
    transmission VARCHAR(255),
    fuelType VARCHAR(255),
    seatingCapacity INT,
    interiorColor VARCHAR(255),
    exteriorColor VARCHAR(255),
    carType VARCHAR(255)
);

CREATE TABLE CarFeatures (
    feature_id INT PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(255) NOT NULL UNIQUE  --  e.g., 'GPS', 'Sunroof'
);

CREATE TABLE CarListingFeatures (
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    feature_id INT FOREIGN KEY REFERENCES CarFeatures(feature_id),
    PRIMARY KEY (listing_id, feature_id)
);

CREATE TABLE CarServiceHistory (
    history_id INT PRIMARY KEY IDENTITY(1,1),
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    recentServicing BIT DEFAULT 0,
    noAccidentHistory BIT DEFAULT 0,
    modifications BIT DEFAULT 0
);

CREATE TABLE CarPricingDetails (
    pricing_detail_id INT PRIMARY KEY IDENTITY(1,1),
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    taxRate DECIMAL(5, 4) DEFAULT 0.08,
    registrationFee DECIMAL(10, 2) DEFAULT 300
);

CREATE TABLE CarImages (
    image_id INT PRIMARY KEY IDENTITY(1,1),
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    url VARCHAR(MAX),
    filename VARCHAR(255)
);

CREATE TABLE Bookings (
    booking_id INT PRIMARY KEY IDENTITY(1,1),
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    user_id INT FOREIGN KEY REFERENCES Users(user_id),
    booking_start_date DATE NOT NULL,
    booking_end_date DATE NOT NULL,
    total_price DECIMAL(10, 2) NOT NULL,
    paid_price DECIMAL(10, 2) NOT NULL,
    booking_status VARCHAR(20) DEFAULT 'pending' CHECK (booking_status IN ('pending', 'confirmed', 'canceled', 'completed')),
    payment_status VARCHAR(20) DEFAULT 'pending' CHECK (payment_status IN ('pending', 'paid', 'failed')),
    transaction_id VARCHAR(255) UNIQUE
);

CREATE TABLE Payments (
    payment_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT FOREIGN KEY REFERENCES Users(user_id),
    transaction_id VARCHAR(255) UNIQUE NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    payment_method VARCHAR(20) CHECK (payment_method IN ('credit_card', 'debit_card', 'upi', 'paypal')),
    payment_status VARCHAR(20) DEFAULT 'pending' CHECK (payment_status IN ('pending', 'success', 'failed', 'refunded')),
    date_of_payment DATETIME DEFAULT GETDATE(),
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    booking_id INT FOREIGN KEY REFERENCES Bookings(booking_id),
    additional_details VARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE Reports (
    report_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT FOREIGN KEY REFERENCES Users(user_id),
    report_type VARCHAR(20) NOT NULL CHECK (report_type IN ('daily', 'weekly', 'monthly', 'custom')),
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_listings INT,
    active_listings INT,
    sold_listings INT,
    rented_listings INT,
    average_listing_price DECIMAL(10, 2),
    total_listing_value DECIMAL(10, 2),
    total_bookings INT,
    pending_bookings INT,
    confirmed_bookings INT,
    canceled_bookings INT,
    completed_bookings INT,
    total_booking_value DECIMAL(10, 2),
    total_payments INT,
    successful_payments INT,
    failed_payments INT,
    pending_payments INT,
    refunded_payments INT,
    total_revenue DECIMAL(10, 2),
    total_reviews INT,
    average_rating DECIMAL(3, 2),
    five_star_reviews INT,
    four_star_reviews INT,
    three_star_reviews INT,
    two_star_reviews INT,
    one_star_reviews INT,
    generated_at DATETIME DEFAULT GETDATE(),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Modified Reviews Table: Add image_url and video_url fields
CREATE TABLE Reviews (
    review_id INT PRIMARY KEY IDENTITY(1,1),
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    user_id INT FOREIGN KEY REFERENCES Users(user_id),
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    feedback VARCHAR(MAX),
    image_url VARCHAR(512), -- URL of image uploaded to cloud
    video_url VARCHAR(512), -- URL of video uploaded to cloud
    createdAt DATETIME DEFAULT GETDATE(),
    UNIQUE (listing_id, user_id)
);

CREATE TABLE BlogCategories (
    category_id INT PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(255) NOT NULL UNIQUE,
    description VARCHAR(MAX)
);

CREATE TABLE BlogPosts (
    post_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT FOREIGN KEY REFERENCES Users(user_id),
    category_id INT FOREIGN KEY REFERENCES BlogCategories(category_id),
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    content VARCHAR(MAX) NOT NULL,
    published_date DATETIME,
    is_published BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE BlogTags (
    tag_id INT PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(255) NOT NULL UNIQUE
);

CREATE TABLE BlogPostTags (
    post_id INT FOREIGN KEY REFERENCES BlogPosts(post_id),
    tag_id INT FOREIGN KEY REFERENCES BlogTags(tag_id),
    PRIMARY KEY (post_id, tag_id)
);

CREATE TABLE SaleStatus (
    sale_status_id INT PRIMARY KEY IDENTITY(1,1),
    status_name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE CarSales (
    sale_id INT PRIMARY KEY IDENTITY(1,1),
    listing_id INT FOREIGN KEY REFERENCES CarListings(listing_id),
    booking_id INT FOREIGN KEY REFERENCES Bookings(booking_id) NULL,
    sale_status_id INT FOREIGN KEY REFERENCES SaleStatus(sale_status_id),
    sale_date DATETIME,
    final_price DECIMAL(10, 2),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE PaymentTransactions (
    transaction_log_id INT PRIMARY KEY IDENTITY(1,1),
    payment_id INT FOREIGN KEY REFERENCES Payments(payment_id),
    transaction_date DATETIME DEFAULT GETDATE(),
    gateway_response_code VARCHAR(50),
    gateway_response_message VARCHAR(255),
    transaction_status VARCHAR(20) CHECK (transaction_status IN ('pending', 'success', 'failed', 'refunded')),
    additional_details VARCHAR(MAX)
);