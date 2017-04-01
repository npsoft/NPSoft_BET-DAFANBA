-- #
DELETE FROM AGIN
DELETE FROM AGIN_TRACK

-- #
SELECT A.* FROM AGIN A ORDER BY A.Id ASC
SELECT AT.* FROM AGIN_TRACK AT ORDER BY AT.Id ASC

-- #
SELECT AT.*
FROM AGIN_TRACK AT
WHERE AT.DataAnalysis NOT LIKE '{"total-col":%,"total-row":%,"total-invalid":0,%'
    AND AT.DataAnalysis NOT LIKE '{"total-col":%,"total-row":%,"total-invalid":204,%'
ORDER BY AT.Id ASC
LIMIT 0, 10

-- #
SELECT AT.*
FROM AGIN_TRACK AT
WHERE AT.FileNames IN (';agin-170325-200450-790.png;', ';agin-170325-200513-865.png;', ';agin-170325-200534-999.png;', ';agin-170325-200555-020.png;', ';agin-170325-200615-563.png;', ';agin-170325-200636-733.png;')
ORDER BY AT.Id ASC

DELETE FROM AGIN_TRACK WHERE FileNames IN (';agin-170325-200450-790.png;', ';agin-170325-200513-865.png;', ';agin-170325-200534-999.png;', ';agin-170325-200555-020.png;', ';agin-170325-200615-563.png;', ';agin-170325-200636-733.png;')

-- #
ATTACH DATABASE 'I:\NPSoft_BET-DAFANBA\db\dbBet_17032x.db3' AS aux_17032x;
ATTACH DATABASE 'I:\NPSoft_BET-DAFANBA\db\dbBet_170328.db3' AS aux_170328;
ATTACH DATABASE 'I:\NPSoft_BET-DAFANBA\db\dbBet_170330.db3' AS aux_170330;
ATTACH DATABASE 'I:\NPSoft_BET-DAFANBA\db\dbBet_1703ex.db3' AS aux_1703ex;
ATTACH DATABASE 'I:\NPSoft_BET-DAFANBA\db\dbBet_170328_trungdt.db3' AS aux_170328_trungdt;
ATTACH DATABASE 'I:\NPSoft_BET-DAFANBA\db\dbBet_170329_trungdt.db3' AS aux_170329_trungdt;
DETACH DATABASE aux_17032x;
DETACH DATABASE aux_170328;
DETACH DATABASE aux_170330;
DETACH DATABASE aux_170328_trungdt;
DETACH DATABASE aux_170329_trungdt;
SELECT COUNT(1) FROM aux_17032x.AGIN;
SELECT COUNT(1) FROM aux_170328.AGIN;
SELECT COUNT(1) FROM aux_170330.AGIN;
SELECT COUNT(1) FROM aux_170328_trungdt.AGIN;
SELECT COUNT(1) FROM aux_170329_trungdt.AGIN;
DELETE FROM AGIN_SUMMARY;
INSERT INTO AGIN_SUMMARY (Db, SubId, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy)
SELECT 'aux_17032x', Id, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy FROM aux_17032x.AGIN;
INSERT INTO AGIN_SUMMARY (Db, SubId, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy)
SELECT 'aux_170328', Id, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy FROM aux_170328.AGIN;
INSERT INTO AGIN_SUMMARY (Db, SubId, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy)
SELECT 'aux_170330', Id, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy FROM aux_170330.AGIN;
INSERT INTO AGIN_SUMMARY (Db, SubId, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy)
SELECT 'aux_170328_trungdt', Id, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy FROM aux_170328_trungdt.AGIN;
INSERT INTO AGIN_SUMMARY (Db, SubId, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy)
SELECT 'aux_170329_trungdt', Id, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy FROM aux_170329_trungdt.AGIN;
SELECT ASUM.* FROM AGIN_SUMMARY ASUM ORDER BY ASUM.Id ASC;

SELECT AR.Type, AR.Times, AR.LatestOrder, ASUM.SubId, ASUM.CoordinateX, ASUM.CoordinateY, ASUM.DataAnalysis
FROM AGIN_RESULT AR
    INNER JOIN AGIN_SUMMARY ASUM ON ASUM.Id = AR.SubId
WHERE AR.Type = 'pattern-02' AND ASUM.Db = 'aux_17032x'
ORDER BY AR.Id ASC;

SELECT
    (SELECT MAX(AR.Times) FROM AGIN_RESULT AR WHERE AR.Type = 'pattern-01') [pattern01-max], -- 15
    (SELECT MAX(AR.Times) FROM AGIN_RESULT AR WHERE AR.Type = 'pattern-02') [pattern02-max]; -- 06
