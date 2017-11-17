using System;
using System.Diagnostics;

namespace HD
{
  /// <summary>
  /// Stores a key / value pair for any unique key.
  /// </summary>
  public abstract class KeyValueTable<TValueType> : ITableMigrator
  {
    #region Constants
    const string 
      keyField = "Key",
      valueField = "Value";

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
        return $"Key{typeof(TValueType).Name}Value";
      }
    }
    #endregion

    #region Data
    public abstract string valueSqlType { get;}
    #endregion

    #region Init
    string ITableMigrator.UpgradeTo(
      long version)
    {
      switch (version)
      {
        case 0:
          return $@"
CREATE TABLE IF NOT EXISTS `{tableName}` (
  `{keyField}` TEXT NOT NULL PRIMARY KEY, 
  `{valueField}` {valueSqlType} NOT NULL);
          ";
        default:
          return null;
      }
    }
    #endregion

    #region Write
    public void SetValue(
      string key, 
      TValueType value)
    {
      string sql = $@"
REPLACE INTO {tableName} ({keyField}, {valueField})
VALUES (@{keyField}, @{valueField})
        ";

      SqlManager.ExecuteNonQuery(
        sql,
        ($"@{keyField}", key),
        ($"@{valueField}", value));
    }
    #endregion

    #region Read
    public bool TryGetValue(
      string key,
      out TValueType value)
    {
      string sql = $@"
SELECT {valueField}
FROM {tableName}
WHERE {keyField}=@{keyField}
        ";

      object result = SqlManager.GetScalar(
        sql,
        ($"@{keyField}", key));

      if(result == null)
      {
        value = default(TValueType);
        return false;
      }

      value = (TValueType)result;
      return true;
    }
    #endregion
  }
}
