-- Create a table to store production data
CREATE TABLE IF NOT EXISTS production_data
(
    id              INT AUTO_INCREMENT PRIMARY KEY,
    name            VARCHAR(255),
    qualified_count INT,
    defective_count INT,
    target_amount   INT,
    date            DATETIME
) DEFAULT CHARSET = utf8mb4;

-- Create a table to store daily target for each name
CREATE TABLE IF NOT EXISTS daily_target
(
    id            INT AUTO_INCREMENT PRIMARY KEY,
    name          VARCHAR(255),
    target_amount INT,
    date          DATE,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) DEFAULT CHARSET = utf8mb4;

-- Create a table to store detailed production data
CREATE TABLE IF NOT EXISTS production_details
(
    id              INT AUTO_INCREMENT PRIMARY KEY,
    name            VARCHAR(255),
    operator_id     INT,
    machine_id      INT,
    is_qualified    BOOLEAN,
    production_time DATETIME,
    last_modified   TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) DEFAULT CHARSET = utf8mb4;

