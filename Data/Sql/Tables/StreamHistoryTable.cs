namespace HD
{
  public class StreamHistoryTable : ISqlTableMigrator
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
        return "StreamHistory";
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
  `TimeInTicks` INTEGER NOT NULL, 
  `State` INTEGER NOT NULL );
          ";
        default:
          return null;
      }
    }
  }
}
