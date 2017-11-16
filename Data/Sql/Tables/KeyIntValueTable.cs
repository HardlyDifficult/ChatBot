using System;

namespace HD
{
  [Obsolete]
  public class KeyIntValueTable : ITableMigrator
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
        return "KeyIntValue";
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
