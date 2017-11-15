using System;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using TwitchLib.Models.API.v5.Subscriptions;
using System.Diagnostics;
using TwitchLib.Models.API.v5.Channels;
using TwitchLib.Models.API.v5.Users;
using TwitchLib.Models.API.v5.Communities;
using System.Threading;
using TwitchLib.Interfaces;

namespace HD
{
  public class TwitchController
  {
    #region Data
    public static readonly TwitchController instance = new TwitchController();

    public static string twitchChannelId;

    public static readonly TwitchAPI twitchApi
      = new TwitchAPI(BotSettings.twitch.clientId, BotSettings.twitch.channelOauth);

    TwitchConnection botClient, broadcasterClient;
    readonly Throttle followThrottle = new Throttle(TimeSpan.FromSeconds(2));
    //TwitchFollows twitchFollows;
    #endregion

    #region Properties
    static bool isStreamLive
    {
      get
      {
        try
        {
          return twitchApi.Streams.v5.BroadcasterOnlineAsync(BotSettings.twitch.channelUsername).Result;
        }
        catch { }
        return false;
      }
    }

    public static TimeSpan? streamUptime
    {
      get
      {
        return twitchApi.Streams.v5.GetUptimeAsync(twitchChannelId.ToString()).Result;
      }
    }
    #endregion

    #region Init
    public static void Start()
    {
      Thread thread = new Thread(instance.Reconnect);
      thread.Start();

      string channelName = BotSettings.twitch.channelUsername.ToLower();
      var t = twitchApi.Users.v5.GetUserByNameAsync(channelName);
      var t2 = t.Result;
      var t3 = t2.Matches;
      var t4 = t3[0];
      var t5 = t4.Id;
      twitchChannelId = t5;
    }

    public static void Stop()
    {
      instance.Disconnect();
    }

    void Reconnect()
    {
      try
      {
        botClient?.Disconnect();
        broadcasterClient?.Disconnect();
        //twitchFollows?.Disconnect();
      }
      catch { }

      botClient = new TwitchConnection(BotSettings.twitch.botUsername, BotSettings.twitch.botOauth);

      botClient.OnMessageReceived += OnMessageReceived;
      botClient.OnUserJoined += Client_OnUserJoined;
      botClient.OnWhisperReceived += Client_OnWhisperReceived;
      botClient.OnNewSubscriber += Client_OnNewSubscriber;
      botClient.OnReSubscriber += Client_OnReSubscriber;

      broadcasterClient = new TwitchConnection(BotSettings.twitch.channelUsername, BotSettings.twitch.channelOauth);
      broadcasterClient.OnSendReceiveData += Client_OnSendReceiveData;
      broadcasterClient.OnHostingStarted += BroadcasterClient_OnHostingStarted;

    }

    void Client_OnLog(object sender, OnLogArgs e)
    {
      string a = e.Data;
    }

    void Disconnect()
    {
      botClient?.Disconnect();
    }
    #endregion

    #region Events
    static readonly string jtvMessagePrefix = $":jtv!jtv@jtv.tmi.twitch.tv PRIVMSG {BotSettings.twitch.channelUsername} :";

    void BroadcasterClient_OnHostingStarted(
      object sender,
      OnHostingStartedArgs e)
    {
      BotLogic.OnHostingAnotherChannel(e.TargetChannel);
    }

    void Client_OnSendReceiveData(
      object sender,
      OnSendReceiveDataArgs e)
    {
      // ":jtv!jtv@jtv.tmi.twitch.tv PRIVMSG hardlydifficult :Matthew4898 is now hosting you."
      // teamTALIMA is now auto hosting you for up to 3 viewers.
      if (e.Data.StartsWith(jtvMessagePrefix, StringComparison.InvariantCultureIgnoreCase) == false)
      {
        return;
      }
      // This is a message from JTV
      string message = e.Data.Substring(jtvMessagePrefix.Length);
      string displayNameHostingMe = message.GetBefore(" ");
      message = message.Substring(displayNameHostingMe.Length);
      if (string.IsNullOrWhiteSpace(displayNameHostingMe)
        || string.IsNullOrWhiteSpace(message))
      {
        return;
      }
      bool isHost = message.Contains("host");
      if (isHost == false)
      {
        return;
      }
      bool isAutoHost = message.Contains("auto hosting");
      int? count = null;
      string viewerCountMessage = message.GetBetween("up to ", " viewers");
      if (int.TryParse(viewerCountMessage, out int viewerCount))
      {
        count = viewerCount;
      }

      BotLogic.OnHost(displayNameHostingMe, count, isAutoHost);
    }

    internal static void SendWhisper(object channelUsername, string v)
    {
      throw new NotImplementedException();
    }

    void Client_OnUserJoined(
      object sender,
      OnUserJoinedArgs e)
    {
      BotLogic.OnJoin(e.Username);
    }

    void OnMessageReceived(
      object sender,
      OnMessageReceivedArgs e)
    {
      BotLogic.OnMessage(new Message(e.ChatMessage));
    }

    void Client_OnWhisperReceived(
      object sender,
      OnWhisperReceivedArgs e)
    {
      BotLogic.OnMessage(new Message(e.WhisperMessage));
    }

