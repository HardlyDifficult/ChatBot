using System.Diagnostics;

namespace HD
{
  public class ShoutoutsTableMigrator : ITableMigrator
  {
    public static readonly ShoutoutsTableMigrator instance = new ShoutoutsTableMigrator();

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
        return "Shoutouts";
      }
    }

    ShoutoutsTableMigrator()
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
CREATE TABLE IF NOT EXISTS `{tableName}` ( 
  `UserId` TEXT NOT NULL, 
  `Message` TEXT NOT NULL, 
  PRIMARY KEY(`UserId`) );
          ";
        default:
          return null;
      }
    }
  }
}
