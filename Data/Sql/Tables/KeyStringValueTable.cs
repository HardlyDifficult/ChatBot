namespace HD
{
  public class KeyStringValueTable : ISqlTableMigrator
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
        return "KeyStringValue";
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
  `Value` TEXT NOT NULL, 
  PRIMARY KEY(`Key`) );
          ";
        default:
          return null;
      }
    }
  }
}
