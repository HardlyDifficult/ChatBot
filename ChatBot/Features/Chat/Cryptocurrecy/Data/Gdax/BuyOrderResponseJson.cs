using Newtonsoft.Json;
using System;

namespace HD
{
  public class BuyOrderResponseJson
  {
    [JsonProperty]
    public readonly string id;

    [JsonProperty]
    public readonly string price;

    [JsonProperty]
    public readonly string size;

    /// <summary>
    /// e.g. BTC-USD
    /// </summary>
    [JsonProperty]
    public readonly string product_id;

    /// <summary>
    /// buy | sell
    /// </summary>
    [JsonProperty]
    public readonly string side;

    /// <summary>
    /// Self trading prevention flag
    /// </summary>
    [JsonProperty]
    public readonly string stp;

    /// <summary>
    /// market | limit | stop
    /// </summary>
    [JsonProperty]
    public readonly string type;

    /// <summary>
    /// GTC
    /// </summary>
    [JsonProperty]
    public readonly string time_in_force;

    [JsonProperty]
    public readonly bool post_only;

    [JsonProperty]
    public readonly DateTime created_at;

    [JsonProperty]
    public readonly string fill_fees;

    [JsonProperty]
    public readonly string filled_size;

    [JsonProperty]
    public readonly string executed_value;

    [JsonProperty]
    public readonly string status;

    [JsonProperty]
    public readonly bool settled;
  }
}
