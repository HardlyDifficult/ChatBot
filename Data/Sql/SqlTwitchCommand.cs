using System;
using System.Collections.Generic;

namespace HD
{
  public struct SqlTwitchCommand
  {
    public readonly string command, response;
    public readonly UserLevel userLevel;
    public readonly TimeSpan cooldown;
    public readonly DateTime lastSent;

    public SqlTwitchCommand(
      string command,
      string response,
      UserLevel userLevel,
      TimeSpan cooldown,
      DateTime lastSent)
    {
      this.command = command;
      this.response = response;
      this.userLevel = userLevel;
      this.cooldown = cooldown;
      this.lastSent = lastSent;
    }
  }
}
