namespace HD
{
  public class UptimeTableMigrator : ITableMigrator
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
        return "Uptime";
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
  `StreamEndtimeInTicks` INTEGER NOT NULL, 
  `TimeStreamedInTicks` INTEGER NOT NULL, 
  PRIMARY KEY(`StreamEndtimeInTicks`) );
          ";
        default:
          return null;
      }
    }
  }
}
