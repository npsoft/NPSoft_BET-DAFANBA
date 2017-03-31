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
