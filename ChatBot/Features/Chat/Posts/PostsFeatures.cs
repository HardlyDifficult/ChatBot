using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HD
{
  /// <summary>
  /// Tweet and Pulse
  /// </summary>
  public class PostsFeatures : IBotFeature
  {
    #region Data
    public static PostsFeatures instance;
    #endregion

    #region Init
    PostsFeatures()
    {
      Debug.Assert(instance == null);

      instance = this;
    }

    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand(
         command: "!tweet", 
         helpMessage: "!tweet message for Pulse and Twitter (if not too long)",
         minimumUserLevel: UserLevel.Mods,
         onCommand: OnCommandTweet));

      TimeFeatures.instance.onGoLive += OnGoLive;
    }
    #endregion

    #region Events
    void OnGoLive(
      string goLiveMessage)
    {
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
    #endregion

    #region Commands
    void OnCommandTweet(
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
    #endregion

    #region API
    public void SendTweetAndPulse(
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
    #endregion
  }
}
