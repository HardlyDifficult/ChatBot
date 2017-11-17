using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace HD
{
  public class BotLogic
  {
    #region Data
    public static readonly BotLogic instance = new BotLogic();
    #endregion

    #region Init
    BotLogic()
    {
      SchemaTable.UpdateTables();

      List<IBotFeature> featureList = ReflectionHelpers.CreateOneOfEach<IBotFeature>();
      for (int i = 0; i < featureList.Count; i++)
      {
        featureList[i].Init();
      }

      TwitchController.instance.onMessageFirstPass += OnMessageFirstPass;
    }

    public void Start()
    {
      TwitchController.instance.Start();
    }

    public void Stop()
    {
      TwitchController.instance.Stop();
    }
    #endregion

    #region Events
    void OnMessageFirstPass(
      Message message)
    {
      if (string.IsNullOrWhiteSpace(message.message))
      {
        return;
      }

      if (message.isWhisper
        && char.IsLetter(message.message[0]))
      {
        message.message = "!" + message.message;
      }
    }
    #endregion

    #region API
    /// <summary>
    /// This will whisper the streamer as well to keep them informed.
    /// </summary>
    public void SendModReply(
      string displayName,
      string message)
    {
      if (displayName == null || displayName.Equals(BotSettings.twitch.channelUsername, StringComparison.InvariantCultureIgnoreCase) == false)
      {
        TwitchController.instance.SendWhisper(BotSettings.twitch.channelUsername, $"{displayName} -> {message}");
      }
      TwitchController.instance.SendWhisper(displayName, message);
    }

    /// <summary>
    /// Returns true if the message was sent to chat (vs whisper).
    /// </summary>
    public bool SendMessageOrWhisper(
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
        TwitchController.instance.SendWhisper(messageRespondingTo.user.displayName, message);

        return false;
      }
      else
      {
        TwitchController.instance.SendMessage(message);

        return true;
      }
    }
    #endregion
  }
}
