using System;
using System.Timers;

namespace HD
{
  class EtaInTitleFeature : IBotFeature
  {
    // TODO switch this to use async instead of threads.
    readonly Timer timer = new Timer(1000);//TODO30000);

    string streamTitle;

    void IBotFeature.Init()
    {
      //TimeFeatures.instance.onGoLive += Instance_onGoLive;
      //TimeFeatures.instance.onGoOffline += Instance_onGoOffline;

      //timer.Elapsed += Timer_Elapsed;
    }

    void Timer_Elapsed(
      object sender,
      ElapsedEventArgs e)
    {
      if (StreamHistoryTable.instance.isLive)
      {
        Stop();
        return;
      }

      if (TimeFeatures.instance.timeTillNextStream == null)
      {
        Stop();
        return;
      }

      string etaMessage = TimeFeatures.instance.GetETAString();
      if (string.IsNullOrWhiteSpace(etaMessage))
      {
        Stop();
        return;
      }

      TwitchController.instance.SetTitle($"{etaMessage}. {streamTitle}");
    }

    void Stop()
    {
      timer.Stop();
      if (streamTitle != null)
      {
        TwitchController.instance.SetTitle(streamTitle);
        streamTitle = null;
      }
    }

    async void Instance_onGoOffline(
      string obj)
    {
      streamTitle = (await TwitchController.instance.GetChannelInfo()).title;
      timer.Start();
    }

    void Instance_onGoLive(
      string obj)
    {
      Stop();
    }
  }
}
