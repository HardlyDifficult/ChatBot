using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace HD
{
  public class Gdax
  {
    public static readonly Gdax instance = new Gdax();

    const string baseUrl = "https://api-public.sandbox.gdax.com/";

    Gdax() { }

    #region Write
    public CryptoHoldings Buy(
      CryptoCurrency currency, 
      decimal buyAmountInUsd,
      string buyingUserId)
    {
      if(HDWebClient.GetHTML($"{baseUrl}time", out string timeResults) == false)
      {
        return null;
      }
      TimeJson time = JsonConvert.DeserializeObject<TimeJson>(timeResults);
      string timestamp = time.epoch.ToString();

      string requestPath = "/orders";

      MarketOrderJson marketOrder = MarketOrderJson.CreateBuyOrder(currency, buyAmountInUsd);
      string body = JsonConvert.SerializeObject(marketOrder);
      string method = "POST";
      
      // create the prehash string by concatenating required parts
      string whatToSign = timestamp + method + requestPath + body;
      byte[] whatToSignBytes = whatToSign.ToBytesEncoded();

      // decode the base64 secret
      byte[] key = Convert.FromBase64String(BotSettings.gdax.secret);

      // create a sha256 hmac with the secret
      HMAC hmac = HMACSHA256.Create();
      hmac.Key = key;

      // sign the require message with the hmac
      // and finally base64 encode the result
      string sign = Convert.ToBase64String(hmac.ComputeHash(whatToSignBytes));

      string results = HDWebClient.Post(
        $"{baseUrl}orders",
        new(string, string)[] {
          ("CB-ACCESS-KEY", BotSettings.gdax.key),
          ("CB-ACCESS-SIGN", sign),
          ("CB-ACCESS-TIMESTAMP", timestamp),
          ("CB-ACCESS-PASSPHRASE", BotSettings.gdax.passphrase)
        });
      BuyOrderResponseJson response = JsonConvert.DeserializeObject<BuyOrderResponseJson>(results);
      
      if(response.settled == false)
      {
        BotLogic.instance.SendModReply(null, $"Settle failed: {results}");
        // TODO
        return null;
      }

      return new CryptoHoldings(response.id, currency, 
        amountOfCoin: decimal.Parse(response.filled_size), 
        amountInUsd: decimal.Parse(response.executed_value),
        buyingUserId: buyingUserId);
    }

    public (string orderId, decimal amount) Sell(
      CryptoCurrency currency, 
      decimal numberOfCoins)
    {
      return ("aoenut", 1);
    }
    #endregion

    #region Read
    public decimal GetMarketPrice(
      CryptoCurrency currency)
    {
      string url = $"{baseUrl}/products/{currency}-USD/ticker";
      if(HDWebClient.GetHTML(url, out string result) == false)
      {
        return 0;
      }

      ProductTickerJson productTicker = JsonConvert.DeserializeObject<ProductTickerJson>(result);
      return productTicker.price;
    }
    #endregion
  }
}
