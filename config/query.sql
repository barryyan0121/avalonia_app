CREATE TABLE IF NOT EXISTS production_data
(
    id              INT AUTO_INCREMENT PRIMARY KEY,
    name            VARCHAR(255),
    qualified_count INT,
    defective_count INT,
    qualified_rate  DOUBLE,
    total_count     INT,
    date            DATETIME
) DEFAULT CHARSET = utf8;


-- Insert sample data for the past seven days with distinct values
INSERT INTO production_data (name, qualified_count, defective_count, qualified_rate, total_count, date)
SELECT name,
       FLOOR(RAND() * 30) + 80      AS qualified_count,
       FLOOR(RAND() * 10)           AS defective_count,
       ROUND(RAND() * 0.1 + 0.9, 2) AS qualified_rate,
       FLOOR(RAND() * 30) + 100     AS total_count,
       CURDATE() - INTERVAL i DAY   AS date
FROM (SELECT '胶纸切割' AS name
      UNION ALL
      SELECT '板框焊接'
      UNION ALL
      SELECT '板组件A'
      UNION ALL
      SELECT '板组件B'
      UNION ALL
      SELECT '框膜组件检测'
      UNION ALL
      SELECT '膜框组件A'
      UNION ALL
      SELECT '膜框组件B'
      UNION ALL
      SELECT '三合一电池A'
      UNION ALL
      SELECT '三合一电池B'
      UNION ALL
      SELECT '三合一电池C'
      UNION ALL
      SELECT '三合一电池检测'
      UNION ALL
      SELECT '总装线') AS products
         CROSS JOIN (SELECT 0 AS i
                     UNION ALL
                     SELECT 1
                     UNION ALL
                     SELECT 2
                     UNION ALL
                     SELECT 3
                     UNION ALL
                     SELECT 4
                     UNION ALL
                     SELECT 5
                     UNION ALL
                     SELECT 6) AS days;
