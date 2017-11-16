using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace HD
{
  public static class BotLogic
  {
    #region Data
    public static event Action<TwitchUser> onJoin;

    public static readonly List<DynamicCommand> dynamicCommandList = new List<DynamicCommand>();

    static bool hasSomeoneSaidSomethingSinceGoingLive;
    #endregion

    #region Properties
    public static string streamTitle
    {
      get
      {
        // TODO this may deadlock
        return TwitchController.GetChannelInfo().Result.title;
      }
      set
      {
        string obsMessage = new string(' ', 30);
        obsMessage += value;
        File.WriteAllText("..\\TODO.txt", obsMessage);

        TwitchController.SetTitle(value);
      }
    }
    #endregion

    #region Init
    static BotLogic()
    {
      SchemaTable.UpdateTables();
      List<IBotFeature> featureList = ReflectionHelpers.CreateOneOfEach<IBotFeature>();
      for (int i = 0; i < featureList.Count; i++)
      {
        featureList[i].Init();
      }






      ETA.onGoLive += OnGoLive;



      // TODO broken dynamicCommandList.Add(new DynamicCommand("!subcount", null, UserLevel.Everyone, GetSubCount));

      dynamicCommandList.Add(new DynamicCommand("!edu", "TODO", UserLevel.Everyone, Edu.OnCommand));
      dynamicCommandList.Add(new DynamicCommand("!command", "!command !commandName [userLevel:Everyone|Follower|Subscribers|Mods|Owner (default Everyone)] [timeoutInSeconds (default 200)] = commandText", UserLevel.Mods, UpdateCommand));
      dynamicCommandList.Add(new DynamicCommand("!alias", "List: !alias !commandName; Create: !alias !commandName !aliasName !additionalAliasName; Delete: !alias delete !aliasName", UserLevel.Mods, CreateAlias));
      dynamicCommandList.Add(new DynamicCommand("!delete", "!delete !commandName", UserLevel.Mods, DeleteCommand));
      //dynamicCommandList.Add(new DynamicCommand("!edu", "!edu Message", UserLevel.Mods, UpdateEdu));
      //dynamicCommandList.Add(new DynamicCommand("!credit", null, UserLevel.Mods, RecordCredits));
      dynamicCommandList.Add(new DynamicCommand("!tweet", "!tweet message for Pulse and Twitter (if not too long)", UserLevel.Mods, SendTweetAndPulse));
      dynamicCommandList.Add(new DynamicCommand("!title", "!title New Title", UserLevel.Mods, SetTitle));
      dynamicCommandList.Add(new DynamicCommand("!shoutout", "Give shoutout: !shoutout @Username; Create shoutout: !shoutout @Username = New shoutout message", UserLevel.Mods, Shoutout));
      // TODO dynamicCommandList.Add(new DynamicCommand("!setgame", "!setgame gamedev|coding|game name", UserLevel.Mods, SetGame));
      dynamicCommandList.Add(new DynamicCommand("!help", "Hi!", UserLevel.Everyone, Help));

      dynamicCommandList.Add(new DynamicCommand("!commands", null, UserLevel.Everyone, SendCommandList));
      //dynamicCommandList.Add(new DynamicCommand("!credit", null, UserLevel.Everyone, DisplayCredits));
    }
    #endregion

    #region Events
    static void OnGoLive(
      string goLiveMessage)
    {
      //UnfollowNonStreamers();
      TwitchController.instance.DownloadFullSubList();
      //FollowBot();

      TwitchController.ExitHost();
      TwitchController.SendMessage("Welcome back!");

      hasSomeoneSaidSomethingSinceGoingLive = false;

      if (goLiveMessage != null && goLiveMessage.Length > 3)
      {
        if (SqlManager.GetIsReady("Twitter"))
        {
          SqlManager.SetLastSentForKey("Twitter");
          SendTweetAndPulse($"Live now! {goLiveMessage}", isForLiveThread: true);
        }
      }
      else
      {
        TwitchController.SendWhisper(BotSettings.twitch.channelUsername, "Dude, where's the tweet?");
      }
    }

    public static void OnMessage(
      Message message)
    {
      if (string.IsNullOrWhiteSpace(message.message))
      {
        return;
      }

      if (message.bits > 0)
      {
        Edu.OnMoney(message, message.bits);
      }

      if (message.isWhisper
        && char.IsLetter(message.message[0]))
      {
        message.message = "!" + message.message;
      }

      if (hasSomeoneSaidSomethingSinceGoingLive == false
        && message.userId != TwitchController.twitchChannelId)
      {
        hasSomeoneSaidSomethingSinceGoingLive = true;
        TwitchController.SendMessage($"hardlyHype");
      }

      ProcessDynamicCommands(message);
      ProcessDatabaseCommands(message);
    }

    public static void OnSub(
      string userId,
      string displayName,
      int tier1To3,
      int months)
    {
      Message m = new Message(userId, displayName, UserLevelHelpers.Get(userId), null, false, 0);
      string message = $"hardlyHype {displayName}";

      switch (tier1To3)
      {
        default:
        case 1:
          Edu.OnMoney(m, 499);
          message += " just subscribed!";
          break;
        case 2:
          Edu.OnMoney(m, 999);
          message += " double subbed!";
          break;
        case 3:
          Edu.OnMoney(m, 2499);
          message += " threw down a 6x sub!!!";
          break;
      }

      if (months > 1)
      {
        message += $" (for {months})";
      }

      message += " hardlyHeart";

      TwitchController.SendMessage(message);
    }

    public static void OnHost(
      string displayName,
      int? viewerCount,
      bool isAutoHost)
    {
      // TODO put this back AutoFollow(TwitchController.GetUserId(displayName));
      if (isAutoHost)
      {
        return;
      }
      if (viewerCount < 10)
      { // Hide counts less than 10
        viewerCount = null;
      }
      (string hosterName, string shoutoutMessage) = GetShoutoutMessage(displayName);
      if (string.IsNullOrWhiteSpace(hosterName))
      { // I dunno who you are...
        return;
      }

      StringBuilder message = new StringBuilder();
      message.Append("Thanks for the host");
      if (viewerCount != null)
      {
        message.Append(" for ");
        message.Append(viewerCount.Value);
        message.Append(" viewers! ");
      }
      else
      {
        message.Append(". ");
      }
      if (shoutoutMessage != null)
      {
        message.Append(shoutoutMessage);
      }
      message.Append(" twitch.tv/");
      message.Append(hosterName);
      message.Append(" hardlyHype hardlyHype");
      TwitchController.SendMessage(message.ToString());
    }

    public static void OnJoin(
      string username)
    {
      TwitchUser user = new TwitchUser(username);
      onJoin?.Invoke(user);
    }

    public static void OnHostingAnotherChannel(
      string channelName)
    {
      (string streamerName, string shoutoutMessage) = GetShoutoutMessage(channelName);
      StringBuilder builder = new StringBuilder();
      builder.Append("Now hosting ");
      builder.Append(streamerName);
      builder.Append(". ");
      if (shoutoutMessage != null)
      {
        builder.Append(shoutoutMessage);
        builder.Append(" ");
      }
      builder.Append("twitch.tv/");
      builder.Append(streamerName);
      TwitchController.SendMessage(builder.ToString());
    }
    #endregion

    public static void Add(
      DynamicCommand dynamicCommand)
    {
      dynamicCommandList.Add(dynamicCommand);
    }

    //public static void OnFollow(
    //  string userId)
    //{
    //  AutoFollow(userId);
    //}

    static void ProcessDynamicCommands(
     Message message)
    {
      for (int i = 0; i < dynamicCommandList.Count; i++)
      {
        DynamicCommand command = dynamicCommandList[i];
        command.OnMessage(message);
      }
    }

    static void ProcessDatabaseCommands(
      Message message)
    {
      string firstWord = message.message.GetBefore(" ");
      SqlTwitchCommand command = SqlManager.GetCommand(firstWord);
      if (command.command != null)
      {
        if (message.userLevel < command.userLevel)
        {
          return;
        }

        bool cooldownReady = SqlManager.CooldownIsReady(message.userLevel, command);
        string response = SwapInVariables(command.response);
        if (SendMessageOrWhisper(message, response, cooldownReady))
        {
          SqlManager.SetLastSentForCommand(command.command);
        }
      }
    }

    // TODO add a help message for SetGame
    /// <summary>
    /// gamedev = Creative/gamedevelopment
    /// coding = Creative/programming
    /// Game Name = Game Name / chill-streams
    /// </summary>
    static void SetGame(
      Message message)
    {
      string game = message.message.GetAfter(" ");
      if (string.IsNullOrWhiteSpace(game))
      {
        return;
      }
      if (game.Equals("gamedev", StringComparison.InvariantCultureIgnoreCase))
      {
        TwitchController.SetGame("Creative");
        TwitchController.SetCommunities("gamedevelopment", "programming", "chill-streams");
      }
      else if (game.Equals("coding", StringComparison.InvariantCultureIgnoreCase))
      {
        TwitchController.SetGame("Creative");
        TwitchController.SetCommunities("programming", "chill-streams");
      }
      else
      {
        TwitchController.SetGame(game);
        TwitchController.SetCommunities("chill-streams");
      }

      SendModReplyWithTitle(message);
    }

    static void Help(
      Message message)
    {
      if (UserLevelHelpers.Get(message.userId) >= UserLevel.Mods)
      {

        string command = message.message.GetAfter(" ");
        for (int i = 0; i < dynamicCommandList.Count; i++)
        {
          DynamicCommand dynamicCommand = dynamicCommandList[i];
          if (dynamicCommand.command.Equals(command, StringComparison.InvariantCultureIgnoreCase))
          {
            if (dynamicCommand.helpMessage != null)
            {
              TwitchController.SendWhisper(message.displayName, dynamicCommand.helpMessage);
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
        TwitchController.SendWhisper(message.displayName, builder.ToString());
      }
      else
      {
        SendCommandList(message);
      }
    }

    #region Private Write API
    public static void SendWhisper(
      string userName,
      string message)
    {
      TwitchController.SendWhisper(userName, message);
    }

    public static void SendMessage(
      string message)
    {
      TwitchController.SendMessage(message);
    }

    /// <summary>
    /// Returns true if the message was sent to chat (vs whisper).
    /// </summary>
    public static bool SendMessageOrWhisper(
      Message messageRespondingTo,
      string message,
      bool cooldownReady)
    {
      bool shouldWhisper = messageRespondingTo?.isWhisper ?? false;
      if (cooldownReady == false)
      {
        if (messageRespondingTo == null)
        {
          return false;
        }
        shouldWhisper = true;
      }
      if (shouldWhisper && messageRespondingTo.userLevel >= UserLevel.Mods)
      {
        shouldWhisper = false;
      }

      if (shouldWhisper)
      {
        SendWhisper(messageRespondingTo.displayName, message);

        return false;
      }
      else
      {
        SendMessage(message);

        return true;
      }
    }

    //static void RecordCredits(
    //  Message message)
    //{
    //  // TODO credits
    //  string usernameToCredit = null;
    //  Project? project = null;
    //  Category? category = null;
    //  string userId = null;
    //  string creditMessage = null;

    //  try
    //  {
    //    if(message.message.Contains("=") == false)
    //    { // Assume it's display credits instead
    //      return;
    //    }

    //    string[] tokens = message.message.GetBetween(" ", "=")?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    //    for(int i = 0; i < tokens.Length; i++)
    //    {
    //      if(project == null && Enum.TryParse<Project>(tokens[i], true, out Project selectedProject))
    //      {
    //        project = selectedProject;
    //      }
    //      else if(category == null && Enum.TryParse<Category>(tokens[i], true, out Category selectedCategory))
    //      {
    //        category = selectedCategory;
    //      }
    //      else if(usernameToCredit == null)
    //      {
    //        usernameToCredit = tokens[i].ToLower();
    //        if(usernameToCredit.StartsWith("@"))
    //        {
    //          usernameToCredit = usernameToCredit.Substring(1);
    //        }
    //      }
    //      else
    //      { // Got something we didn't expect, abort!
    //        usernameToCredit = null;
    //        break;
    //      }
    //    }

    //    userId = TwitchController.GetUserId(usernameToCredit);
    //    creditMessage = message.message.GetAfter("=");
    //  }
    //  catch { }

    //  if(usernameToCredit == null || userId == null || project == null || category == null || creditMessage == null)
    //  {
    //    TwitchController.SendWhisper(message.displayName,
    //      $"Fail. Expecting !credit @user pizza art = Very short summary. {Enum.GetNames(typeof(Project)).ToCsv()} / {Enum.GetNames(typeof(Category)).ToCsv()}");
    //    return;
    //  }

    //  SqlManager.AddCredits(userId, message.userId, project.Value.ToString(), category.Value.ToString(), creditMessage);

    //  (int projectContributions, int totalContributions) = SqlManager.GetCreditsCount(userId, project.Value.ToString());

    //  StringBuilder builder = new StringBuilder();
    //  builder.Append("Yay, ");
    //  builder.Append(usernameToCredit);
    //  builder.Append("!");

    //  if(totalContributions > 1)
    //  {
    //    builder.Append(" That's ");
    //    builder.Append(projectContributions);
    //    builder.Append(" towards ");
    //    builder.Append(project.Value);
    //    if(totalContributions > projectContributions)
    //    {
    //      builder.Append(" (");
    //      builder.Append(totalContributions);
    //      builder.Append(" total)");
    //    }
    //    builder.Append(".");
    //  }

    //  builder.Append(" Thanks for everything!");

    //  TwitchController.SendMessage(builder.ToString());
    //}

    static void SendTweetAndPulse(
      Message message)
    {
      string tweet = message.message.GetAfter(" ");
      if (string.IsNullOrWhiteSpace(tweet))
      {
        return;
      }
      tweet.Trim();
      if (tweet.StartsWith("="))
      {
        tweet = tweet.Substring(1).Trim();
      }

      SendTweetAndPulse(tweet, isForLiveThread: false);
    }

    static void SendTweetAndPulse(
      string tweet,
      bool isForLiveThread)
    {
      if (tweet == null || tweet.Length < 3)
      {
        return;
      }

      TwitterController.SendTweet($"{tweet} twitch.tv/HardlyDifficult", isForLiveThread);
      TwitchController.PostToPulse(tweet);
      TwitchController.SendWhisper(BotSettings.twitch.channelUsername, $"Tweeted / Pulsed: {tweet}");
    }

    static void SetTitle(
      Message message)
    {
      string title = message.message.GetAfter(" ");
      if (title != null && title.Length > 3)
      {
        TwitchController.SetTitle(title);
      }

      SendModReplyWithTitle(message);
    }

    static async void SendModReplyWithTitle(
      Message message)
    {
      Thread.Sleep(1000);
      (string title, string game) = await TwitchController.GetChannelInfo();
      string[] communityList = await TwitchController.GetCommunity();
      SendModReply(message.displayName, $"\"{title}\" {game} / {communityList.ToCsv()}");
    }

    static void DeleteCommand(
      Message message)
    {
      string command = message.message.GetAfter(" ");
      if (command == null)
      {
        return;
      }

      if (SqlManager.DeleteCommand(command))
      {
        TwitchController.SendWhisper(message.displayName, $"Deleted {command}");
      }
      else
      {
        TwitchController.SendWhisper(message.displayName, "Failed.. to delete a command, !delete !oldcommand");
      }
    }

    static void UpdateCommand(
      Message message)
    {
      if (TryGetNewCommandDetails(message, out string commandName, out string commandText, out UserLevel userLevel, out int timeoutInSeconds))
      {
        CreateOrUpdateResult result = SqlManager.CreateOrUpdateCommand(commandName, commandText, userLevel, timeoutInSeconds);

        switch (result)
        {
          default:
          case CreateOrUpdateResult.Fail:
            TwitchController.SendWhisper(message.displayName, "Failed to create command..");
            break;
          case CreateOrUpdateResult.Created:
          case CreateOrUpdateResult.Updated:
            TwitchController.SendWhisper(message.displayName, $"Command {result}: {commandName} {userLevel} {timeoutInSeconds} = {commandText}");
            break;
        }
      }
    }

    static void UpdateEdu(
      Message message)
    {
      string eduText = message.message.GetAfter(" ");
      if (string.IsNullOrWhiteSpace(eduText))
      {
        return;
      }
      eduText.Trim();
      if (eduText.StartsWith("="))
      {
        eduText = eduText.Substring(0);
        eduText = eduText.Trim();
      }

      SqlManager.SetStringValue("EDU", eduText);
    }

    static void CreateAlias(
      Message message)
    {

      if (message.message.StartsWith("!alias", StringComparison.InvariantCultureIgnoreCase) == false)
      {
        return;
      }

      string[] tokens = message.message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      if (tokens.Length < 2)
      { // Incomplete
        return;
      }

      if (tokens[1].Equals("delete", StringComparison.InvariantCultureIgnoreCase))
      { // Delete
        for (int i = 2; i < tokens.Length; i++)
        {
          if (SqlManager.DeleteCommand(tokens[i]))
          {
            SendModReply(message.displayName, $"Deleted {tokens[i]}");
          }
        }
      }
      else if (tokens.Length == 2)
      { // Display
        (string command, List<string> aliasList) = SqlManager.GetAliases(tokens[1]);
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
        TwitchController.SendWhisper(message.displayName, response.ToString());
      }
      else
      { // Create
        string command = tokens[1];
        for (int i = 2; i < tokens.Length; i++)
        {
          string alias = tokens[i];
          if (SqlManager.CreateAlias(command, alias))
          {
            SendModReply(message.displayName, $"Created alias for {command}: {alias}");
          }
        }
      }
    }

    static string SwapInVariables(
      string message)
    {
      int index = message.IndexOf("{edu}", StringComparison.CurrentCultureIgnoreCase);
      if (index < 0)
      {
        return message;
      }

      string eduMessage = SqlManager.GetStringValue("EDU");
      if (eduMessage == null)
      {
        eduMessage = "";
      }
      message = message.Substring(0, index) + eduMessage + message.Substring(index + 5);
      return message;
    }

    //static void FollowBot()
    //{
    //  TwitchController.GetAllFollowers(AutoFollow);
    //}

    //static async void AutoFollow(
    //  string userId)
    //{
    //  return;
    //  if(SqlManager.HasAutoFollowedBefore(userId) == false
    //    && await TwitchController.ShouldAutoFollow(userId))
    //  {
    //    string displayName = TwitchController.GetDisplayName(userId);
    //    SqlManager.StoreUser(userId, displayName, UserLevelHelpers.Get(userId));
    //    TwitchController.AutoFollow(userId, () =>
    //    {
    //      SqlManager.SetHasAutoFollowed(userId);
    //    });
    //  }
    //}

    //static void UnfollowNonStreamers()
    //{
    //  TwitchController.UnfollowFollowedNonStreamers();
    //}
    #endregion

    #region Private Read API
    static void Shoutout(
      Message message)
    {
      string usernameToShout = message.message.GetAfter(" ");
      if (usernameToShout == null)
      {
        return;
      }
      string newShoutoutMessage = usernameToShout.GetAfter("=");
      if (string.IsNullOrWhiteSpace(newShoutoutMessage))
      {
        newShoutoutMessage = null;
      }
      else
      {
        newShoutoutMessage = newShoutoutMessage.Trim();
        usernameToShout = usernameToShout.GetBefore("=");
      }

      usernameToShout = usernameToShout.Trim();
      if (usernameToShout.StartsWith("@")
        && usernameToShout.Length > 1)
      {
        usernameToShout = usernameToShout.Substring(1);
      }
      if (string.IsNullOrEmpty(usernameToShout))
      {
        return;
      }

      if (newShoutoutMessage != null)
      {
        (string streamerName, string streamerId) = TwitchController.GetUserInfo(usernameToShout);
        if (streamerId != null)
        {
          SqlManager.SetShoutoutMessage(streamerId, newShoutoutMessage);
        }
      }
      {
        (string streamerName, string shoutoutMessage) = GetShoutoutMessage(usernameToShout);
        if (streamerName == null)
        { // I don't know who you are
          return;
        }
        if (shoutoutMessage == null)
        {
          shoutoutMessage = "Known streamer -> ";
        }
        TwitchController.SendMessage($"{shoutoutMessage} twitch.tv/{streamerName}");
      }
    }

    private static (string streamerName, string shoutoutMessage) GetShoutoutMessage(
      string usernameToShout)
    {
      (string streamerName, string streamerId) = TwitchController.GetUserInfo(usernameToShout);
      if (streamerId == null)
      {
        return (streamerName, null);
      }
      string shoutoutMessage = SqlManager.GetShoutoutMessage(streamerId);
      if (shoutoutMessage == null)
      {
        return (streamerName, null);
      }

      return (streamerName, shoutoutMessage);
    }


    //static void DisplayCredits(
    //  Message message)
    //{
    //  if(message.message.Contains("="))
    //  { // Assume modifying credits
    //    return;
    //  }

    //  string username = message.message.GetAfter(" ");
    //  if(username != null && username.StartsWith("@"))
    //  {
    //    username = username.Substring(1);
    //  }

    //  string userId = TwitchController.GetUserId(username);
    //  if(userId == null)
    //  {
    //    TwitchController.SendWhisper(message.displayName, $"I don't know who {username} is");
    //    return;
    //  }

    //  List<(string project, string category, string contribution)> contributions = SqlManager.GetContributions(userId);

    //  if(contributions == null || contributions.Count == 0)
    //  {
    //    TwitchController.SendWhisper(message.displayName, $"I don't have any credits recorded for {username}");
    //    return;
    //  }

    //  StringBuilder builder = new StringBuilder();
    //  string lastProject = null;
    //  int countSinceLastProject = 0;

    //  builder.Append(username);
    //  builder.Append(" contributions: ");

    //  for(int i = 0; i < contributions.Count; i++)
    //  {
    //    if(contributions[i].project != lastProject)
    //    {
    //      countSinceLastProject = 0;
    //      if(lastProject != null)
    //      {
    //        builder.Append(". ");
    //      }
    //      builder.Append(contributions[i].project);
    //      builder.Append(": ");
    //      lastProject = contributions[i].project;
    //    }

    //    if(countSinceLastProject > 0)
    //    {
    //      builder.Append(",");
    //    }
    //    builder.Append(" ");
    //    builder.Append(contributions[i].contribution);

    //    countSinceLastProject++;
    //  }

    //  if(message.userLevel < UserLevel.Mods || message.isWhisper)
    //  {
    //    TwitchController.SendWhisper(message.displayName, builder.ToString());
    //  }
    //  else
    //  {
    //    TwitchController.SendMessage(builder.ToString());
    //  }
    //}

    static void SendCommandList(
      Message message)
    {
      if (message.isWhisper)
      {
        string commandList = GetCommandListMessage(message.userLevel);
        TwitchController.SendWhisper(message.displayName, commandList);
      }
      else
      {
        const string key = "WhisperRequired";
        (string dataValue, bool isCooldownReady) = SqlManager.GetValueIfReady(message.userLevel, key);
        if (isCooldownReady && dataValue != null)
        {
          SqlManager.SetLastSentForKey(key);

          TwitchController.SendWhisper(message.displayName, GetCommandListMessage(message.userLevel));
          TwitchController.SendMessage(dataValue);
        }
      }
    }

    static string GetCommandListMessage(
        UserLevel userLevel)
    {
      StringBuilder builder = new StringBuilder();

      List<string> commandList = SqlManager.GetCommandList(userLevel);
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

    static void SendModReply(
      string displayName,
      string message)
    {
      if (displayName.Equals(BotSettings.twitch.channelUsername, StringComparison.InvariantCultureIgnoreCase) == false)
      {
        TwitchController.SendWhisper(BotSettings.twitch.channelUsername, $"{displayName} -> {message}");
      }
      TwitchController.SendWhisper(displayName, message);
    }

    static bool TryGetNewCommandDetails(
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
        TwitchController.SendWhisper(message.displayName, "Create new commands like so: !commands !newcommand Mods 120 = Command text. ...To Delete, !delete !oldcommand");
        return false;
      }

      return true;
    }

    static void GetSubCount(
      Message message)
    {
      const int nextMilestone = 200;
      int currentSubCount = SqlManager.GetTotalSubCount();
      int remaining = nextMilestone - currentSubCount;

      TwitchController.SendMessage($"We have {currentSubCount} subs!  Only {remaining} till our next emote.");
    }
    #endregion
  }
}
