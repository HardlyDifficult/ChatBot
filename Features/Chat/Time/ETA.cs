using System;
using System.Collections.Generic;
using System.Text;

namespace HD
{
  /// <summary>
  /// In DB
  ///  - 'StreamHistory' HistoryState / Time
  ///  - 'KeyIntValue' ETA value and cooldown
  ///  - 'KeyStringValue' ETA message
  /// </summary>
  public static class ETA // TODO IBotFeature
  {
    #region Data
    public static event Action<string> onGoLive;
    public static event Action<string> onGoOffline;

    const string etaKey = "ETA";

    static DateTime lastEtaPost = DateTime.Now;

    static readonly string[] dayOfWeek = new string[]
    {
      "sun",
      "mon",
      "tue",
      "wed",
      "thu",
      "fri",
      "sat",
    };
    #endregion

    #region Properties
    public static TimeSpan? timeTillNextStream
    {
      get
      {
        if (timeOfNextStream == null)
        {
          return null;
        }

        return timeOfNextStream.Value - DateTime.Now;
      }
    }

    public static DateTime? timeOfNextStream
    {
      get
      {
        if (StreamHistoryTable.instance.isLive)
        {
          return null;
        }

        long ticks = SqlManager.GetLongValue(etaKey);
        return new DateTime(ticks);
      }
      set
      {
        SqlManager.SetLongValue(etaKey, value.Value.Ticks);
      }
    }

    static bool hasCooldownPassedSinceLastPost
    {
      get
      {
        return SqlManager.CooldownIsReadyForIntKey(UserLevel.Everyone, etaKey);
      }
    }

    static bool hasJustPosted
    {
      get
      {
        return DateTime.Now - lastEtaPost < TimeSpan.FromSeconds(1);
      }
    }

    static string etaMessage
    {
      get
      {
        return SqlManager.GetStringValue(etaKey);
      }
      set
      {
        SqlManager.SetStringValue(etaKey, value ?? "");
      }
    }
    #endregion

    #region Init
    static ETA()
    {
      BotLogic.dynamicCommandList.Add(new DynamicCommand("!eta", "!eta timeSpan [= Message]", UserLevel.Mods, UpdateEta));
      BotLogic.dynamicCommandList.Add(new DynamicCommand("!live", "!live Tweet/Pulse message", UserLevel.Mods, GoLive));
      BotLogic.dynamicCommandList.Add(new DynamicCommand("!eta", null, UserLevel.Everyone, ConsiderShowingEta));

      BotLogic.onJoin += OnJoin;
    }

    [CallStaticConstructorOnStart]
    static void Dummy() { }
    #endregion

    #region Events
    static void OnJoin(
      TwitchUser user)
    {
      ConsiderShowingEta();
    }

    static void OnGoLive(
      string goLiveMessage)
    {
      onGoLive?.Invoke(goLiveMessage);
      RecordHistory(HistoryState.Live);
    }

    static void OnGoOffline(
      string etaMessage)
    {
      onGoOffline?.Invoke(etaMessage);
      RecordHistory(HistoryState.Offline);
    }
    #endregion

    #region Commands
    static void ConsiderShowingEta(
      Message message = null)
    {
      ShowEta(message, false);
    }

    static void UpdateEta(
     Message message)
    {
      (DateTime? nextStreamTime, string etaMessage) = ExtractUpdateCommand(message);
      if (nextStreamTime == null)
      {
        return;
      }
      timeOfNextStream = nextStreamTime.Value;
      ETA.etaMessage = etaMessage;

      bool includeGoodbye;
      if (StreamHistoryTable.instance.isLive)
      {
        OnGoOffline(etaMessage);
        includeGoodbye = true;
      }
      else
      {
        includeGoodbye = false;
      }

      ShowEta(message, includeGoodbye);
    }

    static void GoLive(
      Message message)
    {
      string goLiveMessage = message.message.GetAfter(" ")?.Trim();
      if (goLiveMessage?.StartsWith("=") ?? false)
      {
        goLiveMessage = goLiveMessage.Substring(1).Trim();
      }

      OnGoLive(goLiveMessage);
    }
    #endregion

    #region Private 
    static (DateTime? nextStreamTime, string message) ExtractUpdateCommand(
      Message message)
    {
      string etaString = message.message.GetAfter(" ")?.Trim();
      if (string.IsNullOrEmpty(etaString))
      {
        return (null, null);
      }

      string etaMessage = etaString.GetAfter("=");
      if (etaMessage != null)
      {
        etaString = etaString.GetBefore("=");
      }

      DateTime? nextStreamTime = CalcTime(etaString);
      return (nextStreamTime, etaMessage);
    }

    static void RecordHistory(
      HistoryState state)
    {
      StreamHistoryTable.instance.AddStreamHistory(state);
    }

