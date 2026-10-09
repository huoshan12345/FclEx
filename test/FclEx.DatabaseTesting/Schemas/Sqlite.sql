CREATE TABLE "EntityHasStates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EntityHasStates" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    "IsDisabled" INTEGER NOT NULL,
    "DeletedAt" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL
);


CREATE TABLE "EntityWithAutoKey" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EntityWithAutoKey" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "Value" INTEGER NOT NULL
);


CREATE TABLE "EntityWithGuidKey" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_EntityWithGuidKey" PRIMARY KEY,
    "Value" INTEGER NOT NULL,
    "Order" INTEGER NULL
);


CREATE TABLE "EntityWithIdAndIndex" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EntityWithIdAndIndex" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Value" INTEGER NOT NULL
);


CREATE TABLE "EntityWithoutKey" (
    "Name" TEXT NULL,
    "Value" INTEGER NOT NULL
);


CREATE TABLE "EntityWithSqliteBlob" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EntityWithSqliteBlob" PRIMARY KEY AUTOINCREMENT,
    "blob_bytes" blob NULL
);


CREATE TABLE "has_table_name" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_has_table_name" PRIMARY KEY AUTOINCREMENT
);


CREATE TABLE "HasPostfix" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_HasPostfix" PRIMARY KEY AUTOINCREMENT
);


CREATE TABLE "TruncateAttributed" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TruncateAttributed" PRIMARY KEY AUTOINCREMENT
);


CREATE TABLE "TruncateIsolationRow" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TruncateIsolationRow" PRIMARY KEY AUTOINCREMENT,
    "Value" INTEGER NOT NULL
);


CREATE TABLE "TruncateKeylessRow" (
    "Value" INTEGER NOT NULL
);


CREATE TABLE "TruncateManualRow" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TruncateManualRow" PRIMARY KEY
);


CREATE TABLE "TruncateOtherRow" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TruncateOtherRow" PRIMARY KEY AUTOINCREMENT,
    "Value" INTEGER NOT NULL
);


CREATE TABLE "TruncateParent" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TruncateParent" PRIMARY KEY AUTOINCREMENT
);


CREATE TABLE "TruncateRow" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TruncateRow" PRIMARY KEY AUTOINCREMENT,
    "Value" INTEGER NOT NULL
);


CREATE TABLE "EntityWithNavigation" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EntityWithNavigation" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "NavigationId" INTEGER NULL,
    CONSTRAINT "FK_EntityWithNavigation_EntityHasStates_NavigationId" FOREIGN KEY ("NavigationId") REFERENCES "EntityHasStates" ("Id")
);


CREATE TABLE "TruncateChild" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TruncateChild" PRIMARY KEY AUTOINCREMENT,
    "ParentId" INTEGER NOT NULL,
    CONSTRAINT "FK_TruncateChild_TruncateParent_ParentId" FOREIGN KEY ("ParentId") REFERENCES "TruncateParent" ("Id") ON DELETE CASCADE
);


CREATE UNIQUE INDEX "IX_EntityWithIdAndIndex_Name" ON "EntityWithIdAndIndex" ("Name");


CREATE INDEX "IX_EntityWithIdAndIndex_Value" ON "EntityWithIdAndIndex" ("Value");


CREATE INDEX "IX_EntityWithNavigation_NavigationId" ON "EntityWithNavigation" ("NavigationId");


CREATE INDEX "IX_TruncateChild_ParentId" ON "TruncateChild" ("ParentId");
