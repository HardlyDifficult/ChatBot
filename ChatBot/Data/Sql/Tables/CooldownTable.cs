using System;
using System.Data.Common;
using System.Diagnostics;

namespace HD
{
  /// <summary>
  /// You must define the cooldown for a key, otherwise it is always ready.
  /// </summary>
  public class CooldownTable : ITableMigrator
  {
    #region Constants
    public string tableName
    {
      get
      {
        return "Cooldown";
      }
    }

    const string keyField = "Key";

    const string cooldownField = "CooldownInTicks";

    const string timeLastIssuedField = "TimeLastIssuedInTicks";

    long ITableMigrator.currentVersion
    {
      get
      {
        return 0;
      }
    }
    #endregion

    #region Data
    public static readonly CooldownTable instance = new CooldownTable();
    #endregion

    #region Init
    CooldownTable()
    {
      Debug.Assert(instance == null || instance == this);

      // TODO change to use static readonly instance.  
      // how -to vs the reflection loader and do the same for features.
      CommandsTable.instance.onCommandDeleted += Instance_onCommandDeleted;
    }
    
    string ITableMigrator.UpgradeTo(
      long version)
    {
      switch (version)
      {
        case 0:
          return $@"
CREATE TABLE `{tableName}` (
	`{keyField}` TEXT NOT NULL PRIMARY KEY,
	`{cooldownField}`	INTEGER NOT NULL,
	`{timeLastIssuedField}`	INTEGER NOT NULL
);
          ";
        default:
          return null;
      }
    }
    #endregion

    #region Events
    void Instance_onCommandDeleted(
      string commandName)
    {
      DeleteCooldown(commandName);
    }
    #endregion

    #region Write
    public void SetTime(
      string key)
    {
      string sql = $@"
UPDATE {tableName} 
SET {timeLastIssuedField}=@{timeLastIssuedField}
WHERE {keyField}=@{keyField};
        ";

      SqlManager.ExecuteNonQuery(
        sql,
        ($"@{timeLastIssuedField}", DateTime.Now.Ticks),
        ($"@{keyField}", key));
    }

    public void SetCooldown(
      string key,
      TimeSpan cooldown)
    {
      string sql = $@"
REPLACE INTO {tableName} ({keyField}, {cooldownField}, {timeLastIssuedField})
VALUES (@{keyField}, @{cooldownField}, @{timeLastIssuedField})
        ";

      SqlManager.ExecuteNonQuery(
        sql,
        ($"@{keyField}", key),
        ($"@{timeLastIssuedField}", 0),
        ($"@{cooldownField}", cooldown.Ticks));
    }

    public bool DeleteCooldown(
      string key)
    {
      string sql = $@"
DELETE FROM {tableName}
WHERE {keyField}=@{keyField}
        ";

      return SqlManager.ExecuteNonQuery(sql, ($"@{keyField}", key));
    }
    #endregion

    #region Read
    public bool IsReady(
      string key)
    {
      string sql = $@"
SELECT {cooldownField}, {timeLastIssuedField} 
FROM {tableName} 
WHERE {keyField}=@{keyField}
        ";

      TimeSpan cooldown;
      DateTime lastIssued;
      using (DbDataReader reader = SqlManager.GetReader(sql, ($"@{keyField}", key)))
      {
        if (reader.Read())
        {
          long cooldownInTicks = (long)reader[cooldownField];
          cooldown = new TimeSpan(cooldownInTicks);
          long lastIssuedInTicks = (long)reader[timeLastIssuedField];
          lastIssued = new DateTime(lastIssuedInTicks);
        }
        else
        { // Command has never been run before and has no cooldown defined
          SetCooldown(key, TimeSpan.Zero); 
          return true;
        }
      }

      return (DateTime.Now - lastIssued) >= cooldown;
    }
    #endregion
  }
}
