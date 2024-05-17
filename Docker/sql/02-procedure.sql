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


DELIMITER //

CREATE TRIGGER after_insert_production_details
    AFTER INSERT
    ON production_details
    FOR EACH ROW
BEGIN
    CALL UpdateProductionData();
END //

CREATE TRIGGER after_update_production_details
    AFTER UPDATE
    ON production_details
    FOR EACH ROW
BEGIN
    CALL UpdateProductionData();
END //

CREATE TRIGGER after_delete_production_details
    AFTER DELETE
    ON production_details
    FOR EACH ROW
BEGIN
    CALL UpdateProductionData();
END //

DELIMITER ;