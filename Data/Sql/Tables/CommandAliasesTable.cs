namespace HD
{
  public class CommandAliasesTable : ITableMigrator
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
        return "CommandAliases";
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
  `Alias` TEXT NOT NULL, 
  PRIMARY KEY(`Alias`) );
          ";
        default:
          return null;
      }
    }
  }
}
