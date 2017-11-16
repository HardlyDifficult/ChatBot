using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

namespace HD
{
  /// <summary>
  /// Aliases are additional trigger words for a command.
  /// </summary>
  public class CommandAliasesTable : ITableMigrator
  {
    #region Constants
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
        return "CommandAliases";
      }
    }

    const string
      commandField = "Command",
      aliasField = "Alias";
    #endregion

    #region Data
    public static CommandAliasesTable instance;
    #endregion

    #region Init
    public CommandAliasesTable()
    {
      Debug.Assert(instance == null);

      instance = this;
    }

    string ITableMigrator.UpgradeTo(
      long version)
    {
      switch (version)
      {
        case 0:
          return $@"
CREATE TABLE IF NOT EXISTS `{tableName}` ( 
  `{aliasField}` TEXT NOT NULL PRIMARY KEY, 
  `{commandField}` TEXT NOT NULL);
          ";
        default:
          return null;
      }
    }
    #endregion

    #region Write
    public bool CreateAlias(
      string commandOrAlias,
      string alias)
    {
      SqlTwitchCommand twitchCommand = CommandsTable.instance.GetCommand(commandOrAlias); 
      if (twitchCommand.isValid == false)
      { // Command not found
        return false;
      }

      string sql = $@"
INSERT INTO {tableName}({commandField}, {aliasField}) 
VALUES(@{commandField}, @{aliasField})
        ";

      return SqlManager.ExecuteNonQuery(sql,
        ($"@{commandField}", twitchCommand.command),
        ($"@{aliasField}", alias));
    }

    public bool DeleteAlias(
      string alias)
    {
      string sql = $@"
DELETE FROM {tableName}
WHERE {aliasField}=@{aliasField}
        ";

      return SqlManager.ExecuteNonQuery(sql,
        ($"@{aliasField}", alias));
    }

    /// <summary>
    /// Called when a command is deleted.
    /// TODO confirm
    /// </summary>
    public bool DeleteAllAliasesForCommand(
      string commandOrAlias)
    {
      SqlTwitchCommand twitchCommand = CommandsTable.instance.GetCommand(commandOrAlias); 
      if (twitchCommand.isValid == false)
      { // Command not found
        return false;
      }

      string sql = $@"
DELETE FROM {tableName}
WHERE {commandField}=@{commandField}
        ";

      return SqlManager.ExecuteNonQuery(sql,
        ($"@{commandField}", twitchCommand.command));
    }
    #endregion

    #region Read
    public (string command, List<string> aliasList) GetAliases(
      string commandOrAlias)
    {
      SqlTwitchCommand twitchCommand = CommandsTable.instance.GetCommand(commandOrAlias); 
      if (twitchCommand.isValid == false)
      { // Command not found
        return (null, null);
      }

      string sql = $@"
SELECT {aliasField}
FROM {tableName}
WHERE {commandField}=@{commandField}
        ";

      using (DbDataReader reader = SqlManager.GetReader(sql, ($"@{commandField}", twitchCommand.command)))
      {
        List<string> aliasList = new List<string>();
        while (reader.Read())
        {
          aliasList.Add((string)reader[aliasField]);
        }
        return (twitchCommand.command, aliasList);
      }
    }

    public string GetCommandNameForAlias(
      string alias)
    {
      string sql = $@"
SELECT {commandField}
FROM {tableName}
WHERE {aliasField}=@{aliasField}
        ";

      return (string)SqlManager.GetScalar(sql, ($"@{aliasField}", alias));
    }
    #endregion
  }
}
