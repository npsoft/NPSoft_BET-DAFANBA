-- #
vacuum;
DELETE FROM AGIN;
DELETE FROM AGIN_TRACK;
DELETE FROM AGIN_SUMMARY;
DELETE FROM AGIN_RESULT1;
DELETE FROM AGIN_RESULT2;
SELECT COUNT(1) FROM AGIN;
SELECT COUNT(1) FROM AGIN_TRACK;
SELECT COUNT(1) FROM AGIN_SUMMARY; -- 8.233 record(s)
SELECT COUNT(1) FROM AGIN_RESULT1; -- 1.437.672 record(s)
SELECT COUNT(1) FROM AGIN_RESULT2; -- 527.725 record(s)

-- #
-- SELECT * FROM aux.AGIN;
-- SELECT * FROM aux.AGIN_TRACK;
-- DELETE FROM aux.AGIN;
-- DELETE FROM aux.AGIN_TRACK;
ATTACH DATABASE 'D:\NPSoft_BET-DAFANBA\db\dbBet.db3' AS aux;
BEGIN TRANSACTION;
INSERT INTO AGIN_SUMMARY (CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy)
SELECT CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy
FROM aux.AGIN
WHERE Id NOT IN (SELECT MAX(Id) FROM aux.AGIN GROUP BY CoordinateX, CoordinateY);
DELETE FROM aux.AGIN WHERE Id NOT IN (SELECT MAX(Id) FROM aux.AGIN GROUP BY CoordinateX, CoordinateY);
END TRANSACTION;
DETACH DATABASE aux;

-- #
SELECT AR.FreqL, MAX(AR.FreqN * AR.FreqL + AR.FreqLSub) MaxL, COUNT(1) Times
FROM AGIN_RESULT1 AR
WHERE AR.FreqN >= 3
    /* v1:*/
    AND (
        AR.FreqL = 1 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 13 OR
        AR.FreqL = 2 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 12 OR
        AR.FreqL = 3 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 20 OR
        AR.FreqL = 4 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 17 OR
        AR.FreqL = 5 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 18 OR
        AR.FreqL = 6 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 20 OR
        AR.FreqL = 7 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 21 OR
        AR.FreqL = 8 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 24 OR
        AR.FreqL = 9 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 27 OR
        AR.FreqL = 10 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 30 OR
        AR.FreqL >= 11)
    /* v2:
    AND (
        AR.FreqL = 1 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 9 OR
        AR.FreqL = 2 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 8 OR
        AR.FreqL = 3 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 16 OR
        AR.FreqL = 4 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 13 OR
        AR.FreqL = 5 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 15 OR
        AR.FreqL = 6 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 18 OR
        AR.FreqL = 7 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 21 OR
        AR.FreqL = 8 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 24 OR
        AR.FreqL = 9 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 27 OR
        AR.FreqL = 10 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 30 OR
        AR.FreqL >= 11)*/
GROUP BY AR.FreqL
ORDER BY AR.FreqL ASC;

SELECT AR.FreqL, COUNT(1) Times
FROM AGIN_RESULT1 AR
    INNER JOIN (
        SELECT FreqL, MAX(FreqN * FreqL + FreqLSub) MaxL
        FROM AGIN_RESULT1
        GROUP BY FreqL) T ON T.FreqL = AR.FreqL AND T.MaxL = AR.FreqN * AR.FreqL + AR.FreqLSub
GROUP BY AR.FreqL
ORDER BY AR.FreqL ASC;

SELECT AR.FreqL, AR.FreqColors, AR.FreqN * AR.FreqL + AR.FreqLSub TotalL
    , ASUM.CreatedOn, ASUM.LastModifiedOn, substr(ASUM.CreatedOn, 11, 3) CreatedHH, substr(ASUM.LastModifiedOn, 11, 3) LastModifiedHH
