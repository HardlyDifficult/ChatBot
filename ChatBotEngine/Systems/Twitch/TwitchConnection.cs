using System;
using TwitchLib;
using TwitchLib.Events.Client;

namespace HD
{
  internal class TwitchConnection
  {
    #region Data
    readonly TwitchClient client;
    readonly Throttle throttleMessage, throttleWhisper;
    #endregion

    #region Properties
    public event EventHandler<OnMessageReceivedArgs> OnMessageReceived
    {
      add
      {
        client.OnMessageReceived += value;
      }
      remove
      {
        client.OnMessageReceived -= value;
      }
    }

    ////
    //// Summary:
    ////     Fires when a whisper message is sent, returns username and message.
    //public event EventHandler<OnWhisperSentArgs> OnWhisperSent;
    ////
    //// Summary:
    ////     Fires when command (uses custom chat command identifier) is received, returns
    ////     channel, command, ChatMessage, arguments as string, arguments as list.
    //public event EventHandler<OnChatCommandReceivedArgs> OnChatCommandReceived;
    ////
    //// Summary:
    ////     Fires when command (uses custom whisper command identifier) is received, returns
    ////     command, Whispermessage.
    //public event EventHandler<OnWhisperCommandReceivedArgs> OnWhisperCommandReceived;
    ////
    //// Summary:
    ////     Fires when a new viewer/chatter joined the channel's chat room, returns username
    ////     and channel.
    public event EventHandler<OnUserJoinedArgs> OnUserJoined
    {
      add
      {
        client.OnUserJoined += value;
      }
      remove
      {
        client.OnUserJoined -= value;
      }
    }
    ////
    //// Summary:
    ////     Fires when a moderator joined the channel's chat room, returns username and channel.
    //public event EventHandler<OnModeratorJoinedArgs> OnModeratorJoined;
    ////
    //// Summary:
    ////     Fires when a moderator joins the channel's chat room, returns username and channel.
    //public event EventHandler<OnModeratorLeftArgs> OnModeratorLeft;
    ////
    //// Summary:
    ////     Fires when new subscriber is announced in chat, returns Subscriber.
    public event EventHandler<OnNewSubscriberArgs> OnNewSubscriber
    {
      add
      {
        client.OnNewSubscriber += value;
      }
      remove
      {
        client.OnNewSubscriber -= value;
      }
    }
    ////
    //// Summary:
    ////     Fires when current subscriber renews subscription, returns ReSubscriber.
    public event EventHandler<OnReSubscriberArgs> OnReSubscriber
    {
      add
      {
        client.OnReSubscriber += value;
      }
      remove
      {
        client.OnReSubscriber -= value;
      }
    }
    ////
    //// Summary:
    ////     Fires when a hosted streamer goes offline and hosting is killed.
    //public event EventHandler OnHostLeft;
    ////
    //// Summary:
    ////     Fires when Twitch notifies client of existing users in chat.
    //public event EventHandler<OnExistingUsersDetectedArgs> OnExistingUsersDetected;
    ////
    //// Summary:
    ////     Fires when a chat message is sent, returns username, channel and message.
    //public event EventHandler<OnMessageSentArgs> OnMessageSent;
    ////
    //// Summary:
    ////     Fires when a PART message is received from Twitch regarding a particular viewer
    //public event EventHandler<OnUserLeftArgs> OnUserLeft;
    ////
    //// Summary:
    ////     Fires when the joined channel quits hosting another channel.
    //public event EventHandler<OnHostingStoppedArgs> OnHostingStopped;
    ////
    //// Summary:
    ////     Fires when bot has disconnected.
    //public event EventHandler<OnDisconnectedArgs> OnDisconnected;
    ////
    //// Summary:
    ////     Forces when bot suffers conneciton error.
    //public event EventHandler<OnConnectionErrorArgs> OnConnectionError;
    ////
    //// Summary:
    ////     Fires when a channel's chat is cleared.
    //public event EventHandler<OnChatClearedArgs> OnChatCleared;
    ////
    //// Summary:
    ////     Fires when a viewer gets timedout by any moderator.
    //public event EventHandler<OnUserTimedoutArgs> OnUserTimedout;
    ////
    //// Summary:
    ////     Fires when client successfully leaves a channel.
    //public event EventHandler<OnLeftChannelArgs> OnLeftChannel;
    ////
    //// Summary:
    ////     Fires when a viewer gets banned by any moderator.
    //public event EventHandler<OnUserBannedArgs> OnUserBanned;
    ////
    //// Summary:
    ////     Fires when a list of moderators is received.
    //public event EventHandler<OnModeratorsReceivedArgs> OnModeratorsReceived;
    ////
    //// Summary:
    ////     Fires when confirmation of a chat color change request was received.
    //public event EventHandler<OnChatColorChangedArgs> OnChatColorChanged;
    ////
    //// Summary:
    ////     Fires when data is either received or sent.
    public event EventHandler<OnSendReceiveDataArgs> OnSendReceiveData
    {
      add
      {
        client.OnSendReceiveData += value;
      }
      remove
      {
        client.OnSendReceiveData -= value;
      }
    }
    ////
    //// Summary:
    ////     Fires when the joined channel begins hosting another channel.
    public event EventHandler<OnHostingStartedArgs> OnHostingStarted
    {
      add
      {
        client.OnHostingStarted += value;
      }
      remove
      {
        client.OnHostingStarted -= value;
      }
    }
    ////
    //// Summary:
    ////     Fires when a new whisper arrives, returns WhisperMessage.
    public event EventHandler<OnWhisperReceivedArgs> OnWhisperReceived
    {
      add
      {
        client.OnWhisperReceived += value;
      }
      remove
      {
        client.OnWhisperReceived -= value;
      }
    }
    ////
    //// Summary:
    ////     Fires when the library detects another channel has started hosting the broadcaster's
    ////     stream. MUST BE CONNECTED AS BROADCASTER.
    //public event EventHandler<OnBeingHostedArgs> OnBeingHosted;
    ////
    //// Summary:
    ////     Fires when connecting and channel state is changed, returns ChannelState.
    //public event EventHandler<OnChannelStateChangedArgs> OnChannelStateChanged;
    ////
    //// Summary:
    ////     Fires on logging in with incorrect details, returns ErrorLoggingInException.
    //public event EventHandler<OnIncorrectLoginArgs> OnIncorrectLogin;
    ////
    //// Summary:
    ////     Fires when client joins a channel.
    //public event EventHandler<OnJoinedChannelArgs> OnJoinedChannel;
    ////
    //// Summary:
    ////     Fires when client connects to Twitch.
    //public event EventHandler<OnConnectedArgs> OnConnected;
    ////
    //// Summary:
    ////     Fires whenever a log write happens.
    //public event EventHandler<OnLogArgs> OnLog;
    ////
    //// Summary:
    ////     Fires when client receives notice that a joined channel is hosting another channel.
    //public event EventHandler<OnNowHostingArgs> OnNowHosting;
    ////
    //// Summary:
    ////     Fires when a user state is received, returns UserState.
    //public event EventHandler<OnUserStateChangedArgs> OnUserStateChanged;
    #endregion

