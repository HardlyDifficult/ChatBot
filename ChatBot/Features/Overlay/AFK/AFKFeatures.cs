using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace HD
{
  public class AFKFeatures : IBotFeature
  {
    Timer countdownTimer;
    DateTime? expectedReturn;

    AFKFeatures() { }

    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand(
        "!afk",
        "Switches to the AFK screen.  Assumes 5 mins unless otherwise stated.",
        UserLevel.Mods,
        onAfk));

      CommandFeatures.instance.Add(new DynamicCommand(
        "!back",
        "Ends the AFK",
        UserLevel.Mods,
        onBack));
    }

    void onAfk(
      Message message)
    {
      StopCountdown();

      (DateTime? eta, string msg) = TimeFeatures.ExtractETAAndMessage(message);
      if (eta == null || eta.Value < DateTime.Now)
      {
        eta = DateTime.Now + TimeSpan.FromMinutes(5);
      }
      expectedReturn = eta.Value;

      countdownTimer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
      countdownTimer.AutoReset = true;
      countdownTimer.Elapsed += CountdownTimer_Elapsed;
      countdownTimer.Start();

      UpdateCountdownText();

      Obs.MuteMic(isMuted: true);
      Obs.ChangeScene(moveUp: false);
    }

    void CountdownTimer_Elapsed(
      object sender, 
      ElapsedEventArgs e)
    {
      UpdateCountdownText();
    }

    void onBack(
      Message message)
    {
      Obs.ChangeScene(moveUp: true);
      Obs.MuteMic(isMuted: false);
      StopCountdown();
    }

    void StopCountdown()
    {
      if (countdownTimer != null)
      {
        try
        {
          countdownTimer.Stop();
          countdownTimer.Close();
          countdownTimer = null;
        }
        catch { }
      }

      expectedReturn = null;
    }

    void UpdateCountdownText()
    {
      if(expectedReturn == null)
      {
        return;
      }

      TimeSpan timeTill = expectedReturn.Value - DateTime.Now;
      string message = timeTill.ToShortTimeString();
      File.WriteAllText("..\\ETACountdown.txt", message);
    }
  }
}
