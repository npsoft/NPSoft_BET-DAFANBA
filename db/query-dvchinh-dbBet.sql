-- #
DELETE FROM AGIN;
DELETE FROM AGIN_TRACK;

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

-- #: 1.843 record(s)
-- DELETE FROM AGIN_SUMMARY;
-- SELECT ASUM.* FROM AGIN_SUMMARY ASUM ORDER BY ASUM.Id ASC;

DELETE FROM AGIN_RESULT

SELECT
    (SELECT MAX(AR.Times) FROM AGIN_RESULT AR WHERE AR.Type = 'pattern-01') [pattern01-max], -- 15
    (SELECT MAX(AR.Times) FROM AGIN_RESULT AR WHERE AR.Type = 'pattern-02') [pattern02-max]; -- 07

SELECT AR.Type, AR.Times, Count(1) Frequency
FROM AGIN_RESULT AR
GROUP BY AR.Times, AR.Type
ORDER BY AR.Type ASC, AR.Times DESC

SELECT AR.Times, AR.LatestOrder, ASUM.CoordinateX, ASUM.CoordinateY, ASUM.FileNames, ASUM.DataAnalysis
FROM AGIN_RESULT AR
    INNER JOIN AGIN_SUMMARY ASUM ON ASUM.Id = AR.SubId
WHERE ASUM.Db = 'aux_170405'
    AND AR.Type = 'pattern-02'
ORDER BY AR.Times DESC, AR.Id ASC
LIMIT 0, 5

-- #: 15314
DELETE FROM aux_1703ex.AGIN_RESULT2
SELECT COUNT(1) FROM aux_1703ex.AGIN_RESULT2 AR2

SELECT MIN(AR2.NumCircleRed / AR2.NumCircleBlue), MIN(AR2.NumCircleRed / AR2.NumCircleBlue)
FROM aux_1703ex.AGIN_RESULT2 AR2
WHERE AR2.NumCircleRed > 0 AND AR2.NumCircleBlue > 0
-- ORDER BY AR2.Id ASC
LIMIT 0, 100


CREATE TEMPORARY TABLE ParentChild AS
WITH FT_CTE AS (
    SELECT AR2.* FROM aux_1703ex.AGIN_RESULT2 AR2
)
SELECT * FROM FT_CTE;
SELECT * FROM ParentChild;

-- 25 <= PNumCircleRB <= 300, 33 <= PNumCircleBR <= 400: All
-- 43 <= PNumCircleRB <= 214, 46 <= PNumCircleBR <= 227: latest-order >= 10
-- 43 <= PNumCircleRB <= 214, 46 <= PNumCircleBR <= 227: num-circle-red >= 10 & num-circle-blue >= 10
SELECT SubId
    , MAX(LatestOrder) LatestOrder
    , MAX(NumCircleRed) NumCircleRed
    , MAX(NumCircleBlue) NumCircleBlue
    , MAX(NumCircleRed) * 100 / MAX(NumCircleBlue) PNumCircleRB
    , MAX(NumCircleBlue) * 100 / MAX(NumCircleRed) PNumCircleBR
FROM ParentChild
WHERE NumCircleRed >= 10 AND NumCircleBlue >= 10
GROUP BY SubId
ORDER BY MAX(NumCircleBlue) * 100 / MAX(NumCircleRed) ASC

SELECT SubId, LatestOrder, NumCircleRed, NumCircleBlue
FROM ParentChild
WHERE NumCircleRed >= 10 AND NumCircleBlue >= 10
    AND (NumCircleRed * 100 / NumCircleBlue < 43
        OR NumCircleRed * 100 / NumCircleBlue > 214
        OR NumCircleBlue * 100 / NumCircleRed < 46
        OR NumCircleBlue * 100 / NumCircleRed > 227)
