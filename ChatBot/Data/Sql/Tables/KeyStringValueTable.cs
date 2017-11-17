using System;
using System.Diagnostics;

namespace HD
{
  public class KeyStringValueTable : KeyValueTable<string>
  {
    public static readonly KeyStringValueTable instance = new KeyStringValueTable();

    public override string valueSqlType
    {
      get
      {
        return "TEXT";
      }
    }

    KeyStringValueTable()
    {
      Debug.Assert(instance == null || instance == this);
    }
  }
}
