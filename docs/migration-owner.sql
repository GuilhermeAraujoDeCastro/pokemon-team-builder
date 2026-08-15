BEGIN TRANSACTION;

ALTER TABLE "Teams" ADD "LegacyIsPublic" INTEGER NULL;

ALTER TABLE "Teams" ADD "LegacyOwnerId" TEXT NULL;

CREATE TABLE "ef_temp_Teams" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Teams" PRIMARY KEY AUTOINCREMENT,
    "CreatedAtUtc" TEXT NOT NULL,
    "DeletedAtUtc" TEXT NULL,
    "IsPublic" INTEGER NOT NULL,
    "LegacyIsPublic" INTEGER NULL,
    "LegacyOwnerId" TEXT NULL,
    "Name" TEXT NOT NULL,
    "UserId" TEXT NULL
);

INSERT INTO "ef_temp_Teams" ("Id", "CreatedAtUtc", "DeletedAtUtc", "IsPublic", "LegacyIsPublic", "LegacyOwnerId", "Name", "UserId")
SELECT "Id", "CreatedAtUtc", "DeletedAtUtc", "IsPublic", "LegacyIsPublic", "LegacyOwnerId", "Name", "UserId"
FROM "Teams";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Teams";

ALTER TABLE "ef_temp_Teams" RENAME TO "Teams";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261101221834_OwnerQuarantineColumns', '8.0.30');

COMMIT;

BEGIN TRANSACTION;

UPDATE Teams
SET LegacyOwnerId = UserId, LegacyIsPublic = IsPublic,
    UserId = NULL, IsPublic = 0
WHERE UserId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM AspNetUsers u WHERE u.Id = Teams.UserId);

CREATE INDEX "IX_Teams_UserId" ON "Teams" ("UserId");

CREATE TABLE "ef_temp_Teams" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Teams" PRIMARY KEY AUTOINCREMENT,
    "CreatedAtUtc" TEXT NOT NULL,
    "DeletedAtUtc" TEXT NULL,
    "IsPublic" INTEGER NOT NULL,
    "LegacyIsPublic" INTEGER NULL,
    "LegacyOwnerId" TEXT NULL,
    "Name" TEXT NOT NULL,
    "UserId" TEXT NULL,
    CONSTRAINT "FK_Teams_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Teams" ("Id", "CreatedAtUtc", "DeletedAtUtc", "IsPublic", "LegacyIsPublic", "LegacyOwnerId", "Name", "UserId")
SELECT "Id", "CreatedAtUtc", "DeletedAtUtc", "IsPublic", "LegacyIsPublic", "LegacyOwnerId", "Name", "UserId"
FROM "Teams";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Teams";

ALTER TABLE "ef_temp_Teams" RENAME TO "Teams";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_Teams_UserId" ON "Teams" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261101221951_EnforceTeamOwner', '8.0.30');

COMMIT;

