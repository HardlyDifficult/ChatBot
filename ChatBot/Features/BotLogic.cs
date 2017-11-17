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
        return TwitchController.instance.GetChannelInfo().Result.title;
      }
      set
      {
        string obsMessage = new string(' ', 30);
        obsMessage += value;
        File.WriteAllText("..\\TODO.txt", obsMessage);

        TwitchController.instance.SetTitle(value);
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
      // TODO dynamicCommandList.Add(new DynamicCommand("!setgame", "!setgame gamedev|coding|game name", UserLevel.Mods, SetGame));

      //dynamicCommandList.Add(new DynamicCommand("!credit", null, UserLevel.Everyone, DisplayCredits));
    }
    #endregion

    #region Events
    static void OnGoLive(
      string goLiveMessage)
    {
      // TODO TwitchController.instance.DownloadFullSubList();

      TwitchController.instance.ExitHost();
      TwitchController.instance.SendMessage("Welcome back!");

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
        TwitchController.instance.SendWhisper(BotSettings.twitch.channelUsername, "Dude, where's the tweet?");
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
        Edu.OnMoney(message.user, message.bits);
      }

      if (message.isWhisper
        && char.IsLetter(message.message[0]))
      {
        message.message = "!" + message.message;
      }

      if (hasSomeoneSaidSomethingSinceGoingLive == false
        && message.user != TwitchController.instance.twitchChannel)
      {
        hasSomeoneSaidSomethingSinceGoingLive = true;
        TwitchController.instance.SendMessage($"hardlyHype");
      }

      onMessage?.Invoke(message);
    }

    public static void OnSub(
      TwitchUser user,
      int tier1To3,
      int months)
    {
      string message = $"hardlyHype {user.displayName}";

      switch (tier1To3)
      {
        default:
        case 1:
          Edu.OnMoney(user, 499);
          message += " just subscribed!";
          break;
        case 2:
          Edu.OnMoney(user, 999);
          message += " double subbed!";
          break;
        case 3:
          Edu.OnMoney(user, 2499);
          message += " threw down a 6x sub!!!";
          break;
      }

      if (months > 1)
      {
        message += $" (for {months})";
      }

      message += " hardlyHeart";

      TwitchController.instance.SendMessage(message);
    }

    public static void OnJoin(
      TwitchUser user)
    {
      onJoin?.Invoke(user);
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
        TwitchController.instance.SetGame("Creative");
        TwitchController.instance.SetCommunities("gamedevelopment", "programming", "chill-streams");
      }
      else if (game.Equals("coding", StringComparison.InvariantCultureIgnoreCase))
      {
        TwitchController.instance.SetGame("Creative");
        TwitchController.instance.SetCommunities("programming", "chill-streams");
      }
      else
      {
        TwitchController.instance.SetGame(game);
        TwitchController.instance.SetCommunities("chill-streams");
      }

      SendModReplyWithTitle(message);
    }

    #region Private Write API
    public static void SendWhisper(
      string userName,
      string message)
    {
      TwitchController.instance.SendWhisper(userName, message);
    }

    public static void SendMessage(
      string message)
    {
      TwitchController.instance.SendMessage(message);
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
      if (shouldWhisper && messageRespondingTo.user.userLevel >= UserLevel.Mods)
      {
        shouldWhisper = false;
      }

      if (shouldWhisper)
      {
        SendWhisper(messageRespondingTo.user.displayName, message);

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

    //    userId = TwitchController.instance.GetUserId(usernameToCredit);
    //    creditMessage = message.message.GetAfter("=");
    //  }
    //  catch { }

    //  if(usernameToCredit == null || userId == null || project == null || category == null || creditMessage == null)
    //  {
    //    TwitchController.instance.SendWhisper(message.displayName,
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

    //  TwitchController.instance.SendMessage(builder.ToString());
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
      TwitchController.instance.PostToPulse(tweet);
      TwitchController.instance.SendWhisper(BotSettings.twitch.channelUsername, $"Tweeted / Pulsed: {tweet}");
    }

    static void SetTitle(
      Message message)
    {
      string title = message.message.GetAfter(" ");
      if (title != null && title.Length > 3)
      {
        TwitchController.instance.SetTitle(title);
      }

      SendModReplyWithTitle(message);
    }

    static async void SendModReplyWithTitle(
      Message message)
    {
      Thread.Sleep(1000);
      (string title, string game) = await TwitchController.instance.GetChannelInfo();
      string[] communityList = await TwitchController.instance.GetCommunity();
      SendModReply(message.user.displayName, $"\"{title}\" {game} / {communityList.ToCsv()}");
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
    //  TwitchController.instance.GetAllFollowers(AutoFollow);
    //}

    //static async void AutoFollow(
    //  string userId)
    //{
    //  return;
    //  if(SqlManager.HasAutoFollowedBefore(userId) == false
    //    && await TwitchController.instance.ShouldAutoFollow(userId))
    //  {
    //    string displayName = TwitchController.instance.GetDisplayName(userId);
    //    SqlManager.StoreUser(userId, displayName, UserLevelHelpers.Get(userId));
    //    TwitchController.instance.AutoFollow(userId, () =>
    //    {
    //      SqlManager.SetHasAutoFollowed(userId);
    //    });
    //  }
    //}

    //static void UnfollowNonStreamers()
    //{
    //  TwitchController.instance.UnfollowFollowedNonStreamers();
    //}
    #endregion

    #region Private Read API

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

    //  string userId = TwitchController.instance.GetUserId(username);
    //  if(userId == null)
    //  {
    //    TwitchController.instance.SendWhisper(message.displayName, $"I don't know who {username} is");
    //    return;
    //  }

    //  List<(string project, string category, string contribution)> contributions = SqlManager.GetContributions(userId);

    //  if(contributions == null || contributions.Count == 0)
    //  {
    //    TwitchController.instance.SendWhisper(message.displayName, $"I don't have any credits recorded for {username}");
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
    //    TwitchController.instance.SendWhisper(message.displayName, builder.ToString());
    //  }
    //  else
    //  {
    //    TwitchController.instance.SendMessage(builder.ToString());
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
        TwitchController.instance.SendWhisper(BotSettings.twitch.channelUsername, $"{displayName} -> {message}");
      }
      TwitchController.instance.SendWhisper(displayName, message);
    }

    static void GetSubCount(
      Message message)
    {
      const int nextMilestone = 200;
      int currentSubCount = SqlManager.GetTotalSubCount();
      int remaining = nextMilestone - currentSubCount;

      TwitchController.instance.SendMessage($"We have {currentSubCount} subs!  Only {remaining} till our next emote.");
    }
    #endregion
  }
}
