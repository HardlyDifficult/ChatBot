using System;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace HD
{
  /// <summary>
  /// !eta, !uptime and related features.
  /// </summary>
  public class TimeFeatures : IBotFeature
  {
    #region Constants
    const string etaKey = "ETA";

    const string uptimeKey = "Uptime";
    #endregion

    #region Data
    public static readonly TimeFeatures instance = new TimeFeatures();

    public event Action<string> onGoLive;

    public event Action<string> onGoOffline;

    bool includeGoodbye = true;
    #endregion

    #region Properties
    /// <summary>
    /// Null if currently live or no known ETA.
    /// </summary>
    public DateTime? nextStream
    {
      get
      {
        if (StreamHistoryTable.instance.isLive)
        {
          return null;
        }

        if (KeyLongValueTable.instance.TryGetValue(etaKey, out long ticks) == false)
        {
          return null;
        }

        return new DateTime(ticks);
      }
      private set
      {
        Debug.Assert(value != null);

        KeyLongValueTable.instance.SetValue(etaKey, value.Value.Ticks);
      }
    }

    /// <summary>
    /// Null if currently live or no known ETA.
    /// </summary>
    public TimeSpan? timeTillNextStream
    {
      get
      {
        if (nextStream == null)
        {
          return null;
        }

        return nextStream.Value - DateTime.Now;
      }
    }

    public string etaMessage
    {
      get
      {
        if (KeyStringValueTable.instance.TryGetValue(etaKey, out string value) == false)
        {
          return null;
        }

        return value;
      }
      set
      {
        KeyStringValueTable.instance.SetValue(etaKey, value ?? "");
      }
    }
    #endregion

    #region Init
    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!eta",
        helpMessage: @"
!eta timeSpan [= Message]
timeSpan may be various formats including '5 mins', '5 am', or '5 am wed'
          ",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnSetEta));

      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!live",
        helpMessage: "!live [Tweet/Pulse message]",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnSetLive));

      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!eta",
        helpMessage: null,
        minimumUserLevel: UserLevel.Everyone,
        onCommand: OnShowEta));

      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!uptime",
        helpMessage: null,
        minimumUserLevel: UserLevel.Everyone,
        onCommand: OnShowUptime));

      TwitchController.instance.onJoinChat += OnJoinChat;
    }
    #endregion

    #region Events
    /// <summary>
    /// While offline, show ETA everytime someone new hops in... unless cooldown.
    /// </summary>
    void OnJoinChat(
      TwitchUser user)
    {
      ShowEta(
        message: null,
        canSwitchCommandIfOffline: false);
    }

    void OnGoLive(
      string goLiveMessage)
    {
      StreamHistoryTable.instance.AddStreamHistory(HistoryState.Live, 0);

      // TODO TwitchController.instance.DownloadFullSubList();
      TwitchController.instance.ExitHost();
      TwitchController.instance.SendMessage("Welcome back!" + (StreamHistoryTable.instance.isLive ? "" : " Stream is starting up now."));

      onGoLive?.Invoke(goLiveMessage);
    }

    void OnGoOffline(
      string etaMessage)
    {
      StreamHistoryTable.instance.AddStreamHistory(HistoryState.Offline, 0);
      includeGoodbye = true;

      onGoOffline?.Invoke(etaMessage);
    }
    #endregion

    #region Commands
    void OnShowUptime(
      Message message)
    {
      ShowUptime(
        message,
        canSwitchCommandIfOffline: true);
    }

    void OnShowEta(
     Message message)
    {
      ShowEta(
        message,
        canSwitchCommandIfOffline: true);
    }

    void OnSetEta(
     Message message)
    {
      (DateTime? nextStreamTime, string etaMessage) = ExtractUpdateCommand(message);
      if (nextStreamTime == null)
      { // Not a valid Set ETA request
        return;
      }

      SetEta(message, nextStreamTime.Value, etaMessage);
    }

    void OnSetLive(
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

    #region Write
    public void ShowUptime(
      Message message,
      bool canSwitchCommandIfOffline)
    {
      TimeSpan? uptime = StreamHistoryTable.instance.GetUptime();
      if (uptime == null)
      { // Offline
        if (canSwitchCommandIfOffline)
        {
          ShowEta(message, canSwitchCommandIfOffline: false);
        }
        return;
      }

      StringBuilder builder = new StringBuilder();
      builder.Append("Uptime: ");
      builder.Append(uptime.Value.ToShortTimeString());
      TimeSpan previousUptimeToday = StreamHistoryTable.instance.GetPreviousUptimeToday();
      if (previousUptimeToday.Ticks > 0)
      {
        TimeSpan totalUptimeToday = previousUptimeToday + uptime.Value;
        builder.Append(". Total today: ");
        builder.Append(totalUptimeToday.ToShortTimeString());
      }
      builder.Append(". Current time in Seattle: ");
      builder.Append(DateTime.Now.ToShortTimeString());

      bool isCooldownReady = CooldownTable.instance.IsReady(uptimeKey);
      if (BotLogic.instance.SendMessageOrWhisper(message, builder.ToString(), isCooldownReady))
      {
        CooldownTable.instance.SetTime(uptimeKey);
      }
    }
    #endregion

    #region Private Write
    void ShowEta(
      Message message,
      bool canSwitchCommandIfOffline)
    {
      if (StreamHistoryTable.instance.isLive || CooldownTable.instance.IsReady(etaKey) == false && message == null)
      {
        return;
      }

      string response = ConstructResponse();
      bool isCooldownReady = CooldownTable.instance.IsReady(etaKey);
      if (BotLogic.instance.SendMessageOrWhisper(message, response, isCooldownReady))
      {
        CooldownTable.instance.SetTime(etaKey);
      }

      string ConstructResponse()
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (includeGoodbye)
        {
          stringBuilder.Append("Cya next time.. ");
          includeGoodbye = false;
        }

        stringBuilder.Append(GetETAString());

        if (etaMessage != null)
        {
          stringBuilder.Append(".  ");
          stringBuilder.Append(etaMessage);
        }

        return stringBuilder.ToString();
      }
    }

    public string GetETAString()
    {
      StringBuilder stringBuilder = new StringBuilder();

      TimeSpan timeTillNextStream = this.timeTillNextStream.Value;
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

      return stringBuilder.ToString();
    }

    void SetEta(
      Message message,
      DateTime nextStreamTime,
      string etaMessage)
    {
      bool wasOffline = StreamHistoryTable.instance.isLive == false;

      if (wasOffline == false)
      {
        CommandFeatures.instance.ExecuteCommandFromAdmin("!uptime");
        CommandFeatures.instance.ExecuteCommandFromAdmin("#sellout");
      }

      this.nextStream = nextStreamTime;
      this.etaMessage = etaMessage;

      ShowEta(
        message,
        canSwitchCommandIfOffline: false);

      if (wasOffline == false)
      {
        OnGoOffline(etaMessage);
      }
    }
    #endregion

    #region Private Read
    (DateTime? nextStreamTime, string message) ExtractUpdateCommand(
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

    DateTime? CalcTime(
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

          bool dayWasSpecified = false;

          string dayString = etaString.Substring(maxIndex + 2);
          dayString.TryGetDayOfWeek(out int day);
          DateTime now = DateTime.Now;
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
