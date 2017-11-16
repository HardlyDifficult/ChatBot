using System;
using System.Data.Common;
using System.Diagnostics;

namespace HD
{
  public class CooldownTable : ITableMigrator
  {
    #region Data
    public static CooldownTable instance;

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

    #region Init
    public CooldownTable()
    {
      Debug.Assert(instance == null);

      instance = this;
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

    #region Read
    public bool IsReady(
      string key)
    {
      string sql = $@"
SELECT {cooldownField}, {timeLastIssuedField} FROM {tableName} WHERE {keyField}=@{keyField}
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
        } else
        { // Command has never been run before!
          return true;
        }
      }

      return (DateTime.Now - lastIssued) >= cooldown;
    }
    #endregion
  }
}
