-- COUNT(1) = 1.265 record(s)
-- DELETE FROM AGIN WHERE FileName IN('agin-170318-213442-362.png', 'agin-170318-213502-615.png', 'agin-170318-213523-509.png', 'agin-170318-213540-391.png', 'agin-170318-213557-130.png')
-- SELECT FileName FROM AGIN GROUP BY fileName ORDER BY Id DESC LIMIT 0, 5
-- SELECT * FROM AGIN WHERE FileName IN('agin-170318-225752-539.png', 'agin-170318-225730-283.png', 'agin-170318-225708-005.png', 'agin-170318-225645-963.png', 'agin-170318-225624-229.png') ORDER BY Id ASC
/* -: SELECT FileName, COUNT(1)
FROM AGIN
WHERE AnalysisData LIKE '{"valid":false,%'
GROUP BY FileName
HAVING COUNT(1) = 5
ORDER BY FileName ASC*/
-- SELECT * FROM AGIN WHERE FileName IN ('agin-170318-213828-468.png', 'agin-170318-213845-092.png', 'agin-170318-213901-528.png', 'agin-170318-213917-760.png', 'agin-170318-213934-249.png', 'agin-170318-213950-880.png', -'agin-170318-214007-491.png') ORDER BY Id ASC
-- DELETE FROM AGIN WHERE FileName IN ('agin-170318-213828-468.png', 'agin-170318-213845-092.png', 'agin-170318-213901-528.png', 'agin-170318-213917-760.png', 'agin-170318-213934-249.png', 'agin-170318-213950-880.png', 'agin-170318-214007-491.png')
-- SELECT * FROM AGIN_ITEMS
-- DELETE FROM AGIN_ITEMS