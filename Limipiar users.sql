DELETE FROM Movements;

DBCC CHECKIDENT ('Movements', RESEED, 0);

DELETE FROM AspNetUserRoles;

DELETE FROM AspNetUserClaims;

DELETE FROM AspNetUserLogins;

DELETE FROM AspNetUserTokens;

DELETE FROM AspNetUsers;

DBCC CHECKIDENT ('AspNetUsers', RESEED, 0);

