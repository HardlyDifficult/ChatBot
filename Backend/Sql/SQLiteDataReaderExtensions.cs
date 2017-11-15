using System;
using System.Data.SQLite;

namespace HD
{
  public static class SQLiteDataReaderExtensions
  {
    public static long GetLong(
      this SQLiteDataReader reader,
      string key)
    {
      object value = reader[key];
      if (value == null
        || value is DBNull)
      {
        return 0;
      }

      return (long)reader[key];
    }
  }
}
