using System;
using System.Collections.Generic;

namespace HD
{
  public class SchemaTable : ITableMigrator
  {
    const string _tableName = "Schema";
    const string tableField = "TableName";
    const string versionField = "Version";

    long ITableMigrator.currentVersion
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

    string ITableMigrator.UpgradeTo(
      long version)
    {
      switch (version)
      {
        case 0:
          return $@"
CREATE TABLE IF NOT EXISTS `{tableName}` (
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
      List<ITableMigrator> tableMigratorList = ReflectionHelpers.CreateOneOfEach<ITableMigrator>();
      tableMigratorList.Insert(0, new SchemaTable()); // This effectively adds the schema table twice, but ensures it's first

      for (int i = 0; i < tableMigratorList.Count; i++)
      {
        ITableMigrator tableMigrator = tableMigratorList[i];
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
        ("@table", tableName),
        ("@version", version));
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
        string sql = $@"
SELECT {versionField}
FROM {_tableName}
WHERE {tableField}=@{tableField}
          ";

        object result = SqlManager.GetScalar(sql, ($"@{tableField}", tableName));
        if(result == null)
        {
          return -1;
        }

        return (long)result;
      }
    }

    static void UpgradeTable(
      ITableMigrator table,
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
