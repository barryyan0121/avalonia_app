DELIMITER //

CREATE PROCEDURE UpdateProductionData()
BEGIN
    -- Use a temporary table to hold the new aggregated data from production_details
    CREATE TEMPORARY TABLE temp_production_data AS
    SELECT name,
           SUM(IF(is_qualified, 1, 0)) AS qualified_count,
           SUM(IF(is_qualified, 0, 1)) AS defective_count,
           DATE(production_time)       AS date
    FROM production_details
    GROUP BY name,
             DATE(production_time);

-- Update the production_data table with the new aggregated data and keep the existing target_amount
    UPDATE production_data pd
        JOIN temp_production_data tpd ON pd.name = tpd.name AND pd.date = tpd.date
    SET pd.qualified_count = tpd.qualified_count,
        pd.defective_count = tpd.defective_count;

-- Insert new records into production_data for data that does not exist
    INSERT INTO production_data (name, qualified_count, defective_count, target_amount, date)
    SELECT tpd.name, tpd.qualified_count, tpd.defective_count, dt.target_amount, tpd.date
    FROM temp_production_data tpd
             LEFT JOIN production_data pd ON pd.name = tpd.name AND pd.date = tpd.date
             JOIN daily_target dt ON tpd.name = dt.name AND tpd.date = dt.date
    WHERE pd.name IS NULL;

-- Drop the temporary table
    DROP TEMPORARY TABLE temp_production_data;
END //

DELIMITER ;

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

DELIMITER //

CREATE PROCEDURE InsertDailyTargetWithDayOffset(IN day_offset INT, IN num_days INT)
BEGIN
    DECLARE counter INT DEFAULT 0;

    WHILE counter < num_days DO
            INSERT INTO daily_target (name, target_amount, date)
            SELECT name,
                   FLOOR(RAND() * 300) + 1000 AS target_amount, -- Random target amount between 1000 and 1300
                   CURDATE() - INTERVAL (day_offset + counter) DAY AS date
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
                  SELECT '总装线') AS products;

            SET counter = counter + 1;
        END WHILE;
END //

DELIMITER ;

DELIMITER //

CREATE PROCEDURE InsertProductionDetailsWithCurdate(IN num_repeats INT)
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
                   CURDATE() + INTERVAL FLOOR(RAND() * 24) HOUR                           AS production_time -- Random hour within the current date
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
                 (SELECT 1 AS repeat_offset) AS repeats; -- Ensure to repeat the required number of times

            -- Increment counter
            SET counter = counter + 1;
        END WHILE;
END //

DELIMITER ;