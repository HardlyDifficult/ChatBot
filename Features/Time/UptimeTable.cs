namespace HD
{
  public class UptimeTableMigrator : ISqlTableMigrator
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
        return "Uptime";
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
