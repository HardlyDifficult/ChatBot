namespace HD
{
  public interface ITableMigrator
  {
    string tableName
    {
      get;
    }

    long currentVersion
    {
      get;
    }

    /// <summary>
    /// Returns the Sql command to run, or null.
    /// 
    /// This will be called for each individual version, starting at 0.
    /// e.g. if current version is 3 and we don't have the table yet, then 
    /// this will be called for 0, 1, 2, and then 3.
    /// </summary>
    string UpgradeTo(long version);
  }
}