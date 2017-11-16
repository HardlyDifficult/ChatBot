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
using TwitchLib.Interfaces;

namespace HD
{
  /// <summary>
  /// This is a facade to TwitchLib
  /// </summary>
  public class TwitchController
  {
    #region Data
    public static readonly TwitchController instance = new TwitchController();

    // TODO DownloadFullSubList & SqlManager.DropAllSubs();
    // then for each: SqlManager.RecordSub(sub.User.Id, tier1To3);

    public delegate void OnHosting(string channelWeAreHosting, int viewerCount);
    public OnHosting onHosting; // TODO shoutout

    public delegate void OnHosted(string channelHostingUs, bool isAutohost, int? viewerCount);
    public OnHosted onHosted; // TODO BotLogic.OnHost

    public delegate void OnJoinChat(string usernameJoining);
    public OnJoinChat onJoinChat; // TODO BotLogic.OnJoin

    public delegate void OnMessage(Message message);
    /// <summary>
    /// Fired for chat and whisper messages.
    /// </summary>
    public OnMessage onMessage; // TODO botlogic.onmessage

    public delegate void OnSub(string userId, string username, int tier, int months);
    public OnSub onSub; // TODO botlogi.onsub AND SqlManager.RecordSub

    /// <summary>
    /// TODO rebuild when settings change.  If something like the channel changes, must restart app
    /// </summary>
    readonly string jtvMessagePrefix = $":jtv!jtv@jtv.tmi.twitch.tv PRIVMSG {BotSettings.twitch.channelUsername} :";

    public string twitchChannelId
    {
      get; private set;
    }

    internal readonly TwitchAPI twitchApi
      = new TwitchAPI(BotSettings.twitch.clientId, BotSettings.twitch.channelOauth);

    TwitchConnection botClient, broadcasterClient;
    #endregion

    #region Init
    public void Start()
    {
      Reconnect();

      string channelName = BotSettings.twitch.channelUsername.ToLower();
      twitchChannelId = twitchApi.Users.v5.GetUserByNameAsync(channelName).Result.Matches[0].Id;
    }

    public void Stop()
    {
      botClient?.Disconnect();
    }

    void Reconnect()
    {
      try
      {
        botClient?.Disconnect();
        broadcasterClient?.Disconnect();
      }
      catch { }

      botClient = new TwitchConnection(BotSettings.twitch.botUsername, BotSettings.twitch.botOauth);

      botClient.OnMessageReceived += OnMessageReceived;
      botClient.OnUserJoined += OnUserJoined;
      botClient.OnWhisperReceived += OnWhisperReceived;
      botClient.OnNewSubscriber += OnNewSubscriber;
      botClient.OnReSubscriber += OnReSubscriber;

      broadcasterClient = new TwitchConnection(BotSettings.twitch.channelUsername, BotSettings.twitch.channelOauth);

      broadcasterClient.OnSendReceiveData += OnSendReceiveData;
      broadcasterClient.OnHostingStarted += OnHostingStarted;
    }
    #endregion

    #region Events
    void OnHostingStarted(
      object sender,
      OnHostingStartedArgs e)
    {
      onHosting?.Invoke(e.TargetChannel, e.Viewers);
    }

    /// <summary>
    /// TwitchLib's host event does not detect auto host - so this does that manually.
    /// </summary>
    void OnSendReceiveData(
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

      onHosted?.Invoke(displayNameHostingMe, isAutoHost, count);
    }

    void OnUserJoined(
      object sender,
      OnUserJoinedArgs e)
    {
      onJoinChat?.Invoke(e.Username);
    }

    void OnMessageReceived(
      object sender,
      OnMessageReceivedArgs e)
    {
      onMessage?.Invoke(new Message(e.ChatMessage));
    }

    void OnWhisperReceived(
      object sender,
      OnWhisperReceivedArgs e)
    {
      onMessage?.Invoke(new Message(e.WhisperMessage));
    }

    void OnReSubscriber(
      object sender,
      OnReSubscriberArgs e)
    {
      int tier1To3 = e.ReSubscriber.SubscriptionPlan.GetTier();
      onSub?.Invoke(e.ReSubscriber.UserId.ToString(), e.ReSubscriber.DisplayName, tier1To3, e.ReSubscriber.Months);
    }