FROM AGIN_RESULT1 AR
    INNER JOIN AGIN_SUMMARY ASUM ON ASUM.Id = AR.SubId
    INNER JOIN (
        SELECT FreqL, MAX(FreqN * FreqL + FreqLSub) MaxL
        FROM AGIN_RESULT1
        GROUP BY FreqL) T ON T.FreqL = AR.FreqL AND T.MaxL = AR.FreqN * AR.FreqL + AR.FreqLSub
ORDER BY AR.FreqL ASC, ASUM.CreatedOn ASC;

-- FreqL = 1, MaxL = 16: [00], 01, 02, 03, 04, 05, 06, 07, [08], [09], [10], [11], 12, 13, [14], [15], 16, 17, 18, 19, 20, 21, 22, [23]
-- FreqL = 1, MaxL = 15: [00], 01, 02, 03, 04, 05, [06], [07], [08], [09], [10], [11], [12], 13, [14], [15], [16], 17, 18, 19, [20], [21], [22], [23]
-- FreqL = 1, MaxL = 14: [00], 01, 02, [03], [04], [05], [06], [07], [08], [09], [10], [11], [12], 13, [14], [15], [16], 17, [18], 19, [20], [21], [22], [23]
SELECT AR.FreqL, AR.FreqColors, AR.FreqN * AR.FreqL + AR.FreqLSub TotalL
    , ASUM.CreatedOn, ASUM.LastModifiedOn, substr(ASUM.CreatedOn, 11, 3) CreatedHH, substr(ASUM.LastModifiedOn, 11, 3) LastModifiedHH
FROM AGIN_RESULT1 AR
    INNER JOIN AGIN_SUMMARY ASUM ON ASUM.Id = AR.SubId
    INNER JOIN (
        SELECT 1 FreqL,  13 MaxL) T ON T.FreqL = AR.FreqL AND T.MaxL = AR.FreqN * AR.FreqL + AR.FreqLSub
ORDER BY AR.FreqL ASC, ASUM.CreatedOn ASC, ASUM.LastModifiedOn ASC;

SELECT AR.*, substr(ASUM.CreatedOn, 12, 2)
FROM AGIN_RESULT1 AR
    INNER JOIN AGIN_SUMMARY ASUM ON ASUM.Id = AR.SubId
WHERE AR.FreqL = 1
    AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 9
    AND substr(ASUM.CreatedOn, 12, 2) IN ('01', '02', '13', '17', '19')
    AND substr(ASUM.LastModifiedOn, 12, 2) IN ('01', '02', '13', '17', '19');
-- #
PRAGMA foreign_keys=off;
PRAGMA temp_store=2;
BEGIN TRANSACTION;

CREATE TEMP TABLE IF NOT EXISTS _Variables (Name TEXT PRIMARY KEY NOT NULL, Value TEXT);

DROP TABLE IF EXISTS tmpMaxOrder;
CREATE TEMPORARY TABLE tmpMaxOrder AS
WITH FT_CTE AS (
    SELECT SubId, MAX(LatestOrder) MaxOrder FROM AGIN_RESULT2 GROUP BY SubId)
SELECT * FROM FT_CTE;

DROP TABLE IF EXISTS tmpDistAVG;
CREATE TEMPORARY TABLE tmpDistAVG AS
WITH FT_CTE AS (
    SELECT ASUM.Id
        , MO.MaxOrder
        , CAST(60 AS INT) DistAVG
        , ASUM.LastModifiedOn LastModified
        , strftime('%s', ASUM.LastModifiedOn) LastModifiedUnix
    FROM AGIN_SUMMARY ASUM
        INNER JOIN tmpMaxOrder MO ON MO.SubId = ASUM.Id
    WHERE ASUM.Id > 7713)
SELECT * FROM FT_CTE;

