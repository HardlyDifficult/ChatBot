using Newtonsoft.Json;
using System;

namespace HD
{
  public class ProductTickerJson
  {
    [JsonProperty]
    public readonly int trade_id;

    [JsonProperty]
    public readonly decimal price;

    [JsonProperty]
    public readonly decimal size;

    [JsonProperty]
    public readonly decimal bid;

    [JsonProperty]
    public readonly decimal ask;

    [JsonProperty]
    public readonly decimal volume;

    [JsonProperty]
    public readonly DateTime time;
  }
}
