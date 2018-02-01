//using GDax;
//using System;
//using System.Collections.Generic;
//using System.Data.Common;

//namespace HD
//{
//  public class CryptoHoldingsTable : ITableMigrator
//  {
//    #region Data
//    public static readonly CryptoHoldingsTable instance = new CryptoHoldingsTable();

//    long ITableMigrator.currentVersion
//    {
//      get
//      {
//        return 0;
//      }
//    }

//    public string tableName
//    {
//      get
//      {
//        return "CryptoHoldings";
//      }
//    }

//    const string
//      buyOrderIdField = "BuyOrderId",
//      userIdField = "UserId",
//      currencyField = "Currency",
//      amountField = "Amount",
//      amountInUSDField = "AmountinUSD",
//      timeInTicksField = "TimeInTicks";
//    #endregion

//    #region Init
//    string ITableMigrator.UpgradeTo(
//      long version)
//    {
//      switch (version)
//      {
//        case 0:
//          return $@"
//CREATE TABLE IF NOT EXISTS `{tableName}` ( 
//  `{buyOrderIdField}` TEXT NOT NULL PRIMARY KEY, 
//  `{userIdField}` TEXT NOT NULL, 
//  `{currencyField}` TEXT NOT NULL, 
//  `{amountField}` TEXT NOT NULL, 
//  `{amountInUSDField}` TEXT NOT NULL, 
//  `{timeInTicksField}` INTEGER NOT NULL );
//          ";
//        default:
//          return null;
//      }
//    }
//    #endregion

//    #region Write
//    public void AddHolding(
//      string userId,
//      CryptoCurrency currency,
//      decimal coinsPurchased,
//      decimal dollarsSpent,
//      string buyOrderId)
//    {
//      string sql = $@"
//INSERT INTO {tableName}
//(
//  {buyOrderIdField},
//  {userIdField}, 
//  {currencyField}, 
//  {amountField}, 
//  {amountInUSDField}, 
//  {timeInTicksField}
//)
//VALUES 
//(
//  @{buyOrderIdField}, 
//  @{userIdField}, 
//  @{currencyField}, 
//  @{amountField}, 
//  @{amountInUSDField}, 
//  @{timeInTicksField}
//)
//        ";

//      SqlManager.ExecuteNonQuery(sql,
//        ($"@{buyOrderIdField}", buyOrderId),
//        ($"@{userIdField}", userId),
//        ($"@{currencyField}", currency.ToString()),
//        ($"@{amountField}", coinsPurchased),
//        ($"@{amountInUSDField}", dollarsSpent),
//        ($"@{timeInTicksField}", DateTime.Now.Ticks));
//    }

//    public void RemoveHolding(
//      string buyOrderId)
//    {
//      string sql = $@"
//DELETE FROM {tableName}
//WHERE {buyOrderIdField}=@{buyOrderIdField}
//        ";

//      SqlManager.ExecuteNonQuery(sql, ($"@{buyOrderIdField}", buyOrderId));
//    }
//    #endregion

//    #region Read
//    public bool HasHoldings(
//      string userId)
//    {
//      string sql = $@"
//SELECT Count(*) 
//FROM {tableName}
//WHERE {userIdField}=@{userIdField}
//        ";

//      return SqlManager.ExecuteNonQuery(sql, ($"@{userIdField}", userId));
//    }

//    public List<CryptoHoldings> GetHoldings(
//      CryptoCurrency currency)
//    {
//      string sql = $@"
//SELECT {buyOrderIdField}, {userIdField}, {amountInUSDField}, {amountField}
//FROM {tableName}
//WHERE {currencyField}=@{currencyField}
//        ";

//      using (DbDataReader reader = SqlManager.GetReader(sql,
//        ($"@{currencyField}", currency.ToString())))
//      {
//        List<CryptoHoldings> holdingsList = new List<CryptoHoldings>();
//        while (reader.Read())
//        {
//          CryptoHoldings holdings = new CryptoHoldings(
//            (string)reader[buyOrderIdField],
//            currency,
//            decimal.Parse((string)reader[amountField]),
//            decimal.Parse((string)reader[amountInUSDField]),
//            (string)reader[userIdField]);
//          holdingsList.Add(holdings);
//        }
//        return holdingsList;
//      }
//    }

//    public CryptoHoldings GetHoldings(
//      string userId)
//    {
//      string sql = $@"
//SELECT {buyOrderIdField}, {currencyField}, {userIdField}, {amountInUSDField}, {amountField}
//FROM {tableName}
//WHERE {userIdField}=@{userIdField}
//        ";

//      using (DbDataReader reader = SqlManager.GetReader(sql,
//        ($"@{userIdField}", userId)))
//      {
//        if (reader.Read())
//        {
//          CryptoHoldings holdings = new CryptoHoldings(
//            (string)reader[buyOrderIdField],
//            (CryptoCurrency)Enum.Parse(typeof(CryptoCurrency), (string)reader[currencyField], true),
//            decimal.Parse((string)reader[amountField]),
//            decimal.Parse((string)reader[amountInUSDField]),
//            (string)reader[userIdField]);
//          return holdings;
//        }
//      }

//      return null;
//    }

//    public CryptoHoldings GetLowestBuy(
//      CryptoCurrency currency)
//    {
//      string sql = $@"
//SELECT {buyOrderIdField}, {userIdField}, {amountInUSDField}, {amountField}
//FROM {tableName}
//WHERE {currencyField}=@{currencyField}
//ORDER BY {amountInUSDField} DESC
//LIMIT 1
//        ";

//      using (DbDataReader dataReader = SqlManager.GetReader(sql, ($"@{currencyField}", currency.ToString())))
//      {
//        if (dataReader.Read())
//        {
//          string buyOrderId = (string)dataReader[buyOrderIdField];
//          string userId = (string)dataReader[userIdField];
//          decimal amountInUsd = decimal.Parse((string)dataReader[amountInUSDField]);
//          decimal amountOfCoins = decimal.Parse((string)dataReader[amountField]);

//          return new CryptoHoldings(buyOrderId, currency, amountOfCoins, amountInUsd, userId);
//        }
//      }

//      return null;
//    }
//    #endregion
//  }
//}
