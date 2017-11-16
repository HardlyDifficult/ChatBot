using System;

namespace HD
{
  public class KeyLongValueTable : KeyValueTable<long>
  {
    public override string valueSqlType
    {
      get
      {
        return "INTEGER";
      }
    }
  }
}
