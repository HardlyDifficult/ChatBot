using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HD
{
  /// <summary>
  ///  - Subs, bits should auto add to value.
  ///    
  /// 
  /// !edu: 
  /// Every $200 raised we make another tutorial.  Next up: Quaternions with $189.68 to go.  !edugoal for details.
  /// Mod runs "!edu Quaternions" to set topic
  /// Mod runs "!edu .31" to add a tip (support negative as well)
  /// 
  /// !backlog = I owe you blah.
  /// 
  /// DB
  ///  - amount raised so far in pennies.
  ///  - next topic
  /// 
  /// </summary>
  public static class Edu
  {
    const string dbKeyTopic = "EduTopic";
    const string dbKeyPennies = "EduPennies";

    internal static void OnCommand(
      Message message)
    {
      return;

      //if(message.message.StartsWith("!edugoal", StringComparison.InvariantCultureIgnoreCase))
      //{
      //  return;
      //}
      //if (message.userLevel >= UserLevel.Mods)
      //{
      //  string commandText = message.message.GetAfter(" ");
      //  if (string.IsNullOrWhiteSpace(commandText) == false)
      //  {
      //    if (decimal.TryParse(commandText, out decimal result))
      //    {
      //      result *= 100;
      //      int pennies = (int)result;
      //      OnMoney(message, pennies);
      //      return;
      //    }
      //    else
      //    {
      //      SqlManager.SetStringValue(dbKeyTopic, commandText);
      //    }
      //  }
      //}

      //PostGoal(message, 0);
    }

    static void PostGoal(
      Message message,
      int penniesAddedRecently)
    {
      return;
      //float amountRemaining = 500f - (float)SqlManager.GetLongValue(dbKeyPennies) / 100f;
      //string topic = SqlManager.GetStringValue(dbKeyTopic);

      //string prefix;

      //if(penniesAddedRecently == 0)
      //{
      //  prefix = "Every 100 subs or $500 raised we make another tutorial.";
      //} else
      //{
      //  float amountAdded = penniesAddedRecently / 100f;
      //  prefix = $"+{amountAdded:C}!";
      //}

      //string eduMessage = $"{prefix} Next up: {topic} with {amountRemaining:C} to go.  !edugoal for details.";

      //if (SqlManager.CooldownIsReadyForIntKey(message.userLevel, dbKeyPennies))
      //{
      //  TwitchController.instance.SendMessage(eduMessage);
      //}
      //else
      //{
      //  TwitchController.instance.SendWhisper(message.displayName, eduMessage);
      //}
    }

    public static void OnMoney(
      TwitchUser message,
      int pennies)
    {
      return;
      //SqlManager.SetLongValue(dbKeyPennies, pennies + (int)SqlManager.GetLongValue(dbKeyPennies));
      //PostGoal(message, pennies);
    }
  }
}