    void Client_OnReSubscriber(
      object sender,
      OnReSubscriberArgs e)
    {
      int tier1To3;
      switch (e.ReSubscriber.SubscriptionPlan)
      {
        default:
        case TwitchLib.Enums.SubscriptionPlan.NotSet:
        case TwitchLib.Enums.SubscriptionPlan.Prime:
        case TwitchLib.Enums.SubscriptionPlan.Tier1:
          tier1To3 = 1;
          break;
        case TwitchLib.Enums.SubscriptionPlan.Tier2:
          tier1To3 = 2;
          break;
        case TwitchLib.Enums.SubscriptionPlan.Tier3:
          tier1To3 = 3;
          break;
      }

      SqlManager.RecordSub(e.ReSubscriber.UserId.ToString(), tier1To3);

      BotLogic.OnSub(e.ReSubscriber.UserId.ToString(), e.ReSubscriber.DisplayName, tier1To3, e.ReSubscriber.Months);
    }

    void Client_OnNewSubscriber(
      object sender,
      OnNewSubscriberArgs e)
    {
      int tier1To3;
      switch (e.Subscriber.SubscriptionPlan)
      {
        default:
        case TwitchLib.Enums.SubscriptionPlan.NotSet:
        case TwitchLib.Enums.SubscriptionPlan.Prime:
        case TwitchLib.Enums.SubscriptionPlan.Tier1:
          tier1To3 = 1;
          break;
        case TwitchLib.Enums.SubscriptionPlan.Tier2:
          tier1To3 = 2;
          break;
        case TwitchLib.Enums.SubscriptionPlan.Tier3:
          tier1To3 = 3;
          break;
      }

      SqlManager.RecordSub(e.Subscriber.UserId.ToString(), tier1To3);

      BotLogic.OnSub(e.Subscriber.UserId.ToString(), e.Subscriber.DisplayName, tier1To3, 1);
    }

    //void TwitchFollows_OnFollow(
    //  string userId)
    //{
    //  BotLogic.OnFollow(userId);
    //}
    #endregion

    #region Write API
    //public static async void AutoFollow(
    //  string userId,
    //  Action onSuccess)
    //{
    //  try
    //  {
    //    instance.followThrottle.SleepIfNeeded();
    //    UserFollow follow = await twitchApi.Users.v5.FollowChannel(twitchChannelId, userId, authToken: BotSettings.twitch.channelUsernameOauth);
    //    if (follow.Channel != null)
    //    {
    //      await DebugPrintChannel("Following", follow.Channel);
    //      onSuccess();
    //    }
    //  }
    //  catch { }
    //}

    public static async void SetGame(
      string gameName)
    {
      await twitchApi.Channels.v5.UpdateChannelAsync(twitchChannelId, game: gameName);
    }

    public static async void SetCommunities(
      params string[] communityNameList)
    {
      List<string> communityIdList = new List<string>();
      for (int i = 0; i < communityNameList.Length; i++)
      {
        Community community = await twitchApi.Communities.v5.GetCommunityByNameAsync(communityNameList[i]);
        communityIdList.Add(community.Id);
      }
      await twitchApi.Channels.v5.SetChannelCommunitiesAsync(twitchChannelId, communityIdList, authToken: BotSettings.twitch.channelOauth);
    }

    public static async void UnFollow(
      string userId)
    {
      try
      {
        instance.followThrottle.SleepIfNeeded();
        await twitchApi.Users.v5.UnfollowChannelAsync(twitchChannelId, userId, authToken: BotSettings.twitch.channelOauth);
      }
      catch { }
    }

    internal static void SendWhisper(
      string username,
      string message)
    {
      instance.botClient.SendWhisper(username, message);
    }

    internal static void SendMessage(
      string message)
    {
      instance.botClient.SendMessage(message);
    }

    internal static void ExitHost()
    {
      instance.botClient.SendMessage("/unhost");
    }

    public static void RunCommercial()
    {
      instance.botClient.SendMessage("/commercial");
    }

    public static async void PostToPulse(
      string message)
    {
      var temp = await twitchApi.ChannelFeeds.v3.CreatePostAsync(BotSettings.twitch.channelUsername, message, false,
        BotSettings.twitch.channelOauth);
      //var temp = twitchApi.ChannelFeeds.v5.CreateFeedPost(
      //  twitchChannelId, message, null, BotSettings.twitch.channelUsernameOauth
      //  ).Result;
    }

    public async void DownloadFullSubList()
    {
      SqlManager.DropAllSubs();

      List<Subscription> subList = await twitchApi.Channels.v5.GetAllSubscribersAsync(twitchChannelId).ConfigureAwait(true);

      for (int i = 0; i < subList.Count; i++)
      {
        Subscription sub = subList[i];
        int tier1To3 = int.Parse(sub.SubPlan.Substring(0, 1));
        Debug.Assert(tier1To3 > 0 && tier1To3 < 4);

        SqlManager.RecordSub(sub.User.Id, tier1To3);
      }
    }

