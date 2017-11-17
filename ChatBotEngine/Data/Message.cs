using System;
using TwitchLib.Models.Client;

namespace HD
{
  public class Message
  {
    #region Data
    public readonly TwitchUser user;
    public string message;
    public readonly bool isWhisper;
    public readonly int bits;
    #endregion

    #region Init
    public Message(
      TwitchUser user,
      string message,
      bool isWhisper,
      int bits)
    {
      this.user = user;
      this.message = message;
      this.isWhisper = isWhisper;
      this.bits = bits;
    }

    public Message(
      ChatMessage chatMessage)
      : this(new TwitchUser(chatMessage),
          chatMessage.Message,
          false,
          chatMessage.Bits) {}

    public Message(
      WhisperMessage whisperMessage)
      : this(new TwitchUser(whisperMessage),
          whisperMessage.Message,
          true,
          0) { }
    #endregion
  }
}
