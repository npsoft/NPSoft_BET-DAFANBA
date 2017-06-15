-- #
vacuum;
DELETE FROM AGIN;
DELETE FROM AGIN_TRACK;
DELETE FROM AGIN_SUMMARY;
DELETE FROM AGIN_RESULT1;
DELETE FROM AGIN_RESULT2;
SELECT COUNT(1) FROM AGIN;
SELECT COUNT(1) FROM AGIN_TRACK;
SELECT COUNT(1) FROM AGIN_SUMMARY; -- 7.665 record(s)
SELECT COUNT(1) FROM AGIN_RESULT1; -- 0 record(s)
SELECT COUNT(1) FROM AGIN_RESULT2; -- 0 record(s)

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
    AND substr(ASUM.LastModifiedOn, 12, 2) IN ('01', '02', '13', '17', '19')