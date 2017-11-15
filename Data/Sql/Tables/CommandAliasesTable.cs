namespace HD
{
  public class CommandAliasesTableigrator : ISqlTableMigrator
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
        return "CommandAliases";
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
  `Alias` TEXT NOT NULL, 
  PRIMARY KEY(`Alias`) );
          ";
        default:
          return null;
      }
    }
  }
}
