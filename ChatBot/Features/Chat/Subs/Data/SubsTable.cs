using System.Diagnostics;

namespace HD
{
  // TODO wireup
  public class SubsTable : ITableMigrator
  {
    #region Constants
    long ITableMigrator.currentVersion
    {
      get
      {
        return 0;
      }
    }

    const string
      userIdField = "UserId",
      pointsField = "Points";

    public string tableName
    {
      get
      {
        return "Subs";
      }
    }
    #endregion

    #region Data
    public static readonly SubsTable instance = new SubsTable();
    #endregion

    #region Init
    SubsTable()
    {
      Debug.Assert(instance == null || instance == this);
    }

    string ITableMigrator.UpgradeTo(
      long version)
    {
      switch (version)
      {
        case 0:
          return $@"
CREATE TABLE IF NOT EXISTS `{tableName}` 
( 
  `{userIdField}` TEXT NOT NULL PRIMARY KEY, 
  `{pointsField}` INTEGER NOT NULL
);
          ";
        default:
          return null;
      }
    }
    #endregion

    #region Public Write
    public void DropAllSubs()
    {
      string sql = $@"
DELETE FROM {tableName}
        ";

      SqlManager.ExecuteNonQuery(sql);
    }

    public void RecordSub(
      string userId,
      int tier1To3)
    {
      if (userId == TwitchController.instance.twitchChannel.userId)
      {
        return;
      }

      if (tier1To3 == 3)
      {
        tier1To3 = 6;
      }

      string sql = $@"
REPLACE INTO {tableName} ({userIdField}, {pointsField})
VALUES (@{userIdField}, @{pointsField})
        ";

      SqlManager.ExecuteNonQuery(sql,
        ($"@{userIdField}", userId),
        ($"@{pointsField}", tier1To3));
    }
    #endregion

    #region Public Read
    public int GetTotalSubCount()
    {
      string sql = $@"
SELECT SUM({pointsField})
FROM {tableName}
        ";

      return (int)(long)SqlManager.GetScalar(sql);
    }
    #endregion
  }
}
