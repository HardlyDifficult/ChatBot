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

    public static event Action<Message> onMessage;

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

      TimeFeatures.instance.onGoLive += OnGoLive;

      // TODO broken dynamicCommandList.Add(new DynamicCommand("!subcount", null, UserLevel.Everyone, GetSubCount));

      CommandFeatures.instance.Add(new DynamicCommand("!edu", "TODO", UserLevel.Everyone, Edu.OnCommand));

      //dynamicCommandList.Add(new DynamicCommand("!edu", "!edu Message", UserLevel.Mods, UpdateEdu));
      //dynamicCommandList.Add(new DynamicCommand("!credit", null, UserLevel.Mods, RecordCredits));
      CommandFeatures.instance.Add(new DynamicCommand("!tweet", "!tweet message for Pulse and Twitter (if not too long)", UserLevel.Mods, SendTweetAndPulse));
      CommandFeatures.instance.Add(new DynamicCommand("!title", "!title New Title", UserLevel.Mods, SetTitle));
      CommandFeatures.instance.Add(new DynamicCommand("!shoutout", "Give shoutout: !shoutout @Username; Create shoutout: !shoutout @Username = New shoutout message", UserLevel.Mods, Shoutout));
      // TODO dynamicCommandList.Add(new DynamicCommand("!setgame", "!setgame gamedev|coding|game name", UserLevel.Mods, SetGame));

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

      const string key = "Twitter";

      if (goLiveMessage != null && goLiveMessage.Length > 3)
      {
        if (CooldownTable.instance.IsReady(key))
        {
          SendTweetAndPulse($"Live now! {goLiveMessage}", isForLiveThread: true);
          CooldownTable.instance.SetTime(key);
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

      onMessage?.Invoke(message);
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

    /// <summary>
    /// This will whisper the streamer as well to keep them informed.
    /// </summary>
    public static void SendModReply(
      string displayName,
      string message)
    {
      if (displayName.Equals(BotSettings.twitch.channelUsername, StringComparison.InvariantCultureIgnoreCase) == false)
      {
        TwitchController.SendWhisper(BotSettings.twitch.channelUsername, $"{displayName} -> {message}");
      }
      TwitchController.SendWhisper(displayName, message);
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
