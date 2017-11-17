using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace HD
{
  public class ChannelInfoFeatures : IBotFeature
  {
    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand("!title", "!title New Title", UserLevel.Mods, SetTitle));
      // TODO dynamicCommandList.Add(new DynamicCommand("!setgame", "!setgame gamedev|coding|game name", UserLevel.Mods, SetGame));

      TwitchController.instance.onTitleChange += OnTitleChange;
    }

    async void OnTitleChange()
    {
      string obsMessage = new string(' ', 30);
      obsMessage += (await TwitchController.instance.GetChannelInfo()).title;
      File.WriteAllText("..\\TODO.txt", obsMessage);
    }

    // TODO add a help message for SetGame
    /// <summary>
    /// gamedev = Creative/gamedevelopment
    /// coding = Creative/programming
    /// Game Name = Game Name / chill-streams
    /// </summary>
    void SetGame(
      Message message)
    {
      string game = message.message.GetAfter(" ");
      if (string.IsNullOrWhiteSpace(game))
      {
        return;
      }
      if (game.Equals("gamedev", StringComparison.InvariantCultureIgnoreCase))
      {
        TwitchController.instance.SetGame("Creative");
        TwitchController.instance.SetCommunities("gamedevelopment", "programming", "chill-streams");
      }
      else if (game.Equals("coding", StringComparison.InvariantCultureIgnoreCase))
      {
        TwitchController.instance.SetGame("Creative");
        TwitchController.instance.SetCommunities("programming", "chill-streams");
      }
      else
      {
        TwitchController.instance.SetGame(game);
        TwitchController.instance.SetCommunities("chill-streams");
      }

      SendModReplyWithTitle(message);
    }



    void SetTitle(
      Message message)
    {
      string title = message.message.GetAfter(" ");
      if (title != null && title.Length > 3)
      {
        TwitchController.instance.SetTitle(title);
      }

      SendModReplyWithTitle(message);
    }



    async void SendModReplyWithTitle(
      Message message)
    {
      Thread.Sleep(1000);
      (string title, string game) = await TwitchController.instance.GetChannelInfo();
      string[] communityList = await TwitchController.instance.GetCommunity();
      BotLogic.instance.SendModReply(message.user.displayName, $"\"{title}\" {game} / {communityList.ToCsv()}");
    }
  }
}
