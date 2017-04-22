-- #
vacuum;
DELETE FROM AGIN;
DELETE FROM AGIN_TRACK;
DELETE FROM AGIN_SUMMARY;
DELETE FROM AGIN_RESULT1;
DELETE FROM AGIN_RESULT2;
SELECT COUNT(1) FROM AGIN;
SELECT COUNT(1) FROM AGIN_TRACK;
SELECT COUNT(1) FROM AGIN_SUMMARY; -- 2.754 record(s)
SELECT COUNT(1) FROM AGIN_RESULT1; -- 22.568 record(s)
SELECT COUNT(1) FROM AGIN_RESULT2; -- 174.564 record(s) > 1.922 match(es) > 384.400.000; max-count = 41

-- #
ATTACH DATABASE 'D:\NPSoft_BET-DAFANBA\db\dbBet.db3' AS aux;
-- SELECT * FROM aux.AGIN;
-- SELECT * FROM aux.AGIN_TRACK;
-- DELETE FROM aux.AGIN;
-- DELETE FROM aux.AGIN_TRACK;
BEGIN TRANSACTION;
INSERT INTO AGIN_SUMMARY (CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy)
SELECT CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy
FROM aux.AGIN
WHERE Id NOT IN (SELECT MAX(Id) FROM aux.AGIN GROUP BY CoordinateX, CoordinateY);
DELETE FROM aux.AGIN WHERE Id NOT IN ((SELECT MAX(Id) FROM aux.AGIN GROUP BY CoordinateX, CoordinateY);
END TRANSACTION;
DETACH DATABASE aux;

-- #
SELECT
    (SELECT MAX(AR.Times) FROM AGIN_RESULT1 AR WHERE AR.Type = 'pattern-01') [pattern01-max], -- 16
    (SELECT MAX(AR.Times) FROM AGIN_RESULT1 AR WHERE AR.Type = 'pattern-02') [pattern02-max]; -- 07

SELECT AR.Type, AR.Times, Count(1) Frequency
FROM AGIN_RESULT1 AR
GROUP BY AR.Times, AR.Type
ORDER BY AR.Type ASC, AR.Times DESC;

-- #
CREATE TEMPORARY TABLE tmpARGroup AS
WITH FT_CTE AS (
    SELECT AR.SubId
        , MAX(AR.LatestOrder) LatestOrder
        , MAX(AR.NumCircleRed) NumCircleRed
        , MAX(AR.NumCircleBlue) NumCircleBlue
        , CAST(MAX(AR.NumCircleRed) AS DOUBLE) / MAX(AR.NumCircleBlue) PNumCircleRB
        , CAST(MAX(AR.NumCircleBlue) AS DOUBLE) / MAX(AR.NumCircleRed) PNumCircleBR
    FROM AGIN_RESULT2 AR
    GROUP BY AR.SubId)
SELECT * FROM FT_CTE;

-- num-cricle-red > 04 & num-circle-blue > 04 & latest-order > 00: 0.4090, 2.8888, 0.3461, 2.4444 ~ 9/22, 26/9, 9/26, 22/9
-- num-circle-red > 00 & num-circle-blue > 00 & latest-order > 09: 0.3333, 2.8888, 0.3461, 3.0000 ~ 1/3, 26/9, 9/26, 3/1
-- num-circle-red > 09 & num-circle-blue > 09 & latest-order > 00: 0.4390, 2.2500, 0.4444, 2.2777
--                                                               : 0.4300, 2.1400, 0.4600, 2.2700
SELECT
      MIN(ARG.PNumCircleRB)
    , MAX(ARG.PNumCircleRB)
    , MIN(ARG.PNumCircleBR)
    , MAX(ARG.PNumCircleBR)
FROM tmpARGroup ARG
WHERE ARG.NumCircleRed > 5
    AND ARG.NumCircleBlue > 5;

SELECT ARG.*
FROM tmpARGroup ARG
WHERE ARG.LatestOrder > 9
    AND ARG.NumCircleRed > 0
    AND ARG.NumCircleBlue > 0
    AND substr(ARG.PNumCircleBR, 0, 7) = '2.4444';

-- BEGIN;
PRAGMA temp_store = 2;
BEGIN TRANSACTION;
CREATE TEMP TABLE IF NOT EXISTS _Variables (Name TEXT PRIMARY KEY NOT NULL, Value TEXT);
INSERT OR REPLACE INTO _Variables VALUES ('min-p-num-circle-rb', CAST(9 AS DOUBLE) / 22);
INSERT OR REPLACE INTO _Variables VALUES ('max-p-num-circle-rb', CAST(26 AS DOUBLE) / 9);
INSERT OR REPLACE INTO _Variables VALUES ('min-p-num-circle-br', CAST(9 AS DOUBLE) / 26);
INSERT OR REPLACE INTO _Variables VALUES ('max-p-num-circle-br', CAST(22 AS DOUBLE) / 9);
INSERT OR REPLACE INTO _Variables VALUES ('num-match', CAST(0 AS INT));
-- SELECT * FROM _Variables;

CREATE TEMPORARY TABLE tmpAR1 AS
WITH FT_CTE AS (
    SELECT AR.Id, AR.SubId, AR.LatestOrder, AR.NumCircleRed, AR.NumCircleBlue
        , CAST(AR.NumCircleRed AS DOUBLE) / AR.NumCircleBlue PNumCircleRB
        , CAST(AR.NumCircleBlue AS DOUBLE) / AR.NumCircleRed PNumCircleBR
        , CAST(0 AS INT) [Match]
    FROM AGIN_RESULT2 AR
    WHERE AR.LatestOrder > 5
        AND AR.NumCircleRed > 0
        AND AR.NumCircleBlue > 0
        AND (
               (CAST(AR.NumCircleRed AS DOUBLE) / AR.NumCircleBlue) < (SELECT CAST(COALESCE(Value, NULL) AS DOUBLE) FROM _Variables WHERE Name = 'min-p-num-circle-rb' LIMIT 1)
            OR (CAST(AR.NumCircleRed AS DOUBLE) / AR.NumCircleBlue) > (SELECT CAST(COALESCE(Value, NULL) AS DOUBLE) FROM _Variables WHERE Name = 'max-p-num-circle-rb' LIMIT 1)
            OR (CAST(AR.NumCircleBlue AS DOUBLE) / AR.NumCircleRed) < (SELECT CAST(COALESCE(Value, NULL) AS DOUBLE) FROM _Variables WHERE Name = 'min-p-num-circle-br' LIMIT 1)
            OR (CAST(AR.NumCircleBlue AS DOUBLE) / AR.NumCircleRed) > (SELECT CAST(COALESCE(Value, NULL) AS DOUBLE) FROM _Variables WHERE Name = 'max-p-num-circle-br' LIMIT 1)))
SELECT * FROM FT_CTE;
-- SELECT * FROM tmpAR1 ORDER BY SubId ASC, LatestOrder ASC;

DROP TABLE IF EXISTS tmpAR2;
CREATE TEMP TABLE tmpAR2 (
    Id INTEGER PRIMARY KEY NOT NULL,
    SubId INTEGER NULL,
    LatestOrder INT NULL,
    NumCircleRed INT NULL,
    NumCircleBlue INT NULL,
    PNumCircleRB DOUBLE NULL,
    PNumCircleBR DOUBLE NULL
);
PRAGMA recursive_triggers = on;
DROP TRIGGER IF EXISTS ttring;
CREATE TEMP trigger ttring
BEFORE INSERT ON tmpAR2
WHEN 1 = 1 BEGIN
    UPDATE tmpAR1 SET [Match] = CASE
        WHEN EXISTS (SELECT 1 FROM tmpAR1 WHERE SubId = new.SubId AND LatestOrder = new.LatestOrder - 1 LIMIT 1) THEN
            (SELECT [Match] FROM tmpAR1 WHERE SubId = new.SubId AND LatestOrder = new.LatestOrder - 1 LIMIT 1)
        ELSE
            (SELECT COALESCE(Value, NULL) FROM _Variables WHERE Name = 'num-match' LIMIT 1) + 1
        END
     WHERE Id = new.Id;
     INSERT OR REPLACE INTO _Variables VALUES('num-match', (SELECT MAX([Match]) FROM tmpAR1));
END;
INSERT INTO tmpAR2(Id, SubId, LatestOrder, NumCircleRed, NumCircleBlue, PNumCircleRB, PNumCircleBR)
SELECT Id, SubId, LatestOrder, NumCircleRed, NumCircleBlue, PNumCircleRB, PNumCircleBR FROM tmpAR1 ORDER BY SubId ASC, LatestOrder ASC;
-- SELECT * FROM tmpAR2;

DROP TABLE _Variables;
DROP TABLE tmpAR1;
DROP TABLE tmpAR2;
END TRANSACTION;
-- END;

SELECT AR.SubId, AR.[Match], COUNT(1), MIN(LatestOrder), MAX(Latestorder)
FROM tmpAR1 AR
GROUP BY AR.SubId, AR.[Match]
ORDER BY COUNT(1) DESC, AR.[Match] ASC, AR.SubId ASC;
