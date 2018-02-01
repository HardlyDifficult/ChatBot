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
using TwitchLib.Models.API.v5.Search;

namespace HD
{
  /// <summary>
  /// This is a facade to TwitchLib
  /// </summary>
  public class TwitchController
  {
    #region Data
    public static readonly TwitchController instance = new TwitchController();
    
    public delegate void OnHosting(TwitchUser channelWeAreHosting, int viewerCount);
    public event OnHosting onHosting;

    public delegate void OnHosted(TwitchUser channelHostingUs, bool isAutohost, int? viewerCount);
    public event OnHosted onHosted;

    public delegate void OnJoinChat(TwitchUser userJoining);
    public event OnJoinChat onJoinChat;

    public delegate void OnMessage(Message message);
    /// <summary>
    /// Only used when you need to update a message before others respond.
    /// </summary>
    public event OnMessage onMessageFirstPass;

    /// <summary>
    /// Fired for chat and whisper messages.
    /// </summary>
    public event OnMessage onMessage;

    public delegate void OnSub(TwitchUser user, int tier, int months);
    public event OnSub onSub; 

    public delegate void ChannelInfoChange(string title, string game);
    public event ChannelInfoChange onChannelInfoChange;

    /// <summary>
    /// TODO rebuild when settings change.  If something like the channel changes, must restart app
    /// </summary>
    readonly string jtvMessagePrefix = $":jtv!jtv@jtv.tmi.twitch.tv PRIVMSG {BotSettings.twitch.channelUsername} :";

    public TwitchUser twitchChannel
    {
      get; private set;
    }

    internal readonly TwitchAPI twitchApi
      = new TwitchAPI(BotSettings.twitch.clientId, BotSettings.twitch.channelOauth);

    TwitchConnection botClient, broadcasterClient;
    #endregion

    #region Init
    public async Task Start()
    {
      Reconnect();

      string channelName = BotSettings.twitch.channelUsername.ToLower();
      User channelUser = await GetUser(channelName);
      twitchChannel = new TwitchUser(channelUser.Id, channelUser.DisplayName, UserLevel.Owner);
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
    async void OnHostingStarted(
      object sender,
      OnHostingStartedArgs e)
    {
      onHosting?.Invoke(await TwitchUser.FromName(e.TargetChannel), e.Viewers);
    }

    /// <summary>
    /// TwitchLib's host event does not detect auto host - so this does that manually.
    /// </summary>
    async void OnSendReceiveData(
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

      onHosted?.Invoke(await TwitchUser.FromName(displayNameHostingMe), isAutoHost, count);
    }

    async void OnUserJoined(
      object sender,
      OnUserJoinedArgs e)
    {
      onJoinChat?.Invoke(await TwitchUser.FromName(e.Username));
    }

    void OnMessageReceived(
      object sender,
      OnMessageReceivedArgs e)
    {
      OnMessageOrWhisper(new Message(e.ChatMessage));
    }

    void OnMessageOrWhisper(
      Message message)
    {
      onMessageFirstPass?.Invoke(message);
      onMessage?.Invoke(message);
    }

    void OnWhisperReceived(
      object sender,
      OnWhisperReceivedArgs e)
    {
      OnMessageOrWhisper(new Message(e.WhisperMessage));
    }

    async void OnReSubscriber(
      object sender,
      OnReSubscriberArgs e)
    {
      int tier1To3 = e.ReSubscriber.SubscriptionPlan.GetTier();
      onSub?.Invoke(new TwitchUser(e.ReSubscriber.UserId.ToString(), e.ReSubscriber.DisplayName,
        await UserLevelHelpers.Get(e.ReSubscriber.UserId)), tier1To3, e.ReSubscriber.Months);
    }

    async void OnNewSubscriber(
      object sender,
      OnNewSubscriberArgs e)
    {
      int tier1To3 = e.Subscriber.SubscriptionPlan.GetTier();
      onSub?.Invoke(new TwitchUser(e.Subscriber.UserId.ToString(), e.Subscriber.DisplayName,
        await UserLevelHelpers.Get(e.Subscriber.UserId)), tier1To3, 1);
    }
    #endregion

    #region Write API
    public void InjectFakeMessage(
      Message message)
    {
      OnMessageOrWhisper(message);
    }

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
      (string originalTitle, string originalGame) = await GetChannelInfo();
      if (originalGame == gameName)
      { // No change
        return;
      }

      SearchGames searchResults = await twitchApi.Search.v5.SearchGamesAsync(gameName);
      if(searchResults == null || searchResults.Games.Length == 0)
      {
        return;
      }

      gameName = searchResults.Games[0].Name;
      
      await UpdateChanelInfo(originalTitle, gameName);
    }

