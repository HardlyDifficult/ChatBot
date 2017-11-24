using Newtonsoft.Json;

namespace HD
{
  public class GdaxSettings
  {
    [JsonProperty]
    public string key
    {
      get; set;
    }

    [JsonProperty]
    public string secret
    {
      get; set;
    }

    [JsonProperty]
    public string passphrase
    {
      get; set;
    }
  }
}