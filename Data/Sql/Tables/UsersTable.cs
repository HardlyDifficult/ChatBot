namespace HD
{
  public class UsersTable : ITableMigrator
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
        return "Users";
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
  `UserId` TEXT NOT NULL, 
  `HasAutoFollowed` INTEGER, 
  `UserName` TEXT NOT NULL, 
  `UserLevel` INTEGER NOT NULL, 
  `DateLastSeenInTicks` INTEGER, 
  PRIMARY KEY(`UserId`) );
          ";
        default:
          return null;
      }
    }
  }
}
