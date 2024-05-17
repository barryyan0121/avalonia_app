-- Insert sample data into daily_target for the past ten days with a predefined target amount
INSERT INTO daily_target (name, target_amount, date)
SELECT name,
       FLOOR(RAND() * 30) + 100            AS target_amount, -- Random target amount between 10000 and 13000
       CURDATE() - INTERVAL day_offset DAY AS date
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
         CROSS JOIN (SELECT 0 AS day_offset
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
                     SELECT 6
                     UNION ALL
                     SELECT 7
                     UNION ALL
                     SELECT 8
                     UNION ALL
                     SELECT 9) AS days;

-- Insert sample detailed data for the past ten days with multiple entries per day for each name
DELIMITER //

CREATE PROCEDURE InsertProductionDetails(IN num_repeats INT)
BEGIN
    DECLARE counter INT DEFAULT 0;

    WHILE counter < num_repeats
        DO
            -- Insert data into production_details
            INSERT INTO production_details (name, operator_id, machine_id, is_qualified, production_time)
            SELECT name,
                   FLOOR(RAND() * 10) + 1                                                 AS operator_id,    -- Random operator ID
                   FLOOR(RAND() * 20) + 1                                                 AS machine_id,     -- Random machine ID
                   RAND() > 0.2                                                           AS is_qualified,   -- 80% chance of being qualified
                   CURDATE() - INTERVAL day_offset DAY + INTERVAL FLOOR(RAND() * 24) HOUR AS production_time -- Random production time
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
                     CROSS JOIN
                 (SELECT 0 AS day_offset
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
                  SELECT 6
                  UNION ALL
                  SELECT 7
                  UNION ALL
                  SELECT 8
                  UNION ALL
                  SELECT 9) AS days
                     CROSS JOIN
                     (SELECT 1 AS repeat_offset) AS repeats -- This set will be further filtered
            WHERE RAND() < 0.5;
            -- Randomly filter out about half of the entries for more varied counts

            -- Increment counter
            SET counter = counter + 1;
        END WHILE;
END //

DELIMITER ;

CALL InsertProductionDetails(100);
