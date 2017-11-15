namespace HD
{
  public class StreamHistoryTable : ITableMigrator
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
        return "StreamHistory";
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
  `TimeInTicks` INTEGER NOT NULL, 
  `State` INTEGER NOT NULL );
          ";
        default:
          return null;
      }
    }
  }
}
