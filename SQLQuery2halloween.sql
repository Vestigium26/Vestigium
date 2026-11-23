SELECT
    m.PartNumber,
    m.Quantity AS MaterialQuantity,
    SUM(ml.Quantity) AS LocationQuantity
FROM Materials m
INNER JOIN MaterialLocations ml
    ON ml.MaterialId = m.Id
GROUP BY
    m.PartNumber,
    m.Quantity
HAVING m.Quantity <> SUM(ml.Quantity);