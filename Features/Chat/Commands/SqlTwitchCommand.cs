using System;
using System.Collections.Generic;

namespace HD
{
  public struct SqlTwitchCommand
  {
    #region Data
    public readonly string command, response;
    public readonly UserLevel userLevel;
    #endregion

    #region Properties
    public bool isValid
    {
      get
      {
        return command != null;
      }
    }
    #endregion

    public SqlTwitchCommand(
      string command,
      string response,
      UserLevel userLevel)
    {
      this.command = command;
      this.response = response;
      this.userLevel = userLevel;
    }
  }
}
