using System;
using System.Collections.Generic;
using System.Text;

namespace HD
{
  /// <summary>
  /// TODO 
  ///  - an on/off switch
  ///  - account funds available (may block a buy order)
  /// </summary>
  public class CryptocurrencyFeatures : IBotFeature
  {
    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!buy",
        helpMessage: "!buy BTC|LTC|ETH",
        minimumUserLevel: UserLevel.Everyone,
        onCommand: OnCommandBuy));

      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!sell",
        helpMessage: "!sell BTC|LTC|ETH",
        minimumUserLevel: UserLevel.Everyone,
        onCommand: OnCommandSell));

      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!quote",
        helpMessage: null,
        minimumUserLevel: UserLevel.Everyone,
        onCommand: OnCommandQuote));

      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!holding",
        helpMessage: null,
        minimumUserLevel: UserLevel.Everyone,
        onCommand: OnCommandHolding));
    }

    /// <summary>
    /// Messages overall holdings as well as the user who asked (if any).
    /// </summary>
    void OnCommandHolding(
      Message message)
    {
      StringBuilder builder = new StringBuilder();

      bool isFirstEntry = true;
      for (int i = 0; i < Enum.GetValues(typeof(CryptoCurrency)).Length; i++)
      {
        CryptoCurrency currency = (CryptoCurrency)i;
        List<CryptoHoldings> holdingsList = CryptoHoldingsTable.instance.GetHoldings(currency);
        if (holdingsList == null || holdingsList.Count == 0)
        {
          continue;
        }


        if (isFirstEntry)
        {
          builder.Append("We have ");
        }
        else
        {
          builder.Append(", ");
        }
        decimal totalAmountOfCoin = holdingsList.GetTotalAmountOfCoin();
        builder.Append(totalAmountOfCoin);
        builder.Append(" ");
        builder.Append(currency);
        builder.Append(" (");
        decimal currencyMarketPrice = Gdax.instance.GetMarketPrice(currency);
        decimal holdingsValue = currencyMarketPrice * totalAmountOfCoin;
        builder.Append(holdingsValue);
        builder.Append(")");
        if (holdingsList.Count > 1)
        {
          builder.Append(" as low as ");
          decimal minPrice = holdingsList.GetMinBuyPrice();
          builder.Append(minPrice);
          builder.Append("/");
          builder.Append(currency);
        }
        isFirstEntry = false;
      }

      CryptoHoldings myHoldings = CryptoHoldingsTable.instance.GetHoldings(message.user.userId);
      if (myHoldings != null)
      {
        builder.Append(". ");
        builder.Append(message.user.displayName);
        builder.Append(" has ");
        builder.Append(myHoldings.amountOfCoin);
        builder.Append(" ");
        builder.Append(myHoldings.currency);
        builder.Append(" (");

        decimal currencyMarketPrice = Gdax.instance.GetMarketPrice(myHoldings.currency);
        decimal holdingsValue = currencyMarketPrice * myHoldings.amountOfCoin;
        builder.Append(holdingsValue);
        builder.Append(") ");

        decimal percent = holdingsValue / myHoldings.amountInUsd;
        if (percent >= 1)
        { // Making money
          builder.Append("a ");
          builder.Append(percent);
          builder.Append("% return!");
        }
        else
        { // Losing money
          builder.Append(" currently at ");
          builder.Append(percent);
          builder.Append("% the purchase price.");
        }
      }

      const string quoteKey = "!quote";

      bool isReady = CooldownTable.instance.IsReady(quoteKey);
      BotLogic.instance.SendMessageOrWhisper(message, builder.ToString(), isReady);
    }

    /// <summary>
    /// Gets the current market value of each currency.
    /// </summary>
    void OnCommandQuote(
      Message message)
    {
      StringBuilder builder = new StringBuilder();
      for (int i = 0; i < Enum.GetValues(typeof(CryptoCurrency)).Length; i++)
      {
        if (i > 0)
        {
          builder.Append(", ");
        }
        CryptoCurrency currency = (CryptoCurrency)i;
        builder.Append(currency);
        builder.Append(" ");
        builder.Append(Gdax.instance.GetMarketPrice(currency));
      }

      const string quoteKey = "!quote";

      bool isReady = CooldownTable.instance.IsReady(quoteKey);
      BotLogic.instance.SendMessageOrWhisper(message, builder.ToString(), isReady);
    }

    void OnCommandBuy(
      Message message)
    {
      string currencyString = message.message.GetBetween(" ", " ");
      if (Enum.TryParse(currencyString, out CryptoCurrency currency) == false)
      {
        // TODO fail message, bad currency
        return;
      }

      if (CryptoHoldingsTable.instance.HasHoldings(message.user.userId))
      {
        // TODO fail message, one holding
        return;
      }

      // TODO add a bonus table of sorts (like on sub)
      decimal winningsHistory = CryptoWinningsTable.instance.GetTotalWinningsInUsd(message.user.userId);
      decimal buyPrice = .1m + winningsHistory;
      if(buyPrice <= .001m)
      {
        // TODO fail you're broke
        return;
      }

      CryptoHoldings holdings = Gdax.instance.Buy(
        currency, 
        buyAmountInUsd: buyPrice, 
        buyingUserId: message.user.userId);
      if (holdings == null)
      {
        // TODO something failed;
        return;
      }

      CryptoHoldingsTable.instance.AddHolding(
        message.user.userId,
        currency,
        holdings.amountOfCoin,
        holdings.amountInUsd,
        holdings.buyOrderId);

      BotLogic.instance.SendMessageOrWhisper(message,
        $"Bought {holdings.amountOfCoin} {currency} for {holdings.amountInUsd:C} in the name of {message.user.displayName}",
        true);
    }

    void OnCommandSell(
      Message message)
    {
      string currencyString = message.message.GetBetween(" ", " ");
      if (Enum.TryParse(currencyString, out CryptoCurrency currency) == false)
      {
        // TODO fail message, bad currency
        return;
      }

      CryptoHoldings myHoldings = CryptoHoldingsTable.instance.GetHoldings(message.user.userId);
      if (myHoldings == null)
      {
        // TODO you don't have any holdings
        return;
      }

      decimal marketPrice = Gdax.instance.GetMarketPrice(currency);
      decimal expectedSaleInUsd = marketPrice * myHoldings.amountOfCoin;
      if (expectedSaleInUsd <= myHoldings.amountInUsd)
      {
        // TODO fail no selling at a loss ...yet
        return;
      }

      (string sellOrderId, decimal sellPriceInUsd) = Gdax.instance.Sell(currency, myHoldings.amountOfCoin);

      CryptoHoldingsTable.instance.RemoveHolding(myHoldings.buyOrderId);

      CryptoWinningsTable.instance.AddWinnings(
        buyerUserId: myHoldings.buyingUserId,
        currency: currency,
        numberOfCoins: myHoldings.amountOfCoin,
        buyPriceInUsd: myHoldings.amountInUsd,
        sellPriceInUsd: sellPriceInUsd,
        buyOrderId: myHoldings.buyOrderId,
        sellOrderId: sellOrderId);

      decimal returnAmount = sellPriceInUsd - myHoldings.amountInUsd;
      decimal returnPercent = returnAmount / myHoldings.amountInUsd;

      // TODO message if buyer and seller are the same
      BotLogic.instance.SendMessageOrWhisper(message,
        $"Sold {myHoldings.amountOfCoin} {currency} for {sellPriceInUsd}. That's a {returnPercent} return ({returnAmount}) from {message.user.displayName}",
        true);
    }
  }
}
