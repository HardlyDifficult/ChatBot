using System;

namespace HD
{
  public class SubFeatures : IBotFeature
  {
    #region Init
    void IBotFeature.Init()
    {
      // TODO broken dynamicCommandList.Add(new DynamicCommand("!subcount", null, UserLevel.Everyone, GetSubCount));

      TwitchController.instance.onSub += OnSub;
    }
    #endregion

    #region Events
    void OnSub(
      TwitchUser user,
      int tier1To3,
      int months)
    {
      string message = $"hardlyHype {user.displayName}";

      switch (tier1To3)
      {
        default:
        case 1:
          message += " just subscribed!";
          break;
        case 2:
          message += " double subbed!";
          break;
        case 3:
          message += " threw down a 6x sub!!!";
          break;
      }

      if (months > 1)
      {
        message += $" (for {months})";
      }

      message += " hardlyHeart";

      TwitchController.instance.SendMessage(message);
    }
    #endregion

    #region API
    // TODO
    void GetSubCount(
      Message message)
    {
      const int nextMilestone = 200;
      int currentSubCount = SubsTable.instance.GetTotalSubCount();
      int remaining = nextMilestone - currentSubCount;

      TwitchController.instance.SendMessage($"We have {currentSubCount} subs!  Only {remaining} till our next emote.");
    }
    #endregion
  }
}