    static (DateTime? eta, bool isCooldownReady) GetEta(
      Message message = null)
    {
      (long etaInTicks, bool isCooldownReady) = SqlManager.GetLongValueIfReady(message?.userLevel ?? UserLevel.Everyone, etaKey);

      if (etaInTicks == 0)
      {
        return (null, isCooldownReady);
      }

      DateTime eta = new DateTime(etaInTicks);
      return (eta, isCooldownReady);
    }

    static void ShowEta(
      Message message,
      bool includeGoodbye)
    {
      if (StreamHistoryTable.instance.isLive || hasCooldownPassedSinceLastPost == false && message == null)
      {
        return;
      }

      string response = ConstructResponse();
      bool shouldWhisper = hasJustPosted || hasCooldownPassedSinceLastPost == false && message != null && message.userLevel < UserLevel.Mods;
      lastEtaPost = DateTime.Now;
      if (shouldWhisper)
      {
        BotLogic.SendWhisper(message.displayName, response);
      }
      else
      {
        BotLogic.SendMessage(response);
        SqlManager.SetLastSentForKey(etaKey); // TODO patterns
      }

      string ConstructResponse()
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (includeGoodbye)
        {
          stringBuilder.Append("Cya next time.. ");
        }

        TimeSpan timeTillNextStream = ETA.timeTillNextStream.Value;
        if (timeTillNextStream > TimeSpan.Zero)
        {
          stringBuilder.Append("Live in ");
          stringBuilder.Append(timeTillNextStream.ToShortTimeString());
        }
        else
        {
          stringBuilder.Append("Online sooon! Nick was supposed to be here ");
          stringBuilder.Append((-timeTillNextStream).ToShortTimeString());
          stringBuilder.Append(" ago..");
        }

        if (etaMessage != null)
        {
          stringBuilder.Append(".  ");
          stringBuilder.Append(etaMessage);
        }

        return stringBuilder.ToString();
      }
    }

    static DateTime? CalcTime(
      string etaString)
    {
      { // x = mins
        if (int.TryParse(etaString, out int mins))
        {
          return DateTime.Now + TimeSpan.FromMinutes(mins);
        }
      }

      { // h:mm 
        int iColon = etaString.IndexOf(":");
        if (iColon > 0 && TimeSpan.TryParse(etaString, out TimeSpan result))
        {
          return DateTime.Now + result;
        }
      }

      { // Wed 10am (w/ or w/o 'Wed')
        // Tomorrow/Today 10am

        int amIndex = etaString.IndexOf("am", StringComparison.InvariantCultureIgnoreCase);
        int pmIndex = etaString.IndexOf("pm", StringComparison.InvariantCultureIgnoreCase);
        int maxIndex = Math.Max(amIndex, pmIndex);
        if (maxIndex > 0)
        {
          string tempString = etaString.Substring(0, maxIndex);
          int hour;
          int.TryParse(tempString.GetBefore(":"), out hour);
          if (pmIndex > 0)
          {
            hour += 12;
          }

          int minute;
          int.TryParse(tempString.GetAfter(":"), out minute);

          DateTime now = DateTime.Now;
          int day = now.Day;
          bool dayWasSpecified = false;

          string dayString = etaString.Substring(maxIndex + 2);
          if (string.IsNullOrWhiteSpace(dayString) == false)
          {
            for (int dayIndex = 0; dayIndex < dayOfWeek.Length; dayIndex++)
            {
              if (dayString.IndexOf(dayOfWeek[dayIndex], StringComparison.InvariantCultureIgnoreCase) >= 0)
              {
                day += dayIndex - (int)now.DayOfWeek ;
                dayWasSpecified = true;
                break;
              }
            }
          }

          DateTime nextStream = new DateTime(now.Year, now.Month, day, hour, minute, 0);


          if (nextStream < now)
          {
            nextStream = nextStream.AddDays(dayWasSpecified ? 7 : 1);
          }

          return nextStream;
        }
      }


      { // x hours y mins
        TimeSpan offset = TimeSpan.Zero;
        string[] tokens = etaString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i + 1 < tokens.Length; i += 2)
        {
          string number = tokens[i];
          string numberType = tokens[i + 1];
          int toMins = 0;
          if (numberType.StartsWith("m", StringComparison.InvariantCultureIgnoreCase))
          {
            toMins = 1;
          }
          else if (numberType.StartsWith("h", StringComparison.InvariantCultureIgnoreCase))
          {
            toMins = 60;
          }
          else if (numberType.StartsWith("d", StringComparison.InvariantCultureIgnoreCase))
          {
            toMins = 60 * 24;
          }

          if (double.TryParse(number, out double value))
          {
            offset += TimeSpan.FromMinutes(value * toMins);
          }
        }

        if (offset != TimeSpan.Zero)
        {
          return DateTime.Now + offset;
        }
      }


      return null;
    }
    #endregion
  }
}
