using System;
using System.Diagnostics;
using TwitchLib.Models.Client;

namespace HD
{
  public class TwitchUser
  {
    #region Data
    public readonly string userId;
    public readonly string displayName;
    public readonly UserLevel userLevel;
    #endregion

    #region Init
    // TODO SqlManager.StoreUser(userId, displayName, userLevel);
    public TwitchUser(
      string userId,
      string displayName,
      UserLevel userLevel)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(userId) == false);
      Debug.Assert(string.IsNullOrWhiteSpace(displayName) == false);

      this.userId = userId;
      this.displayName = displayName;
      this.userLevel = userLevel;
    }

    public TwitchUser(
      ChatMessage chatMessage)
      : this(chatMessage.UserId, chatMessage.DisplayName, UserLevelHelpers.Get(chatMessage)) { }

    public TwitchUser(
      WhisperMessage whisperMessage)
      : this(whisperMessage.UserId, whisperMessage.DisplayName, UserLevelHelpers.Get(whisperMessage)) { }

    public static TwitchUser FromName(
      string username)
    {
      string userId = TwitchController.instance.GetUserId(username);
      return new TwitchUser(userId, username, UserLevelHelpers.Get(userId));
    }
    #endregion

    #region Operations
    public static bool operator ==(
      TwitchUser a,
      TwitchUser b)
    {
      return a?.userId == b?.userId;
    }

    public static bool operator !=(
      TwitchUser a,
      TwitchUser b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      if(obj is TwitchUser user)
      {
        return userId == user.userId;
      }

      return false;
    }

    public override int GetHashCode()
    {
      return userId.GetHashCode();
    }
    #endregion
  }
}
