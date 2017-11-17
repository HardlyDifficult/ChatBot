using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

namespace HD
{
  /// <summary>
  /// Manages commands created by the bot and stored in the database.
  /// </summary>
  public class CommandsTable : ITableMigrator
  {
    #region Constants
    long ITableMigrator.currentVersion
    {
      get
      {
        return 2;
      }
    }

    const string
      commandField = "Command",
      responseField = "Response",
      userLevelField = "UserLevel";

    public string tableName
    {
      get
      {
        return "Commands";
      }
    }
    #endregion

    #region Data
    public static CommandsTable instance;
    #endregion

    #region Init
    public CommandsTable()
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
  `{commandField}` TEXT NOT NULL, 
  `LastSentInTicks` INTEGER, 
  `{responseField}` TEXT NOT NULL, 
  `{userLevelField}` INTEGER NOT NULL, 
  `CooldownInSeconds` INTEGER NOT NULL );
          ";
        case 1:
          // We need to preserve the version that existed, 
          // but never need to do this upgrade again since the next
          // includes all changes required.
          return null;
        case 2:
          return SqlManager.GetSqlAlterTable(tableName, $@"
  `{commandField}` TEXT NOT NULL PRIMARY KEY, 
  `{responseField}` TEXT NOT NULL, 
  `{userLevelField}` INTEGER NOT NULL
            ", $"{commandField}, {responseField}, {userLevelField}");
        default:
          return null;
      }
    }
    #endregion

    #region Write
    /// <summary>
    /// Updates an existing command or creates a new one.
    /// </summary>
    /// <param name="commandOrAlias">
    /// Searches existing commands and aliases, if found then an update is performed.
    /// Otherwise a new command with this name is created.
    /// </param>
    public bool CreateOrUpdateCommand(
      string commandOrAlias,
      string commandText,
      UserLevel userLevel)
    {
      SqlTwitchCommand existingCommand = GetCommand(commandOrAlias);
      string command;
      if (existingCommand.isValid)
      {
        command = existingCommand.command;
      }
      else
      {
        command = commandOrAlias;
      }

      return CreateOrUpdateCommandByName(command, commandText, userLevel);
    }

    public bool DeleteCommand(
      string commandOrAlias)
    {
      SqlTwitchCommand command = GetCommand(commandOrAlias);
      if (command.isValid == false)
      { // Command not found
        return false;
      }

      return DeleteCommandByName(command.command);
    }
    #endregion

    #region Read
    internal SqlTwitchCommand GetCommand(
     string commandOrAlias,
     bool shouldPreventRecursion = false) // TODO if detected, then drop the aliases instead of this param
    {
      if (string.IsNullOrWhiteSpace(commandOrAlias))
      {
        return default(SqlTwitchCommand);
      }

      string sql = $@"
SELECT *
FROM {tableName}
WHERE {commandField}=@{commandField}
COLLATE NOCASE
        ";

      using (DbDataReader reader = SqlManager.GetReader(sql, ($"@{commandField}", commandOrAlias)))
      {
        if (reader.Read())
        {
          return new SqlTwitchCommand(
            (string)reader[commandField],
            (string)reader[responseField],
            (UserLevel)(long)reader[userLevelField]);
        }
        else if (shouldPreventRecursion == false)
        {
          return GetCommand(CommandAliasesTable.instance.GetCommandNameForAlias(commandOrAlias), shouldPreventRecursion: true);
        }
        else
        {
          return default(SqlTwitchCommand);
        }
      }
    }

    public List<string> GetCommandList(
      UserLevel userLevel)
    {
      string sql = $@"
SELECT {commandField}
FROM {tableName}
WHERE {userLevelField}<=@{userLevelField}
ORDER BY {commandField}
        ";

      using (DbDataReader reader = SqlManager.GetReader(sql, ($"@{userLevelField}", userLevel)))
      {
        List<string> commandList = new List<string>();
        while (reader.Read())
        {
          commandList.Add((string)reader[commandField]);
        }
        return commandList;
      }
    }
    #endregion

    #region Private Write API
    bool CreateOrUpdateCommandByName(
      string commandName,
      string commandResponse,
      UserLevel userLevel)
    {
      string sql = $@"
REPLACE INTO {tableName}({commandField}, {responseField}, {userLevelField})
VALUES(@{commandField}, @{responseField}, @{userLevelField})
        ";

      return SqlManager.ExecuteNonQuery(sql,
        ($"@{commandField}", commandName),
        ($"@{responseField}", commandResponse),
        ($"@{userLevelField}", userLevel));
    }

    bool DeleteCommandByName(
     string commandName)
    {
      CommandAliasesTable.instance.DeleteAllAliasesForCommand(commandName);

      string sql = $@"
DELETE FROM {tableName} 
WHERE {commandField}=@{commandField}
        ";

      return SqlManager.ExecuteNonQuery(sql, ($"@{commandField}", commandName));
    }
    #endregion
  }
}
