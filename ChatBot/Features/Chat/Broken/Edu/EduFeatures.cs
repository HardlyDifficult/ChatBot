//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;



// TODO: EDU
//   - !edu: what's up next
//   - fundraiser goal (should extract into a separate fundraiser feature)

//namespace HD
//{
//  /// <summary>
//  ///  - Subs, bits should auto add to value.
//  ///    
//  /// 
//  /// !edu: 
//  /// Every $200 raised we make another tutorial.  Next up: Quaternions with $189.68 to go.  !edugoal for details.
//  /// Mod runs "!edu Quaternions" to set topic
//  /// Mod runs "!edu .31" to add a tip (support negative as well)
//  /// 
//  /// !backlog = I owe you blah.
//  /// 
//  /// DB
//  ///  - amount raised so far in pennies.
//  ///  - next topic
//  /// 
//  /// </summary>
//  public class Edu : IBotFeature
//  {
//    #region Constants
//    const string dbKeyTopic = "EduTopic";
//    const string dbKeyPennies = "EduPennies";
//    #endregion

//    #region Init
//    void IBotFeature.Init()
//    {

//      CommandFeatures.instance.Add(new DynamicCommand("!edu", "TODO", UserLevel.Everyone, Edu.OnCommand));

//      //dynamicCommandList.Add(new DynamicCommand("!edu", "!edu Message", UserLevel.Mods, UpdateEdu));
//    }
//    #endregion

//    internal static void OnCommand(
//      Message message)
//    {
//      return;

//      //if(message.message.StartsWith("!edugoal", StringComparison.InvariantCultureIgnoreCase))
//      //{
//      //  return;
//      //}
//      //if (message.userLevel >= UserLevel.Mods)
//      //{
//      //  string commandText = message.message.GetAfter(" ");
//      //  if (string.IsNullOrWhiteSpace(commandText) == false)
//      //  {
//      //    if (decimal.TryParse(commandText, out decimal result))
//      //    {
//      //      result *= 100;
//      //      int pennies = (int)result;
//      //      OnMoney(message, pennies);
//      //      return;
//      //    }
//      //    else
//      //    {
//      //      SqlManager.SetStringValue(dbKeyTopic, commandText);
//      //    }
//      //  }
//      //}

//      //PostGoal(message, 0);
//    }




//public void OnSub(
//      TwitchUser user,
//      int tier1To3,
//      int months)
//{

//  switch (tier1To3)
//  {
//    default:
//    case 1:
//      Edu.OnMoney(user, 499);
//      break;
//    case 2:
//      Edu.OnMoney(user, 999);
//      break;
//    case 3:
//      Edu.OnMoney(user, 2499);
//      break;
//  }

//}


//    static void PostGoal(
//      Message message,
//      int penniesAddedRecently)
//    {
//      return;
//      //float amountRemaining = 500f - (float)SqlManager.GetLongValue(dbKeyPennies) / 100f;
//      //string topic = SqlManager.GetStringValue(dbKeyTopic);

//      //string prefix;

//      //if(penniesAddedRecently == 0)
//      //{
//      //  prefix = "Every 100 subs or $500 raised we make another tutorial.";
//      //} else
//      //{
//      //  float amountAdded = penniesAddedRecently / 100f;
//      //  prefix = $"+{amountAdded:C}!";
//      //}

//      //string eduMessage = $"{prefix} Next up: {topic} with {amountRemaining:C} to go.  !edugoal for details.";

//      //if (SqlManager.CooldownIsReadyForIntKey(message.userLevel, dbKeyPennies))
//      //{
//      //  TwitchController.instance.SendMessage(eduMessage);
//      //}
//      //else
//      //{
//      //  TwitchController.instance.SendWhisper(message.displayName, eduMessage);
//      //}
//    }

//    public static void OnMoney(
//      TwitchUser message,
//      int pennies)
//    {
//      return;
//      //SqlManager.SetLongValue(dbKeyPennies, pennies + (int)SqlManager.GetLongValue(dbKeyPennies));
//      //PostGoal(message, pennies);
//    }



//      // Move to EDU
//      if (message.bits > 0)
//      {
//        Edu.OnMoney(message.user, message.bits);
//      }


//  void UpdateEdu(
//    Message message)
//  {
//    string eduText = message.message.GetAfter(" ");
//    if (string.IsNullOrWhiteSpace(eduText))
//    {
//      return;
//    }
//    eduText.Trim();
//    if (eduText.StartsWith("="))
//    {
//      eduText = eduText.Substring(0);
//      eduText = eduText.Trim();
//    }

//    SqlManager.SetStringValue("EDU", eduText);
//  }
//}
//}
