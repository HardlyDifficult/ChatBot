namespace HD
{
  public class CommandsTable : ISqlTableMigrator
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
        return "Commands";
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
