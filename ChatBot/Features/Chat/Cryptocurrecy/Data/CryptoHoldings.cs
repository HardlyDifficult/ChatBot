using System.Collections.Generic;
using System.Diagnostics;

namespace HD
{
  public class CryptoHoldings
  {
    public readonly string buyOrderId;
    public readonly CryptoCurrency currency;
    public readonly decimal amountOfCoin;
    public readonly decimal amountInUsd;
    public readonly string buyingUserId;

    public decimal buyPrice
    {
      get
      {
        return amountInUsd / amountOfCoin;
      }
    }

    public CryptoHoldings(
      string buyOrderId,
      CryptoCurrency currency,
      decimal amountOfCoin,
      decimal amountInUsd,
      string buyingUserId)
    {
      this.buyOrderId = buyOrderId;
      this.currency = currency;
      this.amountOfCoin = amountOfCoin;
      this.amountInUsd = amountInUsd;
      this.buyingUserId = buyingUserId;
    }
  }

  public static class CryptoHoldingsExtensions
  {
    public static decimal GetTotalAmountOfCoin(
      this List<CryptoHoldings> holdingsList)
    {
      decimal totalAmountOfCoin = 0;
      for (int i = 0; i < holdingsList.Count; i++)
      {
        CryptoHoldings holdings = holdingsList[i];
        totalAmountOfCoin += holdings.amountOfCoin;
      }

      return totalAmountOfCoin;
    }

    public static decimal GetMinBuyPrice(
      this List<CryptoHoldings> holdingsList)
    {
      Debug.Assert(holdingsList.Count > 0);

      decimal minPrice = holdingsList[0].buyPrice;
      for (int i = 1; i < holdingsList.Count; i++)
      {
        CryptoHoldings holdings = holdingsList[i];
        if(holdings.buyPrice < minPrice)
        {
          minPrice = holdings.buyPrice;
        }
      }

      return minPrice;
    }
  }
}