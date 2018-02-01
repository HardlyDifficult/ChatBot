using System;
using System.Collections.Generic;
using System.Media;
using System.Timers;

namespace HD
{
  public class ModPowers : IBotFeature
  {
    public event Action onModCancelRequest;

    Timer timerToStopStream;

    Throttle pokeThrottle = new Throttle(TimeSpan.FromMinutes(1));

    ModPowers() { }

    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand(
        "!stopstream",
        "This will stop OBS / end the stream.  Only use if HD left it running by mistake.",
        UserLevel.Mods,
        onStopStreamRequested));

      CommandFeatures.instance.Add(new DynamicCommand(
        "!cancel",
        "This will cancel an accidental command such as !stopstream.",
        UserLevel.Mods,
        onCancel));

      CommandFeatures.instance.Add(new DynamicCommand(
        "!poke",
        "Play a sound to get HD's attention.  Do not abuse.  tx",
        UserLevel.Mods,
        onPoke));
    }

    #region Events
    void onPoke(
      Message message)
    {
      if(pokeThrottle.isReady == false)
      {
        return;
      }
      pokeThrottle.SetLastUpdateTime();

      SoundPlayer player = new SoundPlayer(
        @"d:\\StreamAssets\\HeyLISTEN.wav");
      player.Play();
    }

    void onStopStreamRequested(
      Message message)
    {
      StopStopStreamTimer();

      TwitchController.instance.SendMessage($"Stream is shutting down in 60 seconds!  Thanks {message.user.displayName}.  If this was a mistake, mods say !cancel");
      timerToStopStream = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
      timerToStopStream.AutoReset = false;
      timerToStopStream.Elapsed += Timer_Elapsed;
      timerToStopStream.Start();
    }

    void onCancel(
      Message message)
    {
      StopStopStreamTimer();
      onModCancelRequest?.Invoke();
    }

    void Timer_Elapsed(
      object sender,
      ElapsedEventArgs e)
    {
      Obs.StopStreaming();
    }
    #endregion

    void StopStopStreamTimer()
    {
      if (timerToStopStream != null)
      {
        try
        {
          timerToStopStream.Stop();
          timerToStopStream.Close();
          TwitchController.instance.SendMessage($"Shutdown terminated.");
        }
        catch { }
        timerToStopStream = null;
      }
    }
  }
}
