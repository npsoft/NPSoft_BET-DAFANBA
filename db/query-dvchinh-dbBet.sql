-- #
-- DELETE FROM AGIN;
-- DELETE FROM AGIN_TRACK;
-- DELETE FROM AGIN_RESULT1;
-- DELETE FROM AGIN_RESULT2;

-- #
SELECT A.* FROM AGIN A ORDER BY A.Id ASC;
SELECT AT.* FROM AGIN_TRACK AT ORDER BY AT.Id ASC;
SELECT COUNT(1) FROM AGIN_SUMMARY ASUM; -- 1.843 record(s)
SELECT COUNT(1) FROM AGIN_RESULT1 AR; -- 11.664 record(s)
SELECT COUNT(1) FROM AGIN_RESULT2 AR; -- 116.148 record(s) > 1.843 group(s)

-- #
SELECT
    (SELECT MAX(AR.Times) FROM AGIN_RESULT1 AR WHERE AR.Type = 'pattern-01') [pattern01-max], -- 15
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

-- num-circle-red > 00 & num-circle-blue > 00 & latest-order > 05: 0.3333, 3.0000, 0.3333, 3.0000
-- num-circle-red > 00 & num-circle-blue > 00 & latest-order > 10: 0.3333, 2.8888, 0.3461, 3.0000
-- num-circle-red > 09 & num-circle-blue > 09 & latest-order > 00: 0.4390, 2.2500, 0.4444, 2.2777
--                                                               : 0.4300, 2.1400, 0.4600, 2.2700
SELECT
      MIN(ARG.PNumCircleRB)
    , MAX(ARG.PNumCircleRB)
    , MIN(ARG.PNumCircleBR)
    , MAX(ARG.PNumCircleBR)
FROM tmpARGroup ARG
WHERE ARG.LatestOrder > 5
    AND ARG.NumCircleRed > 0
    AND ARG.NumCircleBlue > 0;

-- BEGIN;
BEGIN TRANSACTION;
PRAGMA temp_store = 2;

CREATE TEMP TABLE IF NOT EXISTS _Variables (Name TEXT PRIMARY KEY NOT NULL, Value TEXT);
INSERT OR REPLACE INTO _Variables VALUES ('min-p-num-circle-rb', CAST(1 AS DOUBLE) / 3);
INSERT OR REPLACE INTO _Variables VALUES ('max-p-num-circle-rb', CAST(3 AS DOUBLE) / 1);
INSERT OR REPLACE INTO _Variables VALUES ('min-p-num-circle-br', CAST(1 AS DOUBLE) / 3);
INSERT OR REPLACE INTO _Variables VALUES ('max-p-num-circle-br', CAST(3 AS DOUBLE) / 1);
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
               CAST(AR.NumCircleRed AS DOUBLE) / AR.NumCircleBlue < (SELECT COALESCE(Value, NULL) FROM _Variables WHERE Name = 'min-p-num-circle-rb' LIMIT 1)
            OR CAST(AR.NumCircleRed AS DOUBLE) / AR.NumCircleBlue > (SELECT COALESCE(Value, NULL) FROM _Variables WHERE Name = 'max-p-num-circle-rb' LIMIT 1)
            OR CAST(AR.NumCircleBlue AS DOUBLE) / AR.NumCircleRed < (SELECT COALESCE(Value, NULL) FROM _Variables WHERE Name = 'min-p-num-circle-br' LIMIT 1)
            OR CAST(AR.NumCircleBlue AS DOUBLE) / AR.NumCircleRed > (SELECT COALESCE(Value, NULL) FROM _Variables WHERE Name = 'max-p-num-circle-br' LIMIT 1)))
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

-- 592 record(s)
SELECT AR.SubId, COUNT(1)
FROM tmpAR1 AR
GROUP BY AR.SubId
HAVING COUNT(1) > 5
ORDER BY COUNT(1) DESC;

-- 33 times
SELECT AR.* FROM tmpAR1 AR WHERE AR.SubId = 122 ORDER BY LatestOrder ASC;
