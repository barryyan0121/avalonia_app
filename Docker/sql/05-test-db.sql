-- Create the test_db database
CREATE DATABASE IF NOT EXISTS test_db CHARACTER SET utf8 COLLATE utf8_general_ci;

-- Use the test_db database
USE test_db;

-- Grant all privileges to sample_user for the test_db database
GRANT ALL PRIVILEGES ON test_db.* TO 'sample_user'@'%';

-- Flush privileges to ensure the grant takes effect
FLUSH PRIVILEGES;

-- Create the production_data table
CREATE TABLE IF NOT EXISTS production_data (
     id INT AUTO_INCREMENT PRIMARY KEY,
     name VARCHAR(255) NOT NULL,
    qualified_count INT NOT NULL,
    defective_count INT NOT NULL,
    target_amount INT NOT NULL,
    date DATE NOT NULL
);

-- Create the production_details table
CREATE TABLE IF NOT EXISTS production_details (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    operator_id INT NOT NULL,
    machine_id INT NOT NULL,
    is_qualified BOOLEAN NOT NULL,
    production_time DATETIME NOT NULL
);
