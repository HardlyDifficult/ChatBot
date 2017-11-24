using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HD
{
  public class MarketOrderJson
  {
    [JsonProperty]
    public readonly string side; // buy or sell

    [JsonProperty]
    public readonly string type = "market";

    [JsonProperty]
    public readonly string product_id; // e.g. 'BTC-USD'

    [JsonProperty]
    public readonly string funds;

    MarketOrderJson(
      string side,
      string product_id,
      string funds)
    {
      this.side = side;
      this.product_id = product_id;
      this.funds = funds;
    }

    public static MarketOrderJson CreateBuyOrder(
      CryptoCurrency currency,
      decimal fundsInUsd)
    {
      return new MarketOrderJson("buy", $"{currency}-USD", fundsInUsd.ToString());
    }
  }
}