    void OnNewSubscriber(
      object sender,
      OnNewSubscriberArgs e)
    {
      int tier1To3 = e.Subscriber.SubscriptionPlan.GetTier();
      onSub?.Invoke(e.Subscriber.UserId.ToString(), e.Subscriber.DisplayName, tier1To3, 1);
    }
    #endregion

    #region Write API
    public void ExitHost()
    {
      botClient.SendMessage("/unhost");
    }

    public async void PostToPulse(
      string message)
    {
      await twitchApi.ChannelFeeds.v3.CreatePostAsync(BotSettings.twitch.channelUsername, message, false,
        BotSettings.twitch.channelOauth);
    }

    public void RunCommercial()
    {
      botClient.SendMessage("/commercial");
    }

    public void SendMessage(
      string message)
    {
      botClient.SendMessage(message);
    }

    public void SendWhisper(
      string username,
      string message)
    {
      botClient.SendWhisper(username, message);
    }

    public async void SetGame(
      string gameName)
    {
      await twitchApi.Channels.v5.UpdateChannelAsync(twitchChannelId, game: gameName);
    }

    /// <summary>
    /// TODO test - does this work with multiple communities?
    /// </summary>
    public async void SetCommunities(
      params string[] communityNameList)
    {
      List<string> communityIdList = new List<string>();
      for (int i = 0; i < communityNameList.Length; i++)
      {
        Community community = await twitchApi.Communities.v5.GetCommunityByNameAsync(communityNameList[i]);
        communityIdList.Add(community.Id);
      }
      await twitchApi.Channels.v5.SetChannelCommunitiesAsync(twitchChannelId, communityIdList, 
        authToken: BotSettings.twitch.channelOauth);
    }

    public async void SetTitle(
      string title)
    {
      await twitchApi.Channels.v5.UpdateChannelAsync(twitchChannelId, title);
    }

    public async void UnFollow(
      string userId)
    {
      try
      {
        await twitchApi.Users.v5.UnfollowChannelAsync(twitchChannelId, userId, 
          authToken: BotSettings.twitch.channelOauth);
      }
      catch { }
    }
    #endregion

    #region Read API
    /// <param name="callbackForEachSub">UserId, DisplayName, Tier 1-3</param>
    public async void DownloadFullSubList(
      Action<string, string, int> callbackForEachSub)
    {
      List<Subscription> subList = await twitchApi.Channels.v5.GetAllSubscribersAsync(twitchChannelId).ConfigureAwait(true);

      for (int i = 0; i < subList.Count; i++)
      {
        Subscription sub = subList[i];
        int tier1To3 = int.Parse(sub.SubPlan.Substring(0, 1));
        Debug.Assert(tier1To3 > 0 && tier1To3 < 4);

        callbackForEachSub(sub.User.Id, sub.User.DisplayName, tier1To3);
      }
    }

    public async Task<(string title, string game)> GetChannelInfo()
    {
      Channel channel = await twitchApi.Channels.v5.GetChannelByIDAsync(twitchChannelId);
      return (channel.Status, channel.Game);
    }

    public async Task<string[]> GetCommunity()
    {
      CommunitiesResponse community = await twitchApi.Channels.v5.GetChannelCommuntiesAsync(twitchChannelId, BotSettings.twitch.channelOauth);

      string[] nameList = new string[community.Communities.Length];
      for (int i = 0; i < community.Communities.Length; i++)
      {
        nameList[i] = community.Communities[i].Name;
      }

      return nameList;
    }

    public string GetUserId(
      string username)
    {
      try
      {
        return twitchApi.Users.v5.GetUserByNameAsync(username).Result.Matches[0].Id;
      }
      catch { }

      return null;
    }

    public (string displayName, string id) GetUserInfo(
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

    public async void GetAllFollowers(
      Action<string> forEach,
      string resumeFromCursur = null)
    {
      List<ChannelFollow> followList = await twitchApi.Channels.v5.GetAllFollowersAsync(twitchChannelId);

      for (int i = 0; i < followList.Count; i++)
      {
        ChannelFollow follow = followList[i];
        forEach(follow.User.DisplayName);
      }
    }

    public string GetDisplayName(
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

    public async Task<DateTime?> GetLastVideoTime(
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
