-- Create the production_data table
CREATE TABLE IF NOT EXISTS production_data
(
    id              INT AUTO_INCREMENT PRIMARY KEY,
    name            VARCHAR(255),
    qualified_count INT,
    defective_count INT,
    qualified_rate  DOUBLE,
    total_count     INT,
    date            DATETIME
) DEFAULT CHARSET = utf8mb4;

-- Insert sample data
INSERT INTO production_data (name, qualified_count, defective_count, qualified_rate, total_count, date)
VALUES ('胶纸切割', 100, 5, 0.95, 105, '2024-05-10 08:00:00'),
       ('板框焊接', 120, 8, 0.94, 128, '2024-05-10 08:30:00'),
       ('板组件A', 90, 10, 0.9, 100, '2024-05-10 09:00:00'),
       ('板组件B', 110, 12, 0.88, 122, '2024-05-10 09:30:00'),
       ('框膜组件检测', 115, 7, 0.94, 122, '2024-05-10 10:00:00'),
       ('膜框组件A', 105, 15, 0.87, 120, '2024-05-10 10:30:00'),
       ('膜框组件B', 95, 3, 0.97, 98, '2024-05-10 11:00:00'),
       ('三合一电池A', 125, 9, 0.93, 134, '2024-05-10 11:30:00'),
       ('三合一电池B', 85, 6, 0.93, 91, '2024-05-10 12:00:00'),
       ('三合一电池C', 95, 5, 0.95, 100, '2024-05-10 12:30:00'),
       ('三合一电池检测', 130, 2, 0.98, 132, '2024-05-10 13:00:00'),
       ('总装线', 120, 10, 0.92, 130, '2024-05-10 15:00:00');
