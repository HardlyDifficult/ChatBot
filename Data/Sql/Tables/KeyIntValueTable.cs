namespace HD
{
  public class KeyIntValueTable : ISqlTableMigrator
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
        return "KeyIntValue";
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
  `Key` TEXT NOT NULL, 
  `LastSentInTicks` INTEGER, 
  `CooldownInSeconds` INTEGER NOT NULL DEFAULT 200, 
  `Value` INTEGER NOT NULL, 
  PRIMARY KEY(`Key`) );
          ";
        default:
          return null;
      }
    }
  }
}
