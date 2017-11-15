using System;
using System.Collections.Generic;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;

namespace HD
{
  public class Message
  {
    #region Data
    public readonly string userId;
    public readonly string displayName;
    public readonly UserLevel userLevel;
    public string message;
    public readonly bool isWhisper;
    public readonly int bits;
    #endregion

    #region Init
    public Message(
      string userId,
      string displayName,
      UserLevel userLevel,
      string message,
      bool isWhisper,
      int bits)
    {
      this.userId = userId;
      this.displayName = displayName;
      this.userLevel = userLevel;
      this.message = message;
      this.isWhisper = isWhisper;
      this.bits = bits;

      // TODO SqlManager.StoreUser(userId, displayName, userLevel);
    }

    public Message(
      ChatMessage chatMessage)
      : this(chatMessage.UserId,
          chatMessage.DisplayName,
          UserLevelHelpers.Get(chatMessage),
          chatMessage.Message,
          false,
          chatMessage.Bits) {}

    public Message(
      WhisperMessage whisperMessage)
      : this(whisperMessage.UserId.ToString(),
          whisperMessage.DisplayName,
          UserLevelHelpers.Get(whisperMessage.UserId.ToString()),
          whisperMessage.Message,
          true,
          0) { }
    #endregion
  }
}
