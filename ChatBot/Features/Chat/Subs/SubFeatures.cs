using System;
using System.Diagnostics;

namespace HD
{
  public class SubFeatures : IBotFeature
  {
    #region Data
    public static readonly SubFeatures instance = new SubFeatures();

    bool isCountingSubs;
    #endregion

    #region Init
    SubFeatures()
    {
      Debug.Assert(instance == null || instance == this);
    }

    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand("!subcount", null,
        UserLevel.Everyone, GetSubCount));

      TwitchController.instance.onSub += OnSub;

      TimeFeatures.instance.onGoLive += Instance_onGoLive;

      RefreshSubList();

      // then for each: SqlManager.RecordSub(sub.User.Id, tier1To3);
    }
    #endregion

    #region Events
    void Instance_onGoLive(string obj)
    {
      RefreshSubList();
    }

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
      SubsTable.instance.RecordSub(user.userId, tier1To3);
    }
    #endregion

    #region API
    void GetSubCount(
      Message message)
    {
      if (isCountingSubs == false)
      {
        const int nextMilestone = 200;
        int currentSubCount = SubsTable.instance.GetTotalSubCount();
        int remaining = nextMilestone - currentSubCount;

        TwitchController.instance.SendMessage($"We have {currentSubCount} subs!  Only {remaining} till our next emote.");
      }
      else
      {
        TwitchController.instance.SendMessage("I'm still running numbers... check back in a few.");
      }
    }

    async void RefreshSubList()
    {
      isCountingSubs = true;
      SubsTable.instance.DropAllSubs();
      await TwitchController.instance.DownloadFullSubList((sub, tier) =>
      {
        SubsTable.instance.RecordSub(sub.userId, tier);
      });
      isCountingSubs = false;
    }
    #endregion
  }
}
