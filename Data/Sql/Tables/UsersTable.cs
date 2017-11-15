namespace HD
{
  public class UsersTable : ISqlTableMigrator
  {
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
        return "Users";
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
