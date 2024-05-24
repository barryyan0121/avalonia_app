-- Insert sample data into daily_target for the past ten days with a predefined target amount
INSERT INTO daily_target (name, target_amount, date)
SELECT name,
       FLOOR(RAND() * 300) + 1000            AS target_amount, -- Random target amount between 10000 and 13000
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


CALL InsertProductionDetails(1000);
CALL UpdateProductionData();