using System;

namespace HD
{
  public static class TwitchLibExtensions
  {
    /// <summary>
    /// Returns 1, 2, or 3
    /// </summary>
    public static int GetTier(
      this TwitchLib.Enums.SubscriptionPlan plan)
    {
      int tier1To3;
      switch (plan)
      {
        default:
        case TwitchLib.Enums.SubscriptionPlan.NotSet:
        case TwitchLib.Enums.SubscriptionPlan.Prime:
        case TwitchLib.Enums.SubscriptionPlan.Tier1:
          tier1To3 = 1;
          break;
        case TwitchLib.Enums.SubscriptionPlan.Tier2:
          tier1To3 = 2;
          break;
        case TwitchLib.Enums.SubscriptionPlan.Tier3:
          tier1To3 = 3;
          break;
      }

      return tier1To3;
    }
  }
}
