using System;
using System.Diagnostics;

namespace HD
{
  public class KeyLongValueTable : KeyValueTable<long>
  {
    public static readonly KeyLongValueTable instance = new KeyLongValueTable();

    public override string valueSqlType
    {
      get
      {
        return "INTEGER";
      }
    }

    KeyLongValueTable()
    {
      Debug.Assert(instance == null || instance == this);
    }
  }
}
