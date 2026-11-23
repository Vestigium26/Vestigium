SELECT
    U.Email,
    R.Name
FROM AspNetUsers U
INNER JOIN AspNetUserRoles UR
    ON U.Id = UR.UserId
INNER JOIN AspNetRoles R
    ON UR.RoleId = R.Id
WHERE U.Email = 'admin@inventory.com'