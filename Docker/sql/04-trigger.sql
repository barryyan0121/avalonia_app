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

-- 创建在 daily_target 表上的触发器
CREATE TRIGGER after_insert_daily_target
    AFTER INSERT ON daily_target
    FOR EACH ROW
BEGIN
    CALL UpdateProductionData();
END //

CREATE TRIGGER after_update_daily_target
    AFTER UPDATE ON daily_target
    FOR EACH ROW
BEGIN
    CALL UpdateProductionData();
END //

CREATE TRIGGER after_delete_daily_target
    AFTER DELETE ON daily_target
    FOR EACH ROW
BEGIN
    CALL UpdateProductionData();
END //

DELIMITER ;