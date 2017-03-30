-- #
DELETE FROM AGIN
DELETE FROM AGIN_TRACK

-- #
-- reviewed total-invalid: dbBet_17032x.db3, dbBet_170329_trungdt.db3

-- #
SELECT A.* FROM AGIN A ORDER BY A.Id ASC
SELECT AT.* FROM AGIN_TRACK AT ORDER BY AT.Id ASC

-- #
SELECT AT.*
FROM AGIN_TRACK AT
WHERE AT.DataAnalysis NOT LIKE '{"total-col":%,"total-row":%,"total-invalid":0,%'
    AND AT.DataAnalysis NOT LIKE '{"total-col":%,"total-row":%,"total-invalid":204,%'
ORDER BY AT.Id ASC
-- LIMIT 0, 10

-- #
SELECT AT.*
FROM AGIN_TRACK AT
WHERE AT.FileNames IN (';agin-170325-200450-790.png;', ';agin-170325-200513-865.png;', ';agin-170325-200534-999.png;', ';agin-170325-200555-020.png;', ';agin-170325-200615-563.png;', ';agin-170325-200636-733.png;')
ORDER BY AT.Id ASC

DELETE FROM AGIN_TRACK WHERE FileNames IN (';agin-170325-200450-790.png;', ';agin-170325-200513-865.png;', ';agin-170325-200534-999.png;', ';agin-170325-200555-020.png;', ';agin-170325-200615-563.png;', ';agin-170325-200636-733.png;')

-- DELETE FROM AGIN WHERE FileName IN ('agin-170318-215633-059.png', 'agin-170318-215702-621.png', 'agin-170318-215719-358.png', 'agin-170318-215735-731.png', 'agin-170318-215754-723.png', 'agin-170318-215820-010.png', 'agin-170318-215836-532.png', 'agin-170318-215853-198.png', 'agin-170318-215922-232.png', 'agin-170318-215939-653.png', 'agin-170318-215956-323.png', 'agin-170318-220019-930.png', 'agin-170318-220038-924.png', 'agin-170318-220055-380.png', 'agin-170318-220112-110.png', 'agin-170318-220141-792.png', 'agin-170318-220158-709.png', 'agin-170318-220215-153.png', 'agin-170318-220237-210.png', 'agin-170318-220259-457.png')
-- SELECT * FROM AGIN_TRACK ORDER BY Id ASC

PRAGMA foreign_keys=off;
BEGIN TRANSACTION;
-- ALTER TABLE AGIN RENAME TO tmpAGIN;

INSERT INTO AGIN(Id, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy)
SELECT Id, CoordinateX, CoordinateY, FileName, AnalysisData, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy FROM tmpAGIN
BEGIN TRANSACTION;
COMMIT;
PRAGMA foreign_keys=on;
DROP TABLE AGIN
