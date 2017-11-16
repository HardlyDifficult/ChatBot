using System;

namespace HD
{
  public class KeyStringValueTable : KeyValueTable<string>
  {
    public override string valueSqlType
    {
      get
      {
        return "Text";
      }
    }
  }
}
