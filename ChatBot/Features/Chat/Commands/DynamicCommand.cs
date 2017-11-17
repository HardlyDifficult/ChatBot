using System;
using System.Collections.Generic;

namespace HD
{
  /// <summary>
  /// TODO trim help message, but then include other newlines
  /// </summary>
  public class DynamicCommand
  {
    #region Data
    public readonly string command;
    public readonly UserLevel minimumUserLevel;
    public readonly string helpMessage;
    readonly Action<Message> onCommand;
    #endregion

    #region Init
    public DynamicCommand(
      string command,
      string helpMessage,
      UserLevel minimumUserLevel,
      Action<Message> onCommand)
    {
      this.command = command;
      this.helpMessage = helpMessage;
      this.minimumUserLevel = minimumUserLevel;
      this.onCommand = onCommand;
    }
    #endregion

    #region Events
    public void OnMessage(
      Message message)
    {
      if (message.user.userLevel < minimumUserLevel)
      {
        return;
      }

      if (message.message.StartsWith(command, StringComparison.InvariantCultureIgnoreCase))
      {
        onCommand?.Invoke(message);
      }
    }
    #endregion
  }
}