    #region Init
    public TwitchConnection(
      string connectAsUsername,
      string connectAsOauth)
    {
      // TODO the throttle time is high because otherwise messages are thrown out.  Who did that?!
      throttleMessage = new Throttle(TimeSpan.FromSeconds(.75));
      throttleWhisper = new Throttle(TimeSpan.FromSeconds(.75));

      client = new TwitchClient(
        new TwitchLib.Models.Client.ConnectionCredentials(connectAsUsername, connectAsOauth),
        BotSettings.twitch.channelUsername);

      // Not using this throttle because it drops messages
      //client.ChatThrottler = new TwitchLib.Services.MessageThrottler(20 / 2, TimeSpan.FromSeconds(30), true);
      //client.WhisperThrottler = new TwitchLib.Services.MessageThrottler(20 / 2, TimeSpan.FromSeconds(30), true);
      client.AutoReListenOnException = true;
      client.Connect();
    }

    public void Disconnect()
    {
      client?.Disconnect();
    }
    #endregion

    #region Write
    public void SendMessage(
      string message)
    {
      throttleMessage.SleepIfNeeded();
      client.SendMessage(message);
    }

    public void SendWhisper(
      string username,
      string messageToSend)
    {
      string[] messageList = messageToSend.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
      for (int i = 0; i < messageList.Length; i++)
      {
        string message = messageList[i].Trim();
        if(string.IsNullOrWhiteSpace(message))
        {
          continue;
        }

        string remainingMessage;
        if (message.Length > 400)
        {
          int iSpace = message.LastIndexOf(' ', 400);
          remainingMessage = message.Substring(iSpace);
          message = message.Substring(0, iSpace);
        }
        else
        {
          remainingMessage = null;
        }

        throttleWhisper.SleepIfNeeded();
        client.SendWhisper(username, message);

        if (remainingMessage != null)
        {
          System.Threading.Thread.Sleep(500);
          SendWhisper(username, remainingMessage);
        }
      }
    }
    #endregion
  }
}
