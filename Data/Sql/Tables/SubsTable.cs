namespace HD
{
  public class SubsTable : ITableMigrator
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
        return "Subs";
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
  `Points` INTEGER NOT NULL, 
  PRIMARY KEY(`UserId`) );
          ";
        default:
          return null;
      }
    }
  }
}
