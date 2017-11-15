using System;
using System.Collections.Generic;
using System.Threading;

namespace HD
{
  public class TwitchFollows
  {
    #region Data
    Thread thread;
    public Action<string> OnFollow;
    #endregion

    #region Init
    public TwitchFollows()
    {
      thread = new Thread(Run);
      thread.Start();
    }

    public void Disconnect()
    {
      thread?.Abort();
    }
    #endregion

    #region Events
    async void Run()
    {
      while (true)
      {
        Thread.Sleep(TimeSpan.FromMinutes(10));
        string fu = await TwitchController.instance.GetPageOfFollowers(OnFollow);
      }
    }
    #endregion
  }
}
