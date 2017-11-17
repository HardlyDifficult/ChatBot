using System;
using System.Collections.Generic;
using System.Text;

namespace HD
{
  class ShoutoutFeatures : IBotFeature
  {
    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!shoutout",
        helpMessage: "Give shoutout: !shoutout @Username; Create shoutout: !shoutout @Username = New shoutout message",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnShoutout));

      TwitchController.instance.onHosted += OnHosted;
      TwitchController.instance.onHosting += OnHosting;
    }

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
      builder.Append("twitch.tv/");
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
      message.Append(" twitch.tv/");
      message.Append(hosterName);
      message.Append(" hardlyHype hardlyHype");
      TwitchController.instance.SendMessage(message.ToString());
    }

    static void OnShoutout(
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
          return;
        }
        if (shoutoutMessage == null)
        {
          shoutoutMessage = "Known streamer -> ";
        }
        TwitchController.instance.SendMessage($"{shoutoutMessage} twitch.tv/{streamerName}");
      }
    }
    
    static (string streamerName, string shoutoutMessage) GetShoutoutMessage(
      TwitchUser user)
    {
      string shoutoutMessage = SqlManager.GetShoutoutMessage(user.userId);
      if (shoutoutMessage == null)
      {
        return (user.displayName, null);
      }

      return (user.displayName, shoutoutMessage);
    }
  }
}
