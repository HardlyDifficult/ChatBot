using System;
using System.Diagnostics;
using System.IO;

namespace HD
{
  public class ChannelInfoFeatures : IBotFeature
  {
    #region Data
    public static ChannelInfoFeatures instance;
    #endregion

    #region Init
    ChannelInfoFeatures()
    {
      Debug.Assert(instance == null);

      instance = this;
    }

    void IBotFeature.Init()
    {
      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!title",
        helpMessage: @"
View current info: !title
Set title: !title New Title
          ",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnCommandSetTitle));

      CommandFeatures.instance.Add(new DynamicCommand(
        command: "!setgame",
        helpMessage: @"
View current info: !title
Change game: !setgame gamedev|coding|game name
          ",
        minimumUserLevel: UserLevel.Mods,
        onCommand: OnCommandSetGame));

      TwitchController.instance.onChannelInfoChange += OnChannelInfoChange;
    }
    #endregion

    #region Events
    void OnChannelInfoChange(
      string title,
      string game)
    {
      SendModReplyWithTitle(title, game);
    }
    #endregion

    #region Commands
    /// <summary>
    /// TODO how to configure options here for other streamers
    /// gamedev = Creative/gamedevelopment
    /// coding = Creative/programming
    /// Game Name = Game Name / chill-streams
    /// </summary>
    void OnCommandSetGame(
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
        //TODO TwitchController.instance.SetCommunities("gamedevelopment", "programming", "chill-streams");
      }
      else if (game.Equals("coding", StringComparison.InvariantCultureIgnoreCase))
      {
        TwitchController.instance.SetGame("Creative");
        //TODO TwitchController.instance.SetCommunities("programming", "chill-streams");
      }
      else
      {
        TwitchController.instance.SetGame(game);
        //TODO TwitchController.instance.SetCommunities("chill-streams");
      }
    }

    void OnCommandSetTitle(
      Message message)
    {
      string title = message.message.GetAfter(" ");
      if (title != null && title.Length > 1)
      {
        TwitchController.instance.SetTitle(title);
      }
      else
      {
        RefreshChannelInfo();
      }
    }
    #endregion

    async void RefreshChannelInfo()
    {
      string[] communityList = await TwitchController.instance.GetCommunity();
      (string title, string game) = await TwitchController.instance.GetChannelInfo();
      SendChannelInfo(title, game, communityList);
    }

    async void SendModReplyWithTitle(
      string title,
      string game)
    {
      string[] communityList = await TwitchController.instance.GetCommunity();
      // TODO should have a message context
      SendChannelInfo(title, game, communityList);

      string obsMessage = new string(' ', 30);
      obsMessage += title;
      File.WriteAllText("..\\TODO.txt", obsMessage);
    }

    static void SendChannelInfo(string title, string game, string[] communityList)
    {
      BotLogic.instance.SendModReply(null, $"\"{title}\" {game} / {communityList.ToCsv()}");
    }
  }
}
