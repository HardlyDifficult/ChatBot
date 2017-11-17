using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HD
{
  /// <summary>
  /// All commands are registered with this class.
  /// </summary>
  public class CommandFeatures : IBotFeature
  {
    #region Data
    public static CommandFeatures instance;

    readonly List<DynamicCommand> dynamicCommandList = new List<DynamicCommand>();
    #endregion

    #region Init
    public CommandFeatures()
    {
      Debug.Assert(instance == null);

      instance = this;
    }

    void IBotFeature.Init()
    {
      Add(new DynamicCommand(
        command: "!command",
        helpMessage: "!command !commandName [userLevel:Everyone|Follower|Subscribers|Mods|Owner (default Everyone)] [timeoutInSeconds (default 200)] = commandText",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnUpdateCommand));

      Add(new DynamicCommand(
        command: "!alias",
        helpMessage: "List: !alias !commandName; Create: !alias !commandName !aliasName !additionalAliasName; Delete: !alias delete !aliasName",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnAlias));

      Add(new DynamicCommand(
        command: "!delete",
        helpMessage: "!delete !commandName",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnDeleteCommand));

      Add(new DynamicCommand(
        command: "!help",
        helpMessage: "Hi!",
        minimumUserLevel: UserLevel.Everyone,
        onCommand: OnHelp));

      Add(new DynamicCommand(
        command: "!commands",
        helpMessage: null,
        minimumUserLevel: UserLevel.Everyone,
        onCommand: OnSendCommandList));

      BotLogic.onMessage += OnMessage;
    }
    #endregion

    #region Events
    void OnMessage(
      Message message)
    {
      ProcessDynamicCommands(message);
      ProcessDatabaseCommands(message);
    }
    #endregion

    #region Commands
    /// <summary>
    /// Read, Create, or Delete aliases
    /// </summary>
    void OnAlias(
      Message message)
    {
      string[] tokens = message.message.Split(
        new[] { ' ' },
        StringSplitOptions.RemoveEmptyEntries);

      if (tokens.Length < 2)
      { // Must be at least 2 tokens to do anything
        return;
      }

      if (tokens[1].Equals("delete", StringComparison.InvariantCultureIgnoreCase))
      {
        DeleteAlias(message, aliasesToDelete: tokens, startIndex: 2);
      }
      else if (tokens.Length == 2)
      {
        WhisperCommandAliases(message, tokens[1]);
      }
      else
      {
        CreateAliases(message, tokens[1], tokens, 2);
      }
    }

    void OnDeleteCommand(
      Message message)
    {
      string command = message.message.GetAfter(" ");
      if (command == null)
      {
        return;
      }

      DeleteCommand(message, command);
    }

    void OnHelp(
      Message message)
    {
      if (message.user.userLevel >= UserLevel.Mods)
      {
        SendModHelp(message);
      }
      else
      { // There is no help for users other than the command list
        OnSendCommandList(message);
      }
    }

    void OnSendCommandList(
      Message message)
    {
      SendCommandList(message);
    }

    void OnUpdateCommand(
      Message message)
    {
      if (TryGetNewCommandDetails(message, out string commandName, out string commandText, out UserLevel userLevel, out int timeoutInSeconds))
      {
        CreateOrUpdateCommand(message, commandName, commandText, userLevel, TimeSpan.FromSeconds(timeoutInSeconds));
      }
    }
    #endregion

    #region Write
    /// <summary>
    /// Registers a new command, which will be called by this class when triggered a user.
    /// </summary>
    public void Add(
      DynamicCommand dynamicCommand)
    {
      dynamicCommandList.Add(dynamicCommand);
    }

    public void CreateAliases(
      Message message,
      string existingCommandOrAlias,
      string[] aliasListToCreate,
      int startingIndex)
    {
      for (int i = startingIndex; i < aliasListToCreate.Length; i++)
      {
        string alias = aliasListToCreate[i];
        if (CommandAliasesTable.instance.CreateAlias(existingCommandOrAlias, alias))
        {
          BotLogic.SendModReply(
            message.user.displayName,
            $"Created alias for {existingCommandOrAlias}: {alias}");
        }
      }
    }

    public void CreateOrUpdateCommand(
      Message message,
      string commandName,
      string commandText,
      UserLevel userLevel,
      TimeSpan timeout)
    {
      if (CommandsTable.instance.CreateOrUpdateCommand(commandName, commandText, userLevel))
      {
        // Ensure that the commandName is correct (vs an alias)
        SqlTwitchCommand newCommand = CommandsTable.instance.GetCommand(commandName);
        commandName = newCommand.command;

        CooldownTable.instance.SetCooldown(commandName, timeout);

        TwitchController.instance.SendWhisper(message.user.displayName,
          $"Command: {commandName} {userLevel} {timeout} = {commandText}");
      }
      else
      {
        TwitchController.instance.SendWhisper(message.user.displayName, "Failed to create command..");
      }
    }

    public void DeleteAlias(
      Message message,
      string[] aliasesToDelete,
      int startIndex)
    {
      for (int i = startIndex; i < aliasesToDelete.Length; i++)
      {
        if (CommandsTable.instance.DeleteCommand(aliasesToDelete[i]))
        {
          BotLogic.SendModReply(message.user.displayName, $"Deleted {aliasesToDelete[i]}");
        }
      }
    }

    public void DeleteCommand(
      Message message,
      string commandOrAliasToDelete)
    {
      SqlTwitchCommand command = GetCommand(commandOrAliasToDelete);
      if (command.isValid == false)
      { // Command not found
        return;
      }
      string commandToDelete = command.command;

      if (CommandsTable.instance.DeleteCommand(commandToDelete))
      {
        TwitchController.instance.SendWhisper(message.user.displayName, $"Deleted {commandToDelete}");
      }
      else
      {
        TwitchController.instance.SendWhisper(message.user.displayName, "Failed.. to delete a command, !delete !oldcommand");
      }
    }
    #endregion

    #region Read
    public SqlTwitchCommand GetCommand(
      string commandOrAlias)
    {
      return CommandsTable.instance.GetCommand(commandOrAlias);
    }

    public void SendCommandList(
      Message message)
    {
      string commandList = GetCommandListMessage(message.user.userLevel);
      TwitchController.instance.SendWhisper(message.user.displayName, commandList);
    }

    public static void WhisperCommandAliases(
      Message message,
      string existingCommandOrAlias)
    {
      (string command, List<string> aliasList) = CommandAliasesTable.instance.GetAliases(existingCommandOrAlias);
      if (command == null)
      {
        return;
      }
      StringBuilder response = new StringBuilder();
      response.Append(command);
      response.Append(": ");
      for (int i = 0; i < aliasList.Count; i++)
      {
        if (i > 0)
        {
          response.Append(", ");
        }
        response.Append(aliasList[i]);
      }
      TwitchController.instance.SendWhisper(message.user.displayName, response.ToString());
    }

    public void SendModHelp(
     Message message)
    {
      string command = message.message.GetAfter(" ");
      for (int i = 0; i < dynamicCommandList.Count; i++)
      {
        DynamicCommand dynamicCommand = dynamicCommandList[i];
        if (dynamicCommand.command.Equals(command, StringComparison.InvariantCultureIgnoreCase))
        {
          if (dynamicCommand.helpMessage != null)
          {
            TwitchController.instance.SendWhisper(message.user.displayName, dynamicCommand.helpMessage);
            return;
          }
        }
      }

      StringBuilder builder = new StringBuilder();
      builder.Append("I can tell you more about: ");
      bool first = true;
      for (int i = 0; i < dynamicCommandList.Count; i++)
      {
        DynamicCommand dynamicCommand = dynamicCommandList[i];
        if (dynamicCommand.helpMessage != null)
        {
          if (first == false)
          {
            builder.Append(", ");
          }
          first = false;

          builder.Append(dynamicCommand.command);
        }
      }
      TwitchController.instance.SendWhisper(message.user.displayName, builder.ToString());
    }
    #endregion

    #region Helpers
    void ProcessDynamicCommands(
     Message message)
    {
      for (int i = 0; i < dynamicCommandList.Count; i++)
      {
        DynamicCommand command = dynamicCommandList[i];
        command.OnMessage(message);
      }
    }

    void ProcessDatabaseCommands(
      Message message)
    {
      string firstWord = message.message.GetBefore(" ");
      SqlTwitchCommand command = CommandsTable.instance.GetCommand(firstWord);
      if (command.isValid == false)
      { // Command not found
        return;
      }
      if (message.user.userLevel < command.userLevel)
      { // You don't have permission
        return;
      }

      bool cooldownReady = CooldownTable.instance.IsReady(command.command);
      string response = SwapInVariables(command.response);
      if (BotLogic.SendMessageOrWhisper(message, response, cooldownReady))
      {
        CooldownTable.instance.SetTime(command.command);
      }
    }

    string GetCommandListMessage(
        UserLevel userLevel)
    {
      StringBuilder builder = new StringBuilder();

      List<string> commandList = CommandsTable.instance.GetCommandList(userLevel);
      if (commandList != null)
      {
        for (int i = 0; i < commandList.Count; i++)
        {
          builder.Append(commandList[i]);
          builder.Append(", ");
        }
      }

      for (int i = 0; i < dynamicCommandList.Count; i++)
      {
        DynamicCommand command = dynamicCommandList[i];
        if (userLevel >= command.minimumUserLevel)
        {
          builder.Append(command.command);
          builder.Append(", ");
        }
      }

      builder.Remove(builder.Length - 2, 2); // Remove last comma

      return builder.ToString();
    }

    string SwapInVariables(
      string message)
    {
      int index = message.IndexOf("{edu}", StringComparison.CurrentCultureIgnoreCase);
      if (index < 0)
      {
        return message;
      }

      KeyStringValueTable.instance.TryGetValue("EDU", out string eduMessage);
      message = message.Substring(0, index) + eduMessage + message.Substring(index + 5);

      return message;
    }

    bool TryGetNewCommandDetails(
      Message message,
      out string commandName,
      out string commandText,
      out UserLevel userLevel,
      out int timeoutInSeconds)
    {
      string commandOptions = null;

      commandName = message.message.GetBetween(" ", " ");
      if (commandName != null)
      {
        commandOptions = message.message.GetBetween(commandName, "=");
      }
      commandText = message.message.GetAfter("=");
      userLevel = UserLevel.Everyone;
      timeoutInSeconds = 200;

      if (commandOptions != null)
      {
        string[] commandOptionList = commandOptions.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < commandOptionList.Length; i++)
        {
          if (int.TryParse(commandOptionList[i], out int newTimeout))
          {
            timeoutInSeconds = newTimeout;
          }
          else if (Enum.TryParse<UserLevel>(commandOptionList[i], out UserLevel newUserLevel))
          {
            userLevel = newUserLevel;
          }
          else
          {
            commandOptions = null;
            break;
          }
        }
      }

      if (commandName == null || commandOptions == null || commandText == null)
      {
        TwitchController.instance.SendWhisper(message.user.displayName, "Create new commands like so: !commands !newcommand Mods 120 = Command text. ...To Delete, !delete !oldcommand");
        return false;
      }

      return true;
    }
    #endregion
  }
}