    public async void SetTitle(
      string title)
    {
      (string originalTitle, string originalGame) = await GetChannelInfo();
      if (originalTitle == title)
      { // No change
        return;
      }

      await UpdateChanelInfo(title, originalGame);
    }

    private async Task UpdateChanelInfo(
      string title, 
      string game)
    {
      await twitchApi.Channels.v5.UpdateChannelAsync(twitchChannel.userId, title, game);
      await Task.Run(async delegate
      {
        await Task.Delay(3000); // Wait for the change to complete

        (string newTitle, string newGame) = await GetChannelInfo();
        onChannelInfoChange?.Invoke(newTitle, newGame);
      });
    }

    /// <summary>
    /// TODO set communities is not working...?
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
      
      await twitchApi.Channels.v5.SetChannelCommunitiesAsync(twitchChannel.userId, communityIdList);
    }


    public async void UnFollow(
      string userId)
    {
      try
      {
        await twitchApi.Users.v5.UnfollowChannelAsync(twitchChannel.userId, userId,
          authToken: BotSettings.twitch.channelOauth);
      }
      catch { }
    }
    #endregion

    #region Read API
    /// <param name="callbackForEachSub">User, Tier 1-3</param>
    public async Task DownloadFullSubList(
      Action<TwitchUser, int> callbackForEachSub)
    {
      List<Subscription> subList = await twitchApi.Channels.v5.GetAllSubscribersAsync(twitchChannel.userId).ConfigureAwait(true);

      for (int i = 0; i < subList.Count; i++)
      {
        Subscription sub = subList[i];
        int tier1To3 = int.Parse(sub.SubPlan.Substring(0, 1));
        Debug.Assert(tier1To3 > 0 && tier1To3 < 4);

        callbackForEachSub(
          new TwitchUser(sub.User.Id, sub.User.DisplayName, await UserLevelHelpers.Get(sub.User.Id)),
          tier1To3);
      }
    }

    public async Task<(string title, string game)> GetChannelInfo()
    {
      Channel channel = await twitchApi.Channels.v5.GetChannelByIDAsync(twitchChannel.userId);
      return (channel.Status, channel.Game);
    }

    public async Task<string[]> GetCommunity()
    {
      CommunitiesResponse community = await twitchApi.Channels.v5.GetChannelCommuntiesAsync(twitchChannel.userId, BotSettings.twitch.channelOauth);

      string[] nameList = new string[community.Communities.Length];
      for (int i = 0; i < community.Communities.Length; i++)
      {
        nameList[i] = community.Communities[i].Name;
      }

      return nameList;
    }

    public async void GetAllFollowers(
      Action<string> forEach,
      string resumeFromCursur = null)
    {
      List<ChannelFollow> followList = await twitchApi.Channels.v5.GetAllFollowersAsync(twitchChannel.userId);

      for (int i = 0; i < followList.Count; i++)
      {
        ChannelFollow follow = followList[i];
        forEach(follow.User.DisplayName);
      }
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

    internal async Task<User> GetUser(
      string username)
    {
      try
      {
        return (await twitchApi.Users.v5.GetUserByNameAsync(username)).Matches[0];
      }
      catch { }

      return null;
    }

    internal async Task<User> GetUserById(
      string userId)
    {
      try
      {
        return (await twitchApi.Users.v5.GetUserByIDAsync(userId));
      } catch { }

      return null;
    }
    #endregion

    #region Private Read
    (string displayName, string id) GetUserInfo(
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

    string GetDisplayName(
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
    #endregion
  }
}