DROP TABLE IF EXISTS tmpAR1;
CREATE TEMPORARY TABLE tmpAR1 AS
WITH FT_CTE AS (
    SELECT AR.Id, AR.SubId, AR.LatestOrder
        , AR.FreqN, AR.FreqL, AR.FreqLSub, AR.FreqColors
        , AR.FreqN * AR.FreqL + AR.FreqLSub FreqLTotal
        , ((DA.LastModifiedUnix - (DA.MaxOrder - AR.LatestOrder) * DA.DistAVG) / 1800) * 1800 LastModifiedUnix
    FROM AGIN_RESULT1 AR
        INNER JOIN tmpDistAVG DA ON DA.Id = AR.SubId)
SELECT * FROM FT_CTE;

INSERT OR REPLACE INTO _Variables VALUES ('min-latest-modified-unix', (SELECT MIN(LastModifiedUnix) FROM tmpAR1));
INSERT OR REPLACE INTO _Variables VALUES ('max-latest-modified-unix', (SELECT MAX(LastModifiedUnix) FROM tmpAR1));
UPDATE _Variables SET Value =
    ((SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'min-latest-modified-unix' LIMIT 1) / 1800) * 1800
WHERE Name = 'min-latest-modified-unix';
UPDATE _Variables SET Value = (CASE
    WHEN CAST((SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'max-latest-modified-unix' LIMIT 1) AS DOUBLE) / 1800 <> 0 THEN
        ((SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'max-latest-modified-unix' LIMIT 1) / 1800 + 1) * 1800
    ELSE
        (SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'max-latest-modified-unix' LIMIT 1)
    END)
WHERE Name = 'max-latest-modified-unix';
DROP TABLE IF EXISTS tmpTime;
CREATE TEMPORARY TABLE tmpTime AS
WITH FT_CTE AS (
    WITH RECURSIVE recursiveTime (time)
    AS (
        SELECT (SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'min-latest-modified-unix' LIMIT 1)
        UNION ALL
        SELECT time + 1800 FROM recursiveTime WHERE time < (SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'max-latest-modified-unix' LIMIT 1))
    SELECT datetime(time, 'unixepoch') LastModified, time LastModifiedUnix FROM recursiveTime)
SELECT * FROM FT_CTE;

DROP TABLE IF EXISTS tmpAR1Cus;
CREATE TEMPORARY TABLE tmpAR1Cus AS
WITH FT_CTE AS (
    SELECT AR.FreqL, AR.FreqLTotal, T.LastModified, COUNT(1) Times
    FROM tmpAR1 AR
        INNER JOIN tmpTime T ON AR.LastModifiedUnix = T.LastModifiedUnix
    GROUP BY AR.FreqL, AR.FreqLTotal, T.LastModified)
SELECT * FROM FT_CTE;

COMMIT;
PRAGMA foreign_keys=on;
/* -:
SELECT T.*
FROM (SELECT AR.FreqL, AR.FreqLTotal, strftime('%H:%M', T.LastModified) LastModified, COUNT(1) Times
    FROM tmpAR1 AR
        INNER JOIN tmpTime T ON AR.LastModifiedUnix = T.LastModifiedUnix
    GROUP BY AR.FreqL, AR.FreqLTotal, strftime('%H:%M', T.LastModified)) T
WHERE T.FreqL = 1 AND T.FreqLTotal = 18
ORDER BY T.LastModified ASC;*/
/* -: AVG = 54, MAX = 130, MIN = 45
SELECT AVG(DistSeconds) DistSecondsAVG, MAX(DistSeconds) DistSecondsMAX, MIN(DistSeconds) DistSecondsMIN
    FROM (
    SELECT ASUM.Id
        , strftime('%s', ASUM.CreatedOn) CreatedOn
        , strftime('%s', ASUM.LastModifiedOn) LastModifiedOn
        , CAST(((strftime('%s', ASUM.LastModifiedOn) - strftime('%s', ASUM.CreatedOn)) / CAST(MO.MaxOrder AS DOUBLE) + 0.5) AS INT) DistSeconds
    FROM AGIN_SUMMARY ASUM
        INNER JOIN tmpMaxOrder MO ON MO.SubId = ASUM.Id)
WHERE DistSeconds BETWEEN 45 AND 135;*/