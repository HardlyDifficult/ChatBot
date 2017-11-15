namespace HD
{
  public class ShoutoutsTableMigrator : ITableMigrator
  {
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

    string ITableMigrator.UpgradeTo(
      long version)
    {
      switch (version)
      {
        case 0:
          return $@"
CREATE TABLE `{tableName}` ( 
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
