-- #
vacuum;
DELETE FROM AGIN;
DELETE FROM AGIN_TRACK;
DELETE FROM AGIN_SUMMARY;
DELETE FROM AGIN_RESULT1;
DELETE FROM AGIN_RESULT2;
SELECT COUNT(1) FROM AGIN;
SELECT COUNT(1) FROM AGIN_TRACK;
SELECT COUNT(1) FROM AGIN_SUMMARY; -- 9.079 record(s), Id = 8281 > 9127
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
    AND (
        AR.FreqL = 1 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 20 OR
        AR.FreqL = 2 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 17 OR
        AR.FreqL = 3 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 23 OR
        AR.FreqL = 4 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 20 OR
        AR.FreqL = 5 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 21 OR
        AR.FreqL = 6 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 23 OR
        AR.FreqL = 7 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 24 OR
        AR.FreqL = 8 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 25 OR
        AR.FreqL = 9 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 28 OR
        AR.FreqL = 10 AND (AR.FreqN * AR.FreqL + AR.FreqLSub) >= 30 OR
        AR.FreqL >= 11)
GROUP BY AR.FreqL
ORDER BY AR.FreqL ASC;
