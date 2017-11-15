﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace HD
{
  public class SchemaTable : ISqlTableMigrator
  {
    const string _tableName = "Schema";
    const string tableField = "TableName";
    const string versionField = "Version";

    long ISqlTableMigrator.currentVersion
    {
      get
      {
        return 0;
      }
    }
    public string tableName
    {
      get
      {
        return _tableName;
      }
    }

    string ISqlTableMigrator.UpgradeTo(
      long version)
    {
      switch (version)
      {
        case 0:
          return $@"
CREATE TABLE `{tableName}` (
  `{tableField}` TEXT NOT NULL PRIMARY KEY,
	`{versionField}`	INTEGER NOT NULL
);
          ";
        default:
          return null;
      }
    }

    public static void UpdateTables()
    {
      List<ISqlTableMigrator> tableMigratorList = ReflectionHelpers.CreateOneOfEach<ISqlTableMigrator>();
      tableMigratorList.Insert(0, new SchemaTable()); // This effectively adds the schema table twice, but ensures it's first

      for (int i = 0; i < tableMigratorList.Count; i++)
      {
        ISqlTableMigrator tableMigrator = tableMigratorList[i];
        string tableName = tableMigrator.tableName;
        long targetVersion = tableMigrator.currentVersion;
        long currentDbVersion = GetCurrentDbVersion(tableName);
        if (targetVersion != currentDbVersion)
        {
          UpgradeTable(tableMigrator, currentDbVersion, targetVersion);
          SetDbVersion(tableName, targetVersion);
        }
      }
    }

    static void SetDbVersion(
      string tableName,
      long version)
    {
      SqlManager.ExecuteNonQuery($@"
REPLACE INTO {SchemaTable._tableName} ({tableField}, {versionField}) VALUES(@table, @version)
        ",
        new SQLiteParameter("@table", tableName),
        new SQLiteParameter("@version", version));
    }

    static long GetCurrentDbVersion(
      string tableName)
    {
      if (SqlManager.TableExists(tableName) == false)
      {
        return -1;
      }
      else
      {
        return (long?)SqlManager.GetScalar(
          SchemaTable._tableName,
          versionField,
          whereClause: $"{tableField}=\"{tableName}\"") ?? -1; // this should use params instead
      }
    }

    static void UpgradeTable(
      ISqlTableMigrator table,
      long currentDbVersion,
      long targetVersion)
    {
      while (currentDbVersion < targetVersion)
      {
        currentDbVersion++;

        string command = table.UpgradeTo(currentDbVersion);
        if (command != null)
        {
          SqlManager.ExecuteNonQuery(command);
        }
      }
    }
  }
}
