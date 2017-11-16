using System;
using System.Data.Common;
using System.Text;

namespace HD
{
  public class Uptime : IBotFeature
  {
    const string uptimeKey = "Uptime";

    void IBotFeature.Init()
    {
      BotLogic.Add(new DynamicCommand(
        command: "!uptime",
        helpMessage: null,
        minimumUserLevel: UserLevel.Everyone,
        onCommand: SendUptime));
    }

    void SendUptime(
      Message message)
    {
      TimeSpan? uptime = GetUptime();
      if (uptime == null)
      { // Offline TODO issue ETA command
        return;
      }

      StringBuilder builder = new StringBuilder();
      builder.Append("Uptime: ");
      builder.Append(uptime.Value.ToShortTimeString());
      TimeSpan previousUptimeToday = GetPreviousUptimeToday();
      if (previousUptimeToday.Ticks > 0)
      {
        TimeSpan totalUptimeToday = previousUptimeToday + uptime.Value;
        builder.Append(". Total today: ");
        builder.Append(totalUptimeToday.ToShortTimeString());
      }
      builder.Append(". Current time in Seattle: ");
      builder.Append(DateTime.Now.ToShortTimeString());

      bool isCooldownReady = CooldownTable.instance.IsReady(uptimeKey);
      if (BotLogic.SendMessageOrWhisper(message, builder.ToString(), isCooldownReady))
      {
        SqlManager.SetLastSentForKey(uptimeKey);
      }
    }

    public TimeSpan? GetUptime()
    {
      return StreamHistoryTable.instance.GetUptime();
    }

    public TimeSpan GetPreviousUptimeToday()
    {
      return StreamHistoryTable.instance.GetPreviousUptimeSince(DateTime.Now.Date);
    }
























    //public static TimeSpan GetPreviousUptimeToday()
    //{
    //  snhtaoeu

    //  // TODO add start time, don't trust twitch api uptime... 
    //  DateTime now = DateTime.Now;
    //  SQLiteCommand command = new SQLiteCommand("select sum(TimeStreamedInTicks) from Uptime where StreamEndtimeInTicks>=@ThisMorning and StreamEndtimeInTicks<=@Tonight", dbConnection);
    //  command.Parameters.Add(new SQLiteParameter("@ThisMorning", now.Date.Ticks));
    //  command.Parameters.Add(new SQLiteParameter("@Tonight", (now.Date + TimeSpan.FromDays(1)).Ticks));
    //  try
    //  {
    //    return new TimeSpan((long)command.ExecuteScalar());
    //  }
    //  catch { }
    //  return TimeSpan.Zero;
    //}


    //internal static void SetUptime(
    //  DateTime streamEnded,
    //  TimeSpan? streamLength)
    //{
    //  if (streamLength == null)
    //  {
    //    return;
    //  }

    //  SQLiteCommand command = new SQLiteCommand(
    //    "insert into Uptime (StreamEndtimeInTicks, TimeStreamedInTicks) values (@StreamEndtimeInTicks, @TimeStreamedInTicks)", dbConnection);
    //  command.Parameters.Add(new SQLiteParameter("@StreamEndtimeInTicks", streamEnded.Ticks));
    //  command.Parameters.Add(new SQLiteParameter("@TimeStreamedInTicks", streamLength.Value.Ticks));
    //  command.ExecuteNonQuery();
    //}
  }
}