    internal async static void SetTitle(
      string title)
    {
      await twitchApi.Channels.v5.UpdateChannelAsync(twitchChannelId, title);
    }
    #endregion

    #region Read API
    public static async Task<(string title, string game)> GetChannelInfo()
    {
      Channel channel = await twitchApi.Channels.v5.GetChannelByIDAsync(twitchChannelId);
      return (channel.Status, channel.Game);
    }

    public static async Task<string[]> GetCommunity()
    {
      CommunitiesResponse community = await twitchApi.Channels.v5.GetChannelCommuntiesAsync(twitchChannelId, BotSettings.twitch.channelOauth);

      string[] nameList = new string[community.Communities.Length];
      for (int i = 0; i < community.Communities.Length; i++)
      {
        nameList[i] = community.Communities[i].Name;
      }

      return nameList;
    }

    public static string GetUserId(
      string username)
    {
      try
      {
        return twitchApi.Users.v5.GetUserByNameAsync(username).Result.Matches[0].Id;
      }
      catch { }

      return null;
    }

    public static (string displayName, string id) GetUserInfo(
     string username)
    {
      try
      {
        User user = twitchApi.Users.v5.GetUserByNameAsync(username).Result.Matches[0];
        if (user != null)
        {
          return (user.DisplayName, user.Id);
        }
      }
      catch { }

      return (null, null);
    }

    public static async Task<bool> ShouldAutoFollow(
      string userId)
    {
      Channel channel = await twitchApi.Channels.v5.GetChannelByIDAsync(userId);
      if (channel == null)
      {
        return false;
      }
      return await ShouldAutoFollow(channel);
    }

    static async Task<bool> ShouldAutoFollow(
      Channel channel)
    {
      if (channel == null)
      {
        return false;
      }

      if (string.IsNullOrWhiteSpace(channel.BroadcasterType))
      {
        DateTime? lastVideoTime = await GetLastVideoTime(channel.Id.ToString());
        if (lastVideoTime == null || lastVideoTime < DateTime.Now - TimeSpan.FromDays(90))
        {
          return false;
        }
      }

      return true;
    }

    public static async void UnfollowFollowedNonStreamers(
      int offset = 0)
    {
      UserFollows followList = await twitchApi.Users.v5.GetUserFollowsAsync(
        twitchChannelId,
        offset: offset);

      for (int i = 0; i < followList.Follows.Length; i++)
      {
        UserFollow follow = followList.Follows[i];
        try
        {
          if (await ShouldAutoFollow(follow.Channel) == false)
          {
            SqlManager.SetHasAutoFollowed(follow.Channel.Id.ToString(), false);
            UnFollow(follow.Channel.Id.ToString());
            await DebugPrintChannel("Unfollowing", follow.Channel);
          }
          else
          {
            // TODO remove this
            SqlManager.SetHasAutoFollowed(follow.Channel.Id.ToString(), true);
          }
        }
        catch (Exception e)
        {
          string t = e.ToString();
        }
      }
      if (followList.Follows.Length > 0
        && followList.Total - offset > 0)
      {
        UnfollowFollowedNonStreamers(offset + followList.Follows.Length);
      }
      else
      {
        Console.WriteLine("Unfollow bot complete");
      }
    }

    private static async Task DebugPrintChannel(string prefix, Channel channel)
    {
      const string nullString = "null";
      Console.WriteLine($"{prefix} {channel.DisplayName} {channel.Followers} {channel.Views} broadcasterType:{channel.BroadcasterType ?? nullString} lastVideo:{(await GetLastVideoTime(channel.Id.ToString()))?.ToString() ?? nullString}");
    }

    public async Task<string> GetPageOfFollowers(
      Action<string> forEach,
      string resumeFromCursur = null)
    {
      ChannelFollowers followList = await twitchApi.Channels.v5.GetChannelFollowersAsync(twitchChannelId, cursor: resumeFromCursur);
      if (followList == null)
      {
        return null;
      }
      for (int i = 0; i < followList.Follows.Length; i++)
      {
        IFollow follow = followList.Follows[i];
        forEach(follow.User.Id.ToString());
      }
      string cursur = followList.Cursor;
      return cursur;
    }

    internal async void GetAllFollowers(
      Action<string> forEach,
      string resumeFromCursur = null)
    {
      string cursur = await GetPageOfFollowers(forEach, resumeFromCursur);
      if (string.IsNullOrWhiteSpace(cursur) == false)
      {
        GetAllFollowers(forEach, cursur);
      }
    }

    public static string GetDisplayName(
      string userId)
    {
      try
      {
        User user = twitchApi.Users.v5.GetUserByIDAsync(userId).Result;
        if (user != null)
        {
          return user.DisplayName;
        }
      }
      catch { }

      return null;
    }

    public static async Task<DateTime?> GetLastVideoTime(
      string userId)
    {
      ChannelVideos videos = await twitchApi.Channels.v5.GetChannelVideosAsync(userId, 1);
      if (videos == null)
      {
        return null;
      }
      if (videos.Videos.Length > 0)
      {
        return videos.Videos[0].CreatedAt;
      }

      return null;
    }
    #endregion
  }
}
