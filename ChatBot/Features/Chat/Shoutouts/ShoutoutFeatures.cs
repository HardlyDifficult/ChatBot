using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HD
{
  public class ShoutoutFeatures : IBotFeature
  {
    #region Data
    public static readonly ShoutoutFeatures instance = new ShoutoutFeatures();
    #endregion

    #region Init
    ShoutoutFeatures()
    {
      Debug.Assert(instance == null || instance == this);
    }

    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!shoutout",
        helpMessage: @"
Give shoutout: !shoutout @Username
Create shoutout: !shoutout @Username = New shoutout message
          ",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnCommandShoutout));

      TwitchController.instance.onHosted += OnHosted;
      TwitchController.instance.onHosting += OnHosting;
    }
    #endregion

    #region Events
    void OnHosting(
      TwitchUser channelWeAreHosting, 
      int viewerCount)
    {
      (string streamerName, string shoutoutMessage) = GetShoutoutMessage(channelWeAreHosting);
      StringBuilder builder = new StringBuilder();
      builder.Append("Now hosting ");
      builder.Append(streamerName);
      builder.Append(". ");
      if (shoutoutMessage != null)
      {
        builder.Append(shoutoutMessage);
        builder.Append(" ");
      }
      builder.Append("Join us @ twitch.tv/");
      builder.Append(streamerName);
      TwitchController.instance.SendMessage(builder.ToString());
    }

    void OnHosted(
     TwitchUser channelHostingUs, 
     bool isAutohost, 
     int? viewerCount)
    {
      // TODO put this back AutoFollow(TwitchController.instance.GetUserId(displayName));
      if (isAutohost)
      {
        return;
      }
      if (viewerCount < 10)
      { // Hide counts less than 10
        viewerCount = null;
      }
      (string hosterName, string shoutoutMessage) = GetShoutoutMessage(channelHostingUs);
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
      message.Append(" Drop a follow @ twitch.tv/");
      message.Append(hosterName);
      message.Append(" hardlyHype hardlyHype");
      TwitchController.instance.SendMessage(message.ToString());
    }
    #endregion

    #region Commands
    void OnCommandShoutout(
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
        TwitchUser userToShoutout = TwitchUser.FromName(usernameToShout);
        SqlManager.SetShoutoutMessage(userToShoutout.userId, newShoutoutMessage);
      }
      {
        (string streamerName, string shoutoutMessage) = GetShoutoutMessage(TwitchUser.FromName(usernameToShout));
        if (streamerName == null)
        { // I don't know who you are
          TwitchController.instance.SendWhisper(message.user.displayName, $"Fail! Who's {usernameToShout}??");
          return;
        }
        if (shoutoutMessage == null)
        {
          shoutoutMessage = "Known streamer -> ";
        }
        TwitchController.instance.SendMessage($"{shoutoutMessage} Drop a follow @ twitch.tv/{streamerName}");
      }
    }
    #endregion

    #region Private Read
    (string streamerName, string shoutoutMessage) GetShoutoutMessage(
      TwitchUser user)
    {
      if(user == null)
      {
        return (null, null);
      }

      string shoutoutMessage = SqlManager.GetShoutoutMessage(user.userId);
      if (shoutoutMessage == null)
      {
        return (user.displayName, null);
      }

      return (user.displayName, shoutoutMessage);
    }
    #endregion
  }
}
