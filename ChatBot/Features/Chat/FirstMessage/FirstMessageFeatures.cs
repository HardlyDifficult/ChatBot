using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HD
{
  public class FirstMessageFeatures : IBotFeature
  {
    #region Data
    public bool hasSomeoneSaidSomethingSinceGoingLive;

    public event Action<Message> onFirstMessageSinceGoingLive;
    #endregion

    #region Init
    void IBotFeature.Init()
    {
      TwitchController.instance.onMessage += OnMessage;
      TimeFeatures.instance.onGoLive += OnGoLive;
    }
    #endregion

    #region Events
    void OnGoLive(
      string goLiveMessage)
    {
      hasSomeoneSaidSomethingSinceGoingLive = false;
    }

    void OnMessage(
      Message message)
    {
      if (hasSomeoneSaidSomethingSinceGoingLive == false
        && message.user != TwitchController.instance.twitchChannel)
      {
        hasSomeoneSaidSomethingSinceGoingLive = true;
        OnFirstMessage(message);
      }
    }

    void OnFirstMessage(
      Message message)
    {
      TwitchController.instance.SendMessage($"hardlyHype");
      onFirstMessageSinceGoingLive?.Invoke(message);
    }
    #endregion
  }
}
