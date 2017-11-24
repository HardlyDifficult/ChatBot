using Newtonsoft.Json;
using System;

namespace HD
{
  public class TimeJson
  {
    [JsonProperty]
    public readonly string iso;

    [JsonProperty]
    public readonly decimal epoch;
  }
}
