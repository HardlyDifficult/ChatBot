using GDax;
using System;
using System.Diagnostics;

namespace HD
{
  public class CryptoWinningsTable : ITableMigrator
  {
    #region Data
    public static readonly CryptoWinningsTable instance = new CryptoWinningsTable();

    long ITableMigrator.currentVersion
    {
      get
      {
        return 0;
      }
    }

    public string tableName
    {
      get
      {
        return "CryptoWinnings";
      }
    }

    const string
      userIdField = "UserId",
      currencyField = "Currency",
      amountField = "Amount",
      buyAmountInUSDField = "BuyAmountInUSD",
      sellAmountInUSDField = "SellAmountInUSD",
      buyOrderIdField = "BuyOrderId",
      sellOrderIdField = "SellOrderId",
      timeInTicksField = "TimeInTicks";
    #endregion

    #region Init
    string ITableMigrator.UpgradeTo(
      long version)
    {
      switch (version)
      {
        case 0:
          return $@"
CREATE TABLE IF NOT EXISTS `{tableName}` 
( 
  `{userIdField}` TEXT NOT NULL, 
  `{currencyField}` TEXT NOT NULL, 
  `{amountField}` TEXT NOT NULL, 
  `{buyAmountInUSDField}` TEXT NOT NULL, 
  `{sellAmountInUSDField}` TEXT NOT NULL, 
  `{buyOrderIdField}` TEXT NOT NULL PRIMARY KEY,
  `{sellOrderIdField}` TEXT NOT NULL,
  `{timeInTicksField}` INTEGER NOT NULL 
);
          ";
        default:
          return null;
      }
    }
    #endregion

    #region Write
    public void AddWinnings(
      string buyerUserId,
      CryptoCurrency currency,
      decimal numberOfCoins,
      decimal buyPriceInUsd,
      decimal sellPriceInUsd,
      string buyOrderId,
      string sellOrderId)
    {
      string sql = $@"
INSERT INTO {tableName} 
(
  {userIdField}, 
  {currencyField}, 
  {amountField}, 
  {buyAmountInUSDField}, 
  {sellAmountInUSDField}, 
  {buyOrderIdField}, 
  {sellOrderIdField}, 
  {timeInTicksField}
)
VALUES 
(
  @{userIdField}, 
  @{currencyField}, 
  @{amountField}, 
  @{buyAmountInUSDField}, 
  @{sellAmountInUSDField}, 
  @{buyOrderIdField}, 
  @{sellOrderIdField}, 
  @{timeInTicksField}
)
        ";

      SqlManager.ExecuteNonQuery(sql,
        ($"@{userIdField}", buyerUserId),
        ($"@{currencyField}", currency.ToString()),
        ($"@{amountField}", numberOfCoins),
        ($"@{buyAmountInUSDField}", buyPriceInUsd),
        ($"@{sellAmountInUSDField}", sellPriceInUsd),
        ($"@{buyOrderIdField}", buyOrderId),
        ($"@{sellOrderIdField}", sellOrderId),
        ($"@{timeInTicksField}", DateTime.Now.Ticks));
    }
    #endregion

    #region Read
    public decimal GetTotalWinningsInUsd(
      string userId)
    {
      string sql = $@"
SELECT SUM(CAST({sellAmountInUSDField} as decimal) - CAST({buyAmountInUSDField} as decimal))
FROM {tableName}
WHERE {userIdField}=@{userIdField}
        ";

      object result = SqlManager.GetScalar(sql,
        ($"@{userIdField}", userId));
      if(result == null || result is DBNull)
      {
        return 0;
      }

      return (decimal)(float)result;
    }
    #endregion
  }
}
