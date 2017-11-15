namespace HD
{
  public class CommandsTable : ITableMigrator
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
        return "Commands";
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
  `Command` TEXT NOT NULL, 
  `LastSentInTicks` INTEGER, 
  `Response` TEXT NOT NULL, 
  `UserLevel` INTEGER NOT NULL, 
  `CooldownInSeconds` INTEGER NOT NULL );
          ";
        default:
          return null;
      }
    }
  }
}
